using System.Collections.Concurrent;

using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Carolina;

public interface IStateTracker
{
    Task<Result<IReadOnlyDictionary<string, object>>> GetGameState(Guid gameId);
    Guid WatchStates(string[] stateKey, Func<Dictionary<string, object>, Task> changeHandler);
    void UnwatchStates(Guid watchId);
    Dictionary<string, object> GetAllStates();
}

[Singleton]
public class StateTracker : IStateTracker
{
    private readonly IGameContextFactory _gameContextFactory;
    private readonly IChannelMapper _channelMapper;
    private readonly Task _initialized;
    private GameInfo? _currentGame;

    private readonly Dictionary<Guid, IGameStateStore> _watchedGames = new();
    private readonly Dictionary<Guid, SortedDictionary<string, object>> _states = new();
    private readonly ConcurrentDictionary<Guid, WatchedStatesChangeDetector> _changeDetectors = new();

    public StateTracker(
        ISystemStateStore systemStateStore,
        IGameDiscoveryService gameDiscoveryService,
        IGameContextFactory gameContextFactory,
        IChannelMapper channelMapper,
        ILogger<StateTracker> logger)
    {
        _gameContextFactory = gameContextFactory;
        _channelMapper = channelMapper;
        _initialized = Task.Run(async () =>
        {
            await UpdateCurrentGame();
            await InitializeGames();
        });

        systemStateStore.CurrentGameChanged += (_, _) => UpdateCurrentGame();
        gameDiscoveryService.GamesListChanged += OnGamesListChanged;

        return;

        async Task UpdateCurrentGame()
        {
            try
            {
                var currentGameResult = await systemStateStore.GetCurrentGame();

                if (currentGameResult is Success<GameInfo> currentGame)
                    _currentGame = currentGame.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while trying to get current game");
            }
        }

        async Task InitializeGames()
        {
            foreach (var game in await gameDiscoveryService.GetGames())
            {
                AddGame(game);
            }
        }
    }

    public async Task<Result<IReadOnlyDictionary<string, object>>> GetGameState(Guid gameId)
    {
        await _initialized;

        return
            _states.TryGetValue(gameId, out var state)
                ? Result.Succeed<IReadOnlyDictionary<string, object>>(state.AsReadOnly())
                : Result<IReadOnlyDictionary<string, object>>.Fail<GameNotFoundError>();
    }

    public Guid WatchStates(string[] stateKeys, Func<Dictionary<string, object>, Task> changeHandler)
    {
        var watchId = Guid.NewGuid();

        _changeDetectors[watchId] = new WatchedStatesChangeDetector(stateKeys, changeHandler);

        var allStates = GetAllStates();
        _ = _changeDetectors[watchId].ProcessChange(allStates, allStates.Keys.ToArray());

        return watchId;
    }

    public void UnwatchStates(Guid watchId) =>
        _changeDetectors.Remove(watchId, out _);

    public Dictionary<string, object> GetAllStates() =>
        _states.SelectMany(x => x.Value).GroupBy(x => x.Key).Select(g => g.First()).ToDictionary(x => x.Key, x => x.Value);

    private void OnGamesListChanged(object? sender, GamesListChangedEventArgs e)
    {
        var removedGames = _watchedGames.Keys.Except(e.Games.Select(g => g.Id)).ToArray();
        var addedGames = e.Games.ExceptBy(_watchedGames.Keys, l => l.Id).ToArray();

        foreach (var removedGame in removedGames)
        {
            _watchedGames.Remove(removedGame);
        }

        foreach (var addedGame in addedGames)
        {
            AddGame(addedGame);
        }
    }

    private void AddGame(GameInfo game)
    {
        var gameContext = _gameContextFactory.GetGame(game);

        _states[game.Id] = new();
        _watchedGames[game.Id] = gameContext.StateStore;

        var changeHandler = OnStateChanged(game);
        gameContext.StateStore.StateChanged += changeHandler;
        changeHandler(this, EventArgs.Empty);
    }

    private EventHandler<EventArgs> OnStateChanged(GameInfo game) => (_, _) =>
    {
        var stateStore = _watchedGames[game.Id];

        var snapshot = new StateSnapshot(
            stateStore.GetState<GameStageState>(),
            stateStore.GetState<OvertimeState>(),
            stateStore.GetState<JamClockState>(),
            stateStore.GetState<PeriodClockState>(),
            stateStore.GetState<LineupClockState>(),
            stateStore.GetState<TimeoutClockState>(),
            stateStore.GetState<IntermissionClockState>(),
            stateStore.GetState<PostGameClockState>(),
            stateStore.GetState<CurrentTimeoutTypeState>(),
            stateStore.GetState<TimeoutListState>(),
            stateStore.GetState<RulesState>(),
            new(
                stateStore.GetKeyedState<TeamScoreState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<TeamScoreState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<TripScoreState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<TripScoreState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<TeamJamStatsState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<TeamJamStatsState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<TeamTimeoutsState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<TeamTimeoutsState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<JamLineupState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<JamLineupState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<PenaltySheetState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<PenaltySheetState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<LineupSheetState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<LineupSheetState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<PenaltyBoxState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<PenaltyBoxState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<BoxTripsState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<BoxTripsState>(nameof(TeamSide.Away))
            ),
            new(
                stateStore.GetKeyedState<InjuriesState>(nameof(TeamSide.Home)),
                stateStore.GetKeyedState<InjuriesState>(nameof(TeamSide.Away))
            )
        );

        var newStates = _channelMapper.MapGameStates(snapshot, game);

        if (game.Id == _currentGame?.Id)
        {
            var gameIdentifier = $"Game({game.Id})";
            var gameKeys = newStates.Keys.Where(k => k.Contains(gameIdentifier)).ToArray();

            foreach (var key in gameKeys)
            {
                newStates[key.Replace(gameIdentifier, "CurrentGame")] = newStates[key];
            }

            newStates["ScoreBoard.CurrentGame.Game"] = game.Id;
        }

        var previousStates = _states[game.Id];

        var changedKeys =
            newStates.Keys.Difference(previousStates.Keys)
                .Concat(
                    newStates.Keys.Intersect(previousStates.Keys)
                        .Where(k => !previousStates[k].Equals(newStates[k]))
                )
                .ToArray();

        _states[game.Id] = new(newStates);

        foreach (var changeDetector in _changeDetectors.Values)
        {
            _ = changeDetector.ProcessChange(_states[game.Id], changedKeys);
        }
    };
}

public sealed class GameNotFoundError : NotFoundError;
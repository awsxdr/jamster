using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Carolina;

public interface IStateTracker
{
    Task<Result<IReadOnlyDictionary<string, object?>>> GetGameState(Guid gameId);
    void WatchState(string stateKey);
}

[Singleton]
public class StateTracker : IStateTracker
{
    private readonly IGameContextFactory _gameContextFactory;
    private readonly IChannelMapper _channelMapper;
    private readonly Task _initialized;
    private GameInfo? _currentGame = null;

    private readonly Dictionary<Guid, IGameStateStore> _watchedGames = new();
    private readonly Dictionary<Guid, SortedDictionary<string, object?>> _states = new();

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

    public async Task<Result<IReadOnlyDictionary<string, object?>>> GetGameState(Guid gameId)
    {
        await _initialized;

        return
            _states.TryGetValue(gameId, out var state)
                ? Result.Succeed<IReadOnlyDictionary<string, object?>>(state.AsReadOnly())
                : Result<IReadOnlyDictionary<string, object?>>.Fail<GameNotFoundError>();
    }

    public void WatchState(string stateKey)
    {

    }

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

        AddDefaultStates(_states[game.Id]);
        
        var allGameStates = gameContext.StateStore.GetAllStates();

        foreach (var stateKey in allGameStates.Keys)
        {
            UpdateState(stateKey, allGameStates[stateKey], gameContext.StateStore, game.Id);
            gameContext.StateStore.WatchStateByName(stateKey, s => UpdateState(stateKey, s, gameContext.StateStore, game.Id));
        }
    }

    private static void AddDefaultStates(SortedDictionary<string, object?> state)
    {
        state["ScoreBoard.Version(release)"] = "v2025.10";
        state["ScoreBoard.Version(release.commit)"] = "b9a33ac56d7274f93c2889d22214f28d56017605";
        state["ScoreBoard.Version(release.host)"] = "localhost";
        state["ScoreBoard.Version(release.time)"] = "20260420190905";
        state["ScoreBoard.Version(release.user)"] = "frank";
    }

    private Task UpdateState(string stateKey, object state, IGameStateStore stateStore, Guid gameId)
    {
        TeamSide? teamSide =
            stateKey.EndsWith("_Home") ? TeamSide.Home
            : stateKey.EndsWith("_Away") ? TeamSide.Away
            : null;

        var rules = stateStore.GetState<RulesState>().Rules;

        var mappedStates = state switch
        {
            GameStageState s => _channelMapper.Map(s, gameId),
            OvertimeState s => _channelMapper.Map(s, gameId),
            JamClockState s => _channelMapper.Map(s, gameId),
            PeriodClockState s => _channelMapper.Map(s, rules, gameId),
            LineupClockState s => _channelMapper.Map(s, gameId),
            TimeoutClockState s => _channelMapper.Map(s, gameId),
            IntermissionClockState s => _channelMapper.Map(s, gameId),
            PostGameClockState s => _channelMapper.Map(s, gameId),
            TeamScoreState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, gameId),
            TripScoreState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, gameId),
            TeamJamStatsState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, gameId),
            TeamTimeoutsState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, rules, gameId),
            JamLineupState s when teamSide is not null => _channelMapper.Map(s, stateStore.GetKeyedState<TeamDetailsState>(((TeamSide)teamSide).ToString()), (TeamSide)teamSide, gameId),
            CurrentTimeoutTypeState s => _channelMapper.Map(s, gameId),
            TeamDetailsState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, gameId),
            ScoreSheetState s => _channelMapper.Map(s, stateStore.GetKeyedState<ScoreSheetState>(teamSide == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home)), (TeamSide)teamSide!, gameId),
            PenaltySheetState s when teamSide is not null => _channelMapper.Map(s, (TeamSide)teamSide, gameId),
            LineupSheetState s when teamSide is not null => _channelMapper.Map(s, stateStore.GetKeyedState<TeamDetailsState>(teamSide.ToString()!), (TeamSide)teamSide, gameId),
            PenaltyBoxState s when teamSide is not null => _channelMapper.Map(s, stateStore.GetKeyedState<JamLineupState>(teamSide.ToString()!), stateStore.GetKeyedState<TeamDetailsState>(teamSide.ToString()!), (TeamSide)teamSide, gameId),
            TimeoutListState s => _channelMapper.Map(s, gameId),
            _ => []
        };

        foreach (var (key, value) in mappedStates)
        {
            _states[gameId][key] = value;
        }

        return Task.CompletedTask;
    }

    
}

public sealed class GameNotFoundError : ResultError;
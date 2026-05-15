using System.Collections.Immutable;
using System.Text.Json;

using FluentAssertions;
using Func;

using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;
using jamster.engine.TestGames.GameGeneration;

using DomainTick = jamster.engine.Domain.Tick;

namespace jamster.engine.tests.EventHandling;

public class EventIntegrationTests : EventBusIntegrationTest
{
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame), TestName = "Full game")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithScoresAndStats), TestName = "Jams with scores and stats")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.SingleJamStartedWithoutEndingIntermission), TestName = "Single jam started without ending intermission")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.OfficialReviewDuringIntermission), TestName = "Official review during intermission")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.CustomRules), TestName = "Custom rules")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithLineupsAndPenalties), TestName = "Jams with lineups and penalties")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithOverrunningJam), TestName = "Jams with overrunning jam")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.TimeoutBeforeFirstJam), TestName = "Timeout before first jam")]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.OvertimeJam), TestName = "Overtime jam")]
    public async Task EventSources_UpdateStatesAsExpected(Type eventSourceType, string eventSourceName)
    {
        var events = GetEvents(eventSourceType, eventSourceName);

        await AddEvents(events);

        await Tick(events.Last().Tick + 1);

        Console.WriteLine(GetState<GameStageState>());
        Console.WriteLine(GetState<PeriodClockState>());
        Console.WriteLine(GetState<LineupClockState>());
        Console.WriteLine(GetState<JamClockState>());
        Console.WriteLine(GetState<TimeoutClockState>());
        Console.WriteLine(GetState<IntermissionClockState>());
        Console.WriteLine(GetState<TeamScoreState>("Home"));
        Console.WriteLine(GetState<TeamScoreState>("Away"));
        Console.WriteLine(GetState<TripScoreState>("Home"));
        Console.WriteLine(GetState<TripScoreState>("Away"));
        Console.WriteLine(GetState<TeamTimeoutsState>("Home"));
        Console.WriteLine(GetState<TeamTimeoutsState>("Away"));
    }

    [Test, Repeat(20)]
    public async Task RandomGame_UpdatesStatesAsExpected()
    {
        var game = GameGenerator.GenerateRandom();
        var simulator = new GameSimulator(game);
        var simulatorEvents = simulator.SimulateGame(Option.Some(GenerateValidationEvent));

        foreach (var @event in simulatorEvents)
        {
            if (@event is ValidateStateFakeEvent) continue;

            Console.WriteLine(@event.HasBody
                ? $"{@event.Tick}: {@event.GetType().Name} {JsonSerializer.Serialize(@event.GetBodyObject()!)}"
                : $"{@event.Tick}: {@event.GetType().Name}");
        }


        await AddEvents(simulatorEvents);
    }

    private Event GenerateValidationEvent(GameSimulator.GameState gameState, DomainTick tick)
    {
        return new ValidateStateFakeEvent(tick, [
            new PeriodClockState(
                gameState.Clocks.PeriodClock.Running
                    && (
                        gameState.Stage is Stage.Jam or Stage.Timeout
                        || (gameState.Clocks.PeriodClock.TicksPassedAtLastStart + tick - gameState.Clocks.PeriodClock.LastStartTick).Seconds < Rules.DefaultRules.PeriodRules.DurationInSeconds
                    ),
                gameState.Stage is Stage.BeforeGame or Stage.AfterGame
                    || gameState is { Stage: Stage.Lineup or Stage.Timeout or Stage.AfterTimeout, Jam: 0 }
                    || gameState is { Stage: Stage.Intermission, PeriodFinalized: true }
                    || gameState.Clocks.PeriodClock.Running && gameState.Clocks.PeriodClock.TicksPassedAtLastStart + tick - gameState.Clocks.PeriodClock.LastStartTick >= DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds)
                    || !gameState.Clocks.PeriodClock.Running && gameState.Clocks.PeriodClock.PassedTicks >= DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds),
                gameState.Clocks.PeriodClock.PassedTicks > 0,
                gameState.Clocks.PeriodClock.LastStartTick,
                gameState.Clocks.PeriodClock.TicksPassedAtLastStart,
                gameState.Clocks.PeriodClock.Running
                    ? Math.Min(gameState.Clocks.PeriodClock.TicksPassedAtLastStart + tick - gameState.Clocks.PeriodClock.LastStartTick, DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds))
                    : gameState.Clocks.PeriodClock.PassedTicks
            ),
            new JamClockState(
                gameState.Clocks.JamClock.Running,
                gameState.Clocks.JamClock.LastStartTick,
                gameState.Clocks.JamClock.Running
                    ? gameState.Clocks.JamClock.TicksPassedAtLastStart + tick - gameState.Clocks.JamClock.LastStartTick
                    : gameState.Clocks.JamClock.PassedTicks,
                true,
                !gameState.JamInfo.HomeTeam.Called && !gameState.JamInfo.AwayTeam.Called && gameState.Clocks.JamClock.PassedTicks.Seconds == Rules.DefaultRules.JamRules.DurationInSeconds
            ),
            new LineupClockState(
                gameState.Clocks.LineupClock.Running,
                gameState.Clocks.LineupClock.LastStartTick,
                gameState.Clocks.LineupClock.Running
                    ? gameState.Clocks.LineupClock.TicksPassedAtLastStart + tick - gameState.Clocks.LineupClock.LastStartTick
                    : gameState.Clocks.LineupClock.PassedTicks
            ),
            new IntermissionClockState(
                gameState.Clocks.IntermissionClock.Running && (gameState.Clocks.IntermissionClock.LastStartTick - tick).Seconds < Rules.DefaultRules.IntermissionRules.DurationInSeconds,
                gameState.Stage is not Stage.Intermission || gameState.Clocks.IntermissionClock.PassedTicks >= DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds),
                DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds),
                gameState.Stage is Stage.Intermission ? gameState.Clocks.IntermissionClock.LastStartTick + DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds) : 0,
                gameState.Stage is Stage.Intermission ? Math.Max(0, (gameState.Clocks.IntermissionClock.LastStartTick + DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds) - tick).Seconds) : 0
            ),
            ("Home", new PenaltyBoxState(
                gameState.Lineups.HomeTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: not null })
                    .Select(s => s!.Id)
                    .ToArray(),
                gameState.Lineups.HomeTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: null })
                    .Select(s => s!.Id)
                    .ToArray()
                )),
            ("Away", new PenaltyBoxState(
                gameState.Lineups.AwayTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: not null })
                    .Select(s => s!.Id)
                    .ToArray(),
                gameState.Lineups.AwayTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: null })
                    .Select(s => s!.Id)
                    .ToArray()
            )),
            ("Home", new JamLineupState(
                GetSkaterId(t => t.HomeTeam)(gameState.Sheets.HomeSheets.LineupSheet[^1].JammerNumber),
                GetSkaterId(t => t.HomeTeam)(gameState.Sheets.HomeSheets.LineupSheet[^1].PivotNumber),
                gameState.Sheets.HomeSheets.LineupSheet[^1].BlockerNumbers.Select(GetSkaterId(t => t.HomeTeam)).OrderBy(x => x).ToArray()
            )),
            ("Away", new JamLineupState(
                GetSkaterId(t => t.AwayTeam)(gameState.Sheets.AwaySheets.LineupSheet[^1].JammerNumber),
                GetSkaterId(t => t.AwayTeam)(gameState.Sheets.AwaySheets.LineupSheet[^1].PivotNumber),
                gameState.Sheets.AwaySheets.LineupSheet[^1].BlockerNumbers.Select(GetSkaterId(t => t.AwayTeam)).OrderBy(x => x).ToArray()
            )),
            ("Home", new TeamScoreState(gameState.Scores.HomeScore.GameTotal, gameState.Scores.HomeScore.JamTotal)),
            ("Away", new TeamScoreState(gameState.Scores.AwayScore.GameTotal, gameState.Scores.AwayScore.JamTotal)),
            ("Home", new TeamJamStatsState(
                gameState.JamInfo.HomeTeam.Lead,
                gameState.JamInfo.HomeTeam.Lost,
                gameState.JamInfo.HomeTeam.Called,
                gameState.Sheets.HomeSheets.ScoreSheet[^1].StarPassTrip != null,
                gameState.JamInfo.HomeTeam.CompletedInitial)
            ),
            ("Away", new TeamJamStatsState(
                gameState.JamInfo.AwayTeam.Lead,
                gameState.JamInfo.AwayTeam.Lost,
                gameState.JamInfo.AwayTeam.Called,
                gameState.Sheets.AwaySheets.ScoreSheet[^1].StarPassTrip != null,
                gameState.JamInfo.AwayTeam.CompletedInitial)
            ),
            new GameStageState(
                gameState.Stage,
                gameState.Period + (gameState.Stage == Stage.BeforeGame ? 1 : 0),
                gameState.Jam,
                gameState.TotalJam,
                gameState.PeriodFinalized,
                gameState.NextJamShouldStart
            ),
            new OvertimeState(gameState.IsInOvertime),
        ]);

        Func<string?, Guid?> GetSkaterId(Func<GameSimulator.Teams, GameTeam> teamSelector) => number =>
            teamSelector(gameState.Teams).Roster.SingleOrDefault(s => s.Number == number)?.Id;

    }

    [Test]
    public async Task LoadingGameGivesSameStateAsRunningGame()
    {
        Type[] excludedReducerTypes =
        [
            typeof(Timeline)
        ];

        var events = GetEvents(typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame)).Where(e => e is not IFakeEvent).ToArray();

        var reducers = Mocker.Create<IEnumerable<IReducer>>().Where(r => !excludedReducerTypes.Contains(r.GetType())).ToImmutableList();
        var stateTypes = reducers.Select(r => r.GetStateKey() is Some<string> k ? (Key: k.Value, r.StateType) : (null, r.StateType)).ToArray();
        var stateGetters = stateTypes.Select(x => GetStateGetter(x.Key, x.StateType)).ToArray();

        var tick = engine.Domain.Tick.FromSeconds(0);
        const int tickStep = 50;

        var stateCaptures = new Dictionary<Guid, object[]>();

        foreach(var @event in events)
        {
            while (tick < @event.Tick)
            {
                await Tick(tick);
                tick += tickStep;
            }

            await EventBus.AddEvent(Game, @event);
            await Tick(@event.Tick);

            stateCaptures[@event.Id] = GetAllStates();
        }

        try
        {
            for (var i = 1; i <= events.Length; ++i)
            {
                var eventSubset = events.Take(i).ToArray();
                var lastEvent = eventSubset.Last();

                StateStore.LoadDefaultStates(reducers);
                await StateStore.ApplyEvents(reducers, null, eventSubset);
                await Tick(lastEvent.Tick);

                var states = GetAllStates();

                states.Should().BeEquivalentTo(stateCaptures[lastEvent.Id]);
            }
        }
        finally
        {
            foreach (var state in GetAllStates())
                Console.WriteLine(state);
        }

        return;

        Func<IGameStateStore, object> GetStateGetter(string? key, Type stateType) =>
            key is null
                ? typeof(IGameStateStore)
                    .GetMethods()
                    .Single(m => m is { Name: nameof(IGameStateStore.GetState), IsGenericMethod: true })
                    .MakeGenericMethod(stateType)
                    .Map(m => (Func<IGameStateStore, object>)(s => m.Invoke(s, [])!))
                : typeof(IGameStateStore)
                    .GetMethods()
                    .Single(m => m is { Name: nameof(IGameStateStore.GetKeyedState), IsGenericMethod: true })
                    .MakeGenericMethod(stateType)
                    .Map(m => (Func<IGameStateStore, object>)(s => m.Invoke(s, [key])!));

        object[] GetAllStates() => stateGetters.Select(g => g(StateStore)).ToArray();
    }

    public static Event[] GetEvents(Type eventSourceType, string eventSourceName) =>
        eventSourceType.GetProperty(eventSourceName)?.GetValue(null) as Event[]
        ?? throw new ArgumentException();
}
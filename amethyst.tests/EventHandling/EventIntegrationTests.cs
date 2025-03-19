using System.Collections.Immutable;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Func;

namespace amethyst.tests.EventHandling;

public class EventIntegrationTests : EventBusIntegrationTest
{
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.TwoJamsWithScores))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.SingleJamStartedWithoutEndingIntermission))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.OfficialReviewDuringIntermission))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.CustomRules))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithLineupsAndPenalties))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithOverrunningJam))]
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

    [Test]
    public async Task LoadingGameGivesSameStateAsRunningGame()
    {
        var events = GetEvents(typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame)).Where(e => e is not IFakeEvent).ToArray();

        var reducers = Mocker.Create<IEnumerable<IReducer>>().ToImmutableList();
        var stateTypes = reducers.Select(r => r.GetStateKey() is Some<string> k ? (Key: k.Value, r.StateType) : (null, r.StateType)).ToArray();
        var stateGetters = stateTypes.Select(x => GetStateGetter(x.Key, x.StateType)).ToArray();

        var tick = Domain.Tick.FromSeconds(0);
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

                Console.WriteLine(lastEvent);

                StateStore.LoadDefaultStates(reducers);
                await StateStore.ApplyEvents(reducers, eventSubset);
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
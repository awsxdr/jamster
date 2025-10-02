﻿using System.Collections.Immutable;

using jamster.engine.tests.GameGeneration;
using FluentAssertions;
using Func;

using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests.EventHandling;

public class EventIntegrationTests : EventBusIntegrationTest
{
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithScoresAndStats))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.SingleJamStartedWithoutEndingIntermission))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.OfficialReviewDuringIntermission))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.CustomRules))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithLineupsAndPenalties))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.JamsWithOverrunningJam))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.TimeoutBeforeFirstJam))]
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
        var simulatorEvents = simulator.SimulateGame();

        await AddEvents(simulatorEvents);
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
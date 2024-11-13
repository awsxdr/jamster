using amethyst.Events;
using amethyst.Reducers;

namespace amethyst.tests.EventHandling;

public class EventIntegrationTests : EventBusIntegrationTest
{
    [TestCase( typeof(TestGameEventsSource), nameof(TestGameEventsSource.FullGame))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.TwoJamsWithScores))]
    [TestCase(typeof(TestGameEventsSource), nameof(TestGameEventsSource.SingleJamStartedWithoutEndingIntermission))]
    public async Task EventSources_UpdateStatesAsExpected(Type eventSourceType, string eventSourceName)
    {
        var events = eventSourceType.GetProperty(eventSourceName)?.GetValue(null) as Event[] 
            ?? throw new ArgumentException();

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
}
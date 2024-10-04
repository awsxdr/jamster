using amethyst.Reducers;

namespace amethyst.tests.EventHandling;

public class ClocksAndStageEventIntegrationTests : EventBusIntegrationTest
{
    [Test]
    public async Task FullGame_UpdatesClocksAsExpected()
    {
        var events = TestGameEventsSource.FullGame;
        await AddEvents(events);

        Tick(events.Last().Tick + 1);

        Console.WriteLine(GetState<GameStageState>());
        Console.WriteLine(GetState<PeriodClockState>());
        Console.WriteLine(GetState<LineupClockState>());
        Console.WriteLine(GetState<JamClockState>());
        Console.WriteLine(GetState<TimeoutClockState>());
        Console.WriteLine(GetState<IntermissionClockState>());
    }
}
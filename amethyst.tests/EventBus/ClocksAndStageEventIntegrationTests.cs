using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.EventBus;

public class ClocksAndStageEventIntegrationTests : EventBusIntegrationTest
{
    [Test]
    public async Task FullGame_UpdatesClocksAsExpected()
    {
        var tick = GetRandomTick();

        await AddEvents(
            ValidateState(GetNextTick(15),
                new GameStageState(Stage.BeforeGame, 0, 0)),
            new IntermissionEnded(GetNextTick(15)),
            ValidateState(GetNextTick(15),
                new GameStageState(Stage.Lineup, 1, 0)),
            new JamStarted(GetNextTick(120 + 30)), // Jam that runs for full duration
            new JamStarted(GetNextTick(57)), // Jam that is called
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(95)),
            new JamEnded(GetNextTick(25)),
            new TimeoutStarted(GetNextTick(60)), // Team timeout
            new TimeoutEnded(GetNextTick(25)),
            new JamStarted(GetNextTick(30)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(107)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(145)),
            new TimeoutStarted(GetNextTick(218)), // Official timeout
            new TimeoutEnded(GetNextTick(15)),
            new JamStarted(GetNextTick(82)),
            new JamEnded(GetNextTick(31)), // Overrunning lineup
            new JamStarted(GetNextTick(58)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(120 + 15)),
            ValidateState(GetNextTick(15),
                new GameStageState(Stage.Lineup, 1, 8)),
            new TimeoutStarted(GetNextTick(90)), // Timeout not ended
            new JamStarted(GetNextTick(76)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(112)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(121)),
            new JamEnded(GetNextTick(29)), // Late jam ended
            new JamStarted(GetNextTick(68)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(60)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(93)),
            new JamEnded(GetNextTick(30)),
            new JamStarted(GetNextTick(112)),
            new JamEnded(GetNextTick(15 * 60)),
            ValidateState(GetNextTick(15),
                new GameStageState(Stage.Intermission, 1, 15))
        );

        Tick(tick + 1);

        Console.WriteLine(GetState<GameStageState>());
        Console.WriteLine(GetState<PeriodClockState>());
        Console.WriteLine(GetState<LineupClockState>());
        Console.WriteLine(GetState<JamClockState>());
        Console.WriteLine(GetState<TimeoutClockState>());
        Console.WriteLine(GetState<IntermissionClockState>());

        //var gameStage = GetState<GameStageState>();
        //gameStage.Should().Be(new GameStageState(Stage.Jam, 1, 3));

        //var jamClock = GetState<JamClockState>();
        //jamClock.IsRunning.Should().BeTrue();

        return;

        Tick GetNextTick(double durationInSeconds)
        {
            var variability = Random.Shared.Next(-100, 100);
            var durationInTicks = (int)(durationInSeconds * 1000 + variability);
            var currentTick = tick;
            tick += durationInTicks;
            return currentTick;
        }
    }
}
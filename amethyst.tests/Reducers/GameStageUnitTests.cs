using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class GameStageUnitTests : ReducerUnitTest<GameStage, GameStageState>
{
    [TestCase(Stage.BeforeGame, Stage.Lineup)]
    [TestCase(Stage.Lineup, Stage.Lineup)]
    [TestCase(Stage.Jam, Stage.Jam)]
    [TestCase(Stage.Timeout, Stage.Timeout)]
    [TestCase(Stage.Intermission, Stage.Lineup)]
    [TestCase(Stage.AfterGame, Stage.AfterGame)]
    public async Task IntermissionEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1, false);

        await Subject.Handle(new IntermissionEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.Jam)]
    [TestCase(Stage.Lineup, Stage.Jam)]
    [TestCase(Stage.Jam, Stage.Jam)]
    [TestCase(Stage.Timeout, Stage.Jam)]
    [TestCase(Stage.Intermission, Stage.Jam)]
    [TestCase(Stage.AfterGame, Stage.Jam)]
    public async Task JamStarted_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1, false);
        MockState(new JamClockState(currentStage == Stage.Jam, 0, 0, 0));

        await Subject.Handle(new JamStarted(0));

        State.Stage.Should().Be(expectedStage);
    }

    [Test]
    public async Task JamStarted_WhenPeriodFinalized_SetsPeriodToNotBeFinalized()
    {
        State = new(Stage.Lineup, 2, 1, true);
        MockState(new JamClockState(false, 0, 0, 0));

        await Subject.Handle(new JamStarted(0));

        State.PeriodIsFinalized.Should().BeFalse();
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame, false, 1)]
    [TestCase(Stage.Lineup, Stage.Lineup, false, 1)]
    [TestCase(Stage.Jam, Stage.Lineup, false, 1)]
    [TestCase(Stage.Jam, Stage.Lineup, false, 2)]
    [TestCase(Stage.Jam, Stage.Intermission, true, 1)]
    [TestCase(Stage.Jam, Stage.AfterGame, true, 2)]
    [TestCase(Stage.Timeout, Stage.Timeout, false, 1)]
    [TestCase(Stage.Intermission, Stage.Intermission, false, 1)]
    [TestCase(Stage.AfterGame, Stage.AfterGame, false, 1)]
    public async Task JamEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, bool periodClockExpired, int period)
    {
        State = new(currentStage, period, 1, false);

        MockState<PeriodClockState>(new (!periodClockExpired, 0, 0, PeriodClock.PeriodLengthInTicks + (periodClockExpired ? 10000 : -10000), 0));

        await Subject.Handle(new JamEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame)]
    [TestCase(Stage.Lineup, Stage.Timeout)]
    [TestCase(Stage.Jam, Stage.Timeout)]
    [TestCase(Stage.Timeout, Stage.Timeout)]
    [TestCase(Stage.Intermission, Stage.Timeout)]
    [TestCase(Stage.AfterGame, Stage.Timeout)]
    public async Task TimeoutStarted_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1, false);

        await Subject.Handle(new TimeoutStarted(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame, 1)]
    [TestCase(Stage.Lineup, Stage.Intermission, 1)]
    [TestCase(Stage.Lineup, Stage.AfterGame, 2)]
    [TestCase(Stage.Jam, Stage.Intermission, 1)]
    [TestCase(Stage.Jam, Stage.AfterGame, 2)]
    [TestCase(Stage.Timeout, Stage.Intermission, 1)]
    [TestCase(Stage.Timeout, Stage.AfterGame, 2)]
    [TestCase(Stage.Intermission, Stage.Intermission, 1)]
    [TestCase(Stage.AfterGame, Stage.AfterGame, 2)]
    public async Task PeriodEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, int period)
    {
        State = new(currentStage, period, 1, false);

        await Subject.Handle(new PeriodEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, 0, 1)]
    [TestCase(Stage.Lineup, 4, 5)]
    [TestCase(Stage.Jam, 5, 5)]
    [TestCase(Stage.Timeout, 6, 7)]
    [TestCase(Stage.Intermission, 0, 1)]
    [TestCase(Stage.AfterGame, 10, 11)]
    public async Task JamStarted_SetsExpectedJamNumber(Stage currentStage, int jamNumber, int expectedJamNumber)
    {
        State = new(currentStage, 1, jamNumber, false);
        MockState(new JamClockState(currentStage == Stage.Jam, 0, 0, 0));

        await Subject.Handle(new JamStarted(0));

        State.JamNumber.Should().Be(expectedJamNumber);
    }

    [Test]
    public async Task PeriodEnded_WhenEnteringIntermission_StartsIntermissionClock()
    {
        State = new(Stage.Jam, 1, 15, false);
        MockState<PeriodClockState>(new(false, 0, 0, PeriodClock.PeriodLengthInTicks + 10000, 0));

        var result = await Subject.Handle(new PeriodEnded(123));

        var implicitEvent = result.Should().ContainSingle().Which.Should().BeAssignableTo<IntermissionStarted>().Which;
        implicitEvent.Tick.Should().Be(123);
        implicitEvent.Body.DurationInSeconds.Should().Be(15 * 60);
    }

    [TestCase(Stage.BeforeGame, 0, 0, false)]
    [TestCase(Stage.Lineup, 1, 1, false)]
    [TestCase(Stage.Jam, 1,  1, false)]
    [TestCase(Stage.Timeout, 1, 1, false)]
    [TestCase(Stage.Intermission, 1, 2, true)]
    [TestCase(Stage.AfterGame, 2, 2, true)]
    public async Task PeriodFinalized_SetsExpectedPeriodNumber_AndSetsExpectedFinalizedState(Stage currentStage, int periodNumber, int expectedPeriodNumber, bool expectedFinalized)
    {
        State = new(currentStage, periodNumber, 1, false);

        await Subject.Handle(new PeriodFinalized(0));

        State.PeriodNumber.Should().Be(expectedPeriodNumber);
        State.PeriodIsFinalized.Should().Be(expectedFinalized);
    }
}
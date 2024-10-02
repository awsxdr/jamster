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
    public void IntermissionEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1);

        Subject.Handle(new IntermissionEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.Jam)]
    [TestCase(Stage.Lineup, Stage.Jam)]
    [TestCase(Stage.Jam, Stage.Jam)]
    [TestCase(Stage.Timeout, Stage.Jam)]
    [TestCase(Stage.Intermission, Stage.Jam)]
    [TestCase(Stage.AfterGame, Stage.Jam)]
    public void JamStarted_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1);

        Subject.Handle(new JamStarted(0));

        State.Stage.Should().Be(expectedStage);
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
    public void JamEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, bool periodClockExpired, int period)
    {
        State = new(currentStage, period, 1);

        MockState<PeriodClockState>(new (!periodClockExpired, 0, 0, PeriodClock.PeriodLengthInTicks + (periodClockExpired ? 10000 : -10000), 0));

        Subject.Handle(new JamEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame)]
    [TestCase(Stage.Lineup, Stage.Timeout)]
    [TestCase(Stage.Jam, Stage.Timeout)]
    [TestCase(Stage.Timeout, Stage.Timeout)]
    [TestCase(Stage.Intermission, Stage.Timeout)]
    [TestCase(Stage.AfterGame, Stage.Timeout)]
    public void TimeoutStarted_SetsExpectedStage(Stage currentStage, Stage expectedStage)
    {
        State = new(currentStage, 1, 1);

        Subject.Handle(new TimeoutStarted(0));

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
    public void PeriodEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, int period)
    {
        State = new(currentStage, period, 1);

        Subject.Handle(new PeriodEnded(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, 0, 1)]
    [TestCase(Stage.Lineup, 4, 5)]
    [TestCase(Stage.Jam, 5, 5)]
    [TestCase(Stage.Timeout, 6, 7)]
    [TestCase(Stage.Intermission, 0, 1)]
    [TestCase(Stage.AfterGame, 10, 11)]
    public void JamStarted_SetsExpectedJamNumber(Stage currentStage, int jamNumber, int expectedJamNumber)
    {
        State = new(currentStage, 1, jamNumber);

        Subject.Handle(new JamStarted(0));

        State.JamNumber.Should().Be(expectedJamNumber);
    }
}
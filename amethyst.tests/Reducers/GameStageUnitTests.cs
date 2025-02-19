using amethyst.Domain;
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
        State = new(currentStage, 1, 1, 1, false);

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
        State = new(currentStage, 1, 1, 1, false);
        MockState<JamClockState>(new(currentStage == Stage.Jam, 0, 0, 0));
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.Stage.Should().Be(expectedStage);
    }

    [Test]
    public async Task JamStarted_WhenPeriodFinalized_SetsPeriodToNotBeFinalized()
    {
        State = new(Stage.Lineup, 2, 1, 1, true);
        MockState<JamClockState>(new(false, 0, 0, 0));
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.PeriodIsFinalized.Should().BeFalse();
    }

    [Test]
    public async Task JamStarted_WhenSetToNotResetJamNumber_DoesNotReset()
    {
        State = new(Stage.Intermission, 2, 10, 20, true);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            JamRules = Rules.DefaultRules.JamRules with {
                ResetJamNumbersBetweenPeriods = false,
            },
        }));

        await Subject.Handle(new JamStarted(0));

        State.JamNumber.Should().Be(11);
    }

    [Test]
    public async Task JamStarted_IncrementsTotalJamNumber()
    {
        State = new(Stage.Lineup, 2, 6, 21, false);
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.TotalJamNumber.Should().Be(22);
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame, false, 1, 2)]
    [TestCase(Stage.Lineup, Stage.Lineup, false, 1, 2)]
    [TestCase(Stage.Jam, Stage.Lineup, false, 1, 2)]
    [TestCase(Stage.Jam, Stage.Lineup, false, 2, 2)]
    [TestCase(Stage.Jam, Stage.Intermission, true, 1, 2)]
    [TestCase(Stage.Jam, Stage.AfterGame, true, 2, 2)]
    [TestCase(Stage.Timeout, Stage.Timeout, false, 1, 2)]
    [TestCase(Stage.Intermission, Stage.Intermission, false, 1, 2)]
    [TestCase(Stage.AfterGame, Stage.AfterGame, false, 1, 2)]
    [TestCase(Stage.Jam, Stage.Intermission, true, 3, 4)]
    public async Task JamEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, bool periodClockExpired, int period, int maxPeriods)
    {
        State = new(currentStage, period, 1, 1, false);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                PeriodCount = maxPeriods,
            }
        }));

        MockState<PeriodClockState>(new (!periodClockExpired, periodClockExpired, 0, 0, Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds + (periodClockExpired ? 10 : -10)), 0));

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
        State = new(currentStage, 1, 1, 1, false);

        await Subject.Handle(new TimeoutStarted(0));

        State.Stage.Should().Be(expectedStage);
    }

    [TestCase(Stage.BeforeGame, Stage.BeforeGame, 1, 2)]
    [TestCase(Stage.Lineup, Stage.Intermission, 1, 2)]
    [TestCase(Stage.Lineup, Stage.AfterGame, 2, 2)]
    [TestCase(Stage.Jam, Stage.Intermission, 1, 2)]
    [TestCase(Stage.Jam, Stage.AfterGame, 2, 2)]
    [TestCase(Stage.Timeout, Stage.Intermission, 1, 2)]
    [TestCase(Stage.Timeout, Stage.AfterGame, 2, 2)]
    [TestCase(Stage.Intermission, Stage.Intermission, 1, 2)]
    [TestCase(Stage.AfterGame, Stage.AfterGame, 2, 2)]
    [TestCase(Stage.Lineup, Stage.Intermission, 2, 3)]
    [TestCase(Stage.Jam, Stage.Intermission, 2, 3)]
    public async Task PeriodEnded_SetsExpectedStage(Stage currentStage, Stage expectedStage, int period, int maxPeriods)
    {
        State = new(currentStage, period, 1, 1, false);
        MockState(new IntermissionClockState(false, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 0, 0));
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                PeriodCount = maxPeriods,
            }
        }));

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
        State = new(currentStage, 1, jamNumber, jamNumber, false);
        MockState<JamClockState>(new(currentStage == Stage.Jam, 0, 0, 0));
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.JamNumber.Should().Be(expectedJamNumber);
    }

    [TestCase(Stage.BeforeGame)]
    [TestCase(Stage.Intermission)]
    public async Task JamStarted_WhenInIntermission_AndPeriodFinalized_SendsIntermissionEnded(Stage stage)
    {
        State = new(stage, 1, 1, 1, true);
        MockState<JamClockState>(new(false, 0, 0, 0));
        MockState<RulesState>(new(Rules.DefaultRules));

        var implicitEvents = await Subject.Handle(new JamStarted(1000));

        implicitEvents.OfType<IntermissionEnded>().Should().ContainSingle()
            .Which.Tick.Should().Be(1000);
    }

    [Test]
    public async Task PeriodEnded_WhenEnteringIntermission_AndIntermissionClockSet_StartsIntermissionClockWithoutChangingValue()
    {
        State = new(Stage.Jam, 1, 15, 15, false);
        MockState<PeriodClockState>(new(false, true, 0, 0, Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds + 10), 0));
        MockState<IntermissionClockState>(new(false, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 0, 10));
        MockState<RulesState>(new(Rules.DefaultRules));

        var result = (await Subject.Handle(new PeriodEnded(123))).ToArray();

        result.Should().HaveCount(1).And.Subject.Single().Should().BeAssignableTo<IntermissionStarted>();
    }

    [TestCase(Stage.BeforeGame, 0, 0, false)]
    [TestCase(Stage.Lineup, 1, 1, false)]
    [TestCase(Stage.Jam, 1,  1, false)]
    [TestCase(Stage.Timeout, 1, 1, false)]
    [TestCase(Stage.Intermission, 1, 2, true)]
    [TestCase(Stage.AfterGame, 2, 2, true)]
    public async Task PeriodFinalized_SetsExpectedPeriodNumber_AndSetsExpectedFinalizedState(Stage currentStage, int periodNumber, int expectedPeriodNumber, bool expectedFinalized)
    {
        State = new(currentStage, periodNumber, 1, 1, false);
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new PeriodFinalized(0));

        State.PeriodNumber.Should().Be(expectedPeriodNumber);
        State.PeriodIsFinalized.Should().Be(expectedFinalized);
    }

    [TestCase(TimeoutType.Official, true, false, Stage.Timeout)]
    [TestCase(TimeoutType.Team, true, false, Stage.Intermission)]
    [TestCase(TimeoutType.Official, true, true, Stage.Intermission)]
    [TestCase(TimeoutType.Team, true, true, Stage.Intermission)]
    [TestCase(TimeoutType.Official, false, false, Stage.Intermission)]
    public async Task TimeoutTypeSet_ShouldUpdateStageCorrectly(TimeoutType newTimeoutType, bool periodExpired, bool periodFinalized, Stage expectedStage)
    {
        State = new(Stage.Intermission, 1, 1, 1, periodFinalized);
        MockState<PeriodClockState>(new(false, periodExpired, 0, 0, 0, 0));
        MockState<TimeoutTypeState>(new(CompoundTimeoutType.HomeTeamTimeout, 0));
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with
            {
                PeriodClockBehavior = TimeoutPeriodClockStopBehavior.OfficialReview | TimeoutPeriodClockStopBehavior.OfficialTimeout,
            }
        }));

        await Subject.Handle(new TimeoutTypeSet(10000, new(newTimeoutType, TeamSide.Away)));

        State.Stage.Should().Be(expectedStage);
    }
}
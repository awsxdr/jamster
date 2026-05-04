using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

using DomainTick = jamster.engine.Domain.Tick;

namespace jamster.engine.tests.Reducers;

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
        State = GameStageState.Default with { Stage = currentStage };

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
        State = GameStageState.Default with { Stage = currentStage };

        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.Stage.Should().Be(expectedStage);
    }

    [Test]
    public async Task JamStarted_WhenPeriodFinalized_SetsPeriodToNotBeFinalized()
    {
        State = GameStageState.Default with { Stage = Stage.Lineup, PeriodNumber = 2, PeriodIsFinalized = true, };

        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.PeriodIsFinalized.Should().BeFalse();
    }

    [Test]
    public async Task JamStarted_WhenSetToNotResetJamNumber_DoesNotReset()
    {
        State = GameStageState.Default with
        {
            Stage = Stage.Intermission,
            PeriodNumber = 2,
            JamNumber = 10,
            TotalJamNumber = 20,
            PeriodIsFinalized = true,
        };

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
        State = GameStageState.Default with
        {
            Stage = Stage.Lineup,
            PeriodNumber = 2,
            JamNumber = 6,
            TotalJamNumber = 21,
        };

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
        State = GameStageState.Default with { Stage = currentStage, PeriodNumber = period };

        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                PeriodCount = maxPeriods,
            }
        }));

        MockState<PeriodClockState>(new (!periodClockExpired, periodClockExpired, true, 0, 0, DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds + (periodClockExpired ? 10 : -10))));

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
        State = GameStageState.Default with { Stage = currentStage };

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
        State = GameStageState.Default with { Stage = currentStage, PeriodNumber = period };

        MockState(new IntermissionClockState(false, false, DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 0, 0));
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
        State = GameStageState.Default with { Stage = currentStage, JamNumber = jamNumber, TotalJamNumber = jamNumber };

        MockState<JamClockState>(new(currentStage == Stage.Jam, 0, 0, true, false));
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new JamStarted(0));

        State.JamNumber.Should().Be(expectedJamNumber);
    }

    [TestCase(Stage.BeforeGame)]
    [TestCase(Stage.Intermission)]
    public async Task JamStarted_WhenInIntermission_AndPeriodFinalized_SendsIntermissionEnded(Stage stage)
    {
        State = GameStageState.Default with { Stage = stage, PeriodIsFinalized = true };

        MockState<JamClockState>(new(false, 0, 0, true, false));
        MockState<RulesState>(new(Rules.DefaultRules));

        var implicitEvents = await Subject.Handle(new JamStarted(1000));

        implicitEvents.OfType<IntermissionEnded>().Should().ContainSingle()
            .Which.Tick.Should().Be(1000);
    }

    [Test]
    public async Task PeriodEnded_WhenEnteringIntermission_AndIntermissionClockSet_StartsIntermissionClockWithoutChangingValue()
    {
        State = GameStageState.Default with { Stage = Stage.Jam, JamNumber = 15, TotalJamNumber = 15 };

        MockState<PeriodClockState>(new(false, true, true, 0, 0, DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds + 10)));
        MockState<IntermissionClockState>(new(false, false, DomainTick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 0, 10));
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
        State = GameStageState.Default with { Stage = currentStage, PeriodNumber = periodNumber };

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
        State = GameStageState.Default with { Stage = Stage.Intermission, PeriodIsFinalized = periodFinalized };

        MockState<PeriodClockState>(new(false, periodExpired, true, 0, 0, 0));
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

    [TestCase(1, 10, 1, -5, 5)]
    [TestCase(1, 10, 1, 5, 15)]
    [TestCase(2, 10, 1, 5, 10)]
    [TestCase(1, 3, 1, -5, 1)]
    [TestCase(1, 10, 2, 5, 10)]
    public async Task JamNumberOffset_AdjustsJamNumberAccordingly(int currentPeriod, int currentJam, int offsetPeriod, int offset, int expectedJam)
    {
        State = new(Stage.Lineup, currentPeriod, currentJam, currentJam + (currentPeriod - 1) * 20, false, false);

        await Subject.Handle(new JamNumberOffset(0, new(offsetPeriod, offset)));

        State.JamNumber.Should().Be(expectedJam);
    }

    [Test]
    public async Task OvertimeStarted_SetsIsInOvertimeAsExpected([Values] Stage stage)
    {
        State = GameStageState.Default with { Stage = stage };

        await Subject.Handle(new OvertimeStarted(0));

        State.IsInOvertime.Should().Be(stage == Stage.AfterGame);
    }

    [Test]
    public async Task OvertimeEnded_SetsIsInOvertimeToFalse()
    {
        State = GameStageState.Default with { IsInOvertime = true };

        await Subject.Handle(new OvertimeEnded(0));

        State.IsInOvertime.Should().BeFalse();
    }
}
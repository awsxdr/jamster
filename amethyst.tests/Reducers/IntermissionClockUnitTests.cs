using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class IntermissionClockUnitTests : ReducerUnitTest<IntermissionClock, IntermissionClockState>
{
    [Test]
    public async Task IntermissionEnded_WhenClockIsRunning_StopsClock()
    {
        State = new(true, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 20000, 10);
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new IntermissionEnded(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task IntermissionEnded_WhenClockIsNotRunning_DoesNotChangeState()
    {
        State = new(false, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 20000, 10);
        var originalState = State;

        await Subject.Handle(new IntermissionEnded(15000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task IntermissionEnded_ResetsClockToDefaultDuration()
    {
        State = new(true, true, Domain.Tick.FromSeconds(10), 0, 0);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            IntermissionRules = Rules.DefaultRules.IntermissionRules with
            {
                DurationInSeconds = 123
            }
        }));

        await Subject.Handle(new IntermissionEnded(0));

        State.InitialDurationTicks.Should().Be(Domain.Tick.FromSeconds(123));
    }

    [Test]
    public async Task IntermissionStarted_WhenTargetTickZero_ResetsIntermissionClockToInitialDuration()
    {
        State = new(false, true, Domain.Tick.FromSeconds(10), 0, 0);

        await Subject.Handle(new IntermissionStarted(1000));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(Domain.Tick.FromSeconds(10) + 1000);
        State.SecondsRemaining.Should().Be(10);
    }

    [Test]
    public async Task IntermissionStarted_WhenTargetTickNonZero_ResumesIntermissionClock()
    {
        State = new(false, false, Domain.Tick.FromSeconds(15), Domain.Tick.FromSeconds(15), 10);

        await Subject.Handle(new IntermissionStarted(Domain.Tick.FromSeconds(7)));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(Domain.Tick.FromSeconds(17));
        State.SecondsRemaining.Should().Be(10);
    }

    [Test]
    public async Task IntermissionClockSet_SetsClock()
    {
        State = new(true, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 15000, 15);
        MockState<PeriodClockState>(new(true, true, 0, 0, 0, 0));

        await Subject.Handle(new IntermissionClockSet(10000, new(20)));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.InitialDurationTicks.Should().Be(Domain.Tick.FromSeconds(20));
        State.TargetTick.Should().Be(30000);
        State.SecondsRemaining.Should().Be(20);
    }

    [Test]
    public async Task IntermissionClockSet_WhenPeriodExpired_StartsClock()
    {
        State = new(false, false, Domain.Tick.FromSeconds(15), 0, 15);
        MockState<PeriodClockState>(new(true, true, 0, 0, 0, 0));

        var implicitEvents = await Subject.Handle(new IntermissionClockSet(10000, new(20)));

        implicitEvents.OfType<IntermissionStarted>().Should().ContainSingle();
        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task IntermissionClockSet_WhenPeriodNotExpired_DoesNotStartClock()
    {
        State = new(false, false, Domain.Tick.FromSeconds(15), 0, 15);
        MockState<PeriodClockState>(new(false, false, 0, 0, 0, 0));

        var implicitEvents = await Subject.Handle(new IntermissionClockSet(10000, new(20)));

        implicitEvents.OfType<IntermissionStarted>().Should().BeEmpty();
        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutStarted_WhenPeriodExpired_StopsClock()
    {
        State = new(true, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 20000, 10);
        MockState<PeriodClockState>(new(false, true, 0, Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds), Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds), Rules.DefaultRules.PeriodRules.DurationInSeconds));

        await Subject.Handle(new TimeoutStarted(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutEnded_WhenPeriodExpired_ResetsClock()
    {
        State = new(true, false, 30000, 15000, 10);
        MockState<PeriodClockState>(new(false, true, 0, Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds), Domain.Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds), Rules.DefaultRules.PeriodRules.DurationInSeconds));

        await Subject.Handle(new TimeoutEnded(5000));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(35000);
        State.SecondsRemaining.Should().Be(30);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_SetsNewTime()
    {
        State = new(true, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 30000, 10);

        await Tick(22000);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.SecondsRemaining.Should().Be(8);
        State.TargetTick.Should().Be(30000);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_AndTargetTickHasPassed_MarksClockAsExpired()
    {
        State = new(true, false, Domain.Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), 30000, 10);

        await Tick(30001);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeTrue();
        State.SecondsRemaining.Should().Be(0);
        State.TargetTick.Should().Be(30000);
    }
}
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;
using Moq;

namespace amethyst.tests.Reducers;

public class IntermissionClockUnitTests : ReducerUnitTest<IntermissionClock, IntermissionClockState>
{
    [Test]
    public async Task JamStarted_WhenClockIsRunning_StopsClock()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 20000, 10);

        await Subject.Handle(new JamStarted(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task JamStarted_WhenClockIsNotRunning_DoesNotChangeState()
    {
        State = new(false, false, IntermissionClock.IntermissionDurationInTicks, 20000, 10);
        var originalState = State;

        await Subject.Handle(new JamStarted(15000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task IntermissionEnded_WhenClockIsRunning_StopsClock()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 20000, 10);

        await Subject.Handle(new IntermissionEnded(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task IntermissionEnded_WhenClockIsNotRunning_DoesNotChangeState()
    {
        State = new(false, false, IntermissionClock.IntermissionDurationInTicks, 20000, 10);
        var originalState = State;

        await Subject.Handle(new IntermissionEnded(15000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task IntermissionStarted_StartsClock()
    {
        await Subject.Handle(new IntermissionStarted(0));

        State.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task IntermissionClockSet_SetsClock()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 15000, 15);

        await Subject.Handle(new IntermissionClockSet(10000, new(20)));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(30000);
        State.SecondsRemaining.Should().Be(20);
    }

    [Test]
    public async Task TimeoutStarted_WhenPeriodExpired_StopsClock()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 20000, 10);
        MockState<PeriodClockState>(new(false, true, 0, PeriodClock.PeriodLengthInTicks, PeriodClock.PeriodLengthInTicks, PeriodClock.PeriodLengthInTicks.Seconds));

        await Subject.Handle(new TimeoutStarted(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutEnded_WhenPeriodExpired_ResetsClock()
    {
        State = new(true, false, 30000, 15000, 10);
        MockState<PeriodClockState>(new(false, true, 0, PeriodClock.PeriodLengthInTicks, PeriodClock.PeriodLengthInTicks, PeriodClock.PeriodLengthInTicks.Seconds));

        await Subject.Handle(new TimeoutEnded(5000));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(35000);
        State.SecondsRemaining.Should().Be(30);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_SetsNewTime()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 30000, 10);

        await Tick(22000);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.SecondsRemaining.Should().Be(8);
        State.TargetTick.Should().Be(30000);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_AndTargetTickHasPassed_MarksClockAsExpired()
    {
        State = new(true, false, IntermissionClock.IntermissionDurationInTicks, 30000, 10);

        await Tick(30001);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeTrue();
        State.SecondsRemaining.Should().Be(0);
        State.TargetTick.Should().Be(30000);
    }
}
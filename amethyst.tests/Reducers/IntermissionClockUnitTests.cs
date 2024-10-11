using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class IntermissionClockUnitTests : ReducerUnitTest<IntermissionClock, IntermissionClockState>
{
    [Test]
    public async Task JamStarted_WhenClockIsRunning_StopsClock()
    {
        State = new(true, false, 20000, 10);

        await Subject.Handle(new JamStarted(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task JamStarted_WhenClockIsNotRunning_DoesNotChangeState()
    {
        State = new(false, false, 20000, 10);
        var originalState = State;

        await Subject.Handle(new JamStarted(15000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task IntermissionEnded_WhenClockIsRunning_StopsClock()
    {
        State = new(true, false, 20000, 10);

        await Subject.Handle(new IntermissionEnded(15000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task IntermissionEnded_WhenClockIsNotRunning_DoesNotChangeState()
    {
        State = new(false, false, 20000, 10);
        var originalState = State;

        await Subject.Handle(new IntermissionEnded(15000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task IntermissionStarted_StartsClock()
    {
        await Subject.Handle(new IntermissionStarted(0, new(15 * 60)));

        State.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task IntermissionStarted_RaisesIntermissionLengthSetEvent()
    {
        await Subject.Handle(new IntermissionStarted(0, new(15 * 60)));

        VerifyEventSent<IntermissionLengthSet, IntermissionLengthSetBody>(
            new IntermissionLengthSet(0, new(15 * 60)));
    }

    [Test]
    public async Task IntermissionLengthSet_SetsClock()
    {
        State = new(true, false, 15000, 15);

        await Subject.Handle(new IntermissionLengthSet(10000, new(20)));

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.TargetTick.Should().Be(30000);
        State.SecondsRemaining.Should().Be(20);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_SetsNewTime()
    {
        State = new(true, false, 30000, 10);

        await Tick(22000);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeFalse();
        State.SecondsRemaining.Should().Be(8);
        State.TargetTick.Should().Be(30000);
    }

    [Test]
    public async Task Tick_WhenClockIsRunning_AndTargetTickHasPassed_MarksClockAsExpired()
    {
        State = new(true, false, 30000, 10);

        await Tick(30001);

        State.IsRunning.Should().BeTrue();
        State.HasExpired.Should().BeTrue();
        State.SecondsRemaining.Should().Be(0);
        State.TargetTick.Should().Be(30000);
    }
}
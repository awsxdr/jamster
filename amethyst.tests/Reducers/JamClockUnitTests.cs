using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class JamClockUnitTests : ReducerUnitTest<JamClock, JamClockState>
{
    [Test]
    public void JamStart_WhenClockStopped_StartsJam()
    {
        State = new(false, 0, 0, 0);

        var randomTick = Random.Shared.Next(0, 100000);

        Subject.Handle(new JamStarted(randomTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public void JamStart_WhenClockRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new JamClockState(true, randomTick, randomTick, randomTick / 1000);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamStarted(secondRandomTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(randomTick);
        State.SecondsPassed.Should().Be(randomTick / 1000);
    }

    [Test]
    public void JamStart_WhenClockRunning_AndPreviousJamRanToLength_StartsNewJam()
    {
        State = new JamClockState(true, 0, 0, 0);

        var newJamStartTick = JamClock.JamLengthInTicks + LineupClock.LineupDurationInTicks;

        Subject.Handle(new JamStarted(newJamStartTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(newJamStartTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public void JamEnded_WhenClockRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public void JamEnded_WhenJamNotRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(false, randomTick, randomTick, randomTick / 1000);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(randomTick);
        State.SecondsPassed.Should().Be(randomTick / 1000);
    }

    [Test]
    public void TimeoutStarted_WhenJamRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new TimeoutStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public void Tick_WhenStillTimeInJam_UpdatesTicksPassed()
    {
        State = new(true, 0, 0, 0);
        Subject.Tick(10000, 10000);

        State.TicksPassed.Should().Be(10000);
        State.SecondsPassed.Should().Be(10);
    }

    [Test]
    public void Tick_WhenOverJamTimeLimit_SendsJamEndedEvent()
    {
        State = new(true, 0, 0, 0);
        Subject.Tick(130 * 1000, 130 * 1000);

        VerifyEventSent<JamEnded>(120 * 1000);
    }
}
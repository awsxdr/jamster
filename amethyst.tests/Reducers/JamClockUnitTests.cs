using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.Reducers;

public class JamClockUnitTests : ReducerUnitTest<JamClock, JamClockState>
{
    [Test]
    public async Task JamStart_WhenClockStopped_StartsJam()
    {
        State = new(false, 0, 0, 0);

        var randomTick = Random.Shared.Next(0, 100000);

        await Subject.Handle(new JamStarted(randomTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task JamStart_WhenClockRunning_DoesNotChangeState()
    {
        var randomTick = GetRandomTick();

        State = new JamClockState(true, randomTick, randomTick, randomTick.Seconds);

        var secondTick = randomTick + 10000;
        var ticksPassed = secondTick - randomTick;

        await Subject.Handle(new JamStarted(secondTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(ticksPassed);
        State.SecondsPassed.Should().Be(ticksPassed.Seconds);
    }

    [Test]
    public async Task JamStart_WhenClockRunning_AndPreviousJamRanToLength_StartsNewJam()
    {
        State = new JamClockState(true, 0, 0, 0);

        var newJamStartTick = JamClock.JamLengthInTicks + LineupClock.LineupDurationInTicks;

        await Subject.Handle(new JamStarted(newJamStartTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(newJamStartTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task JamEnded_WhenClockRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task JamEnded_WhenJamNotRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(false, randomTick, randomTick, randomTick / 1000);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(randomTick);
        State.SecondsPassed.Should().Be(randomTick / 1000);
    }

    [Test]
    public async Task TimeoutStarted_WhenJamRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new TimeoutStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task Tick_WhenStillTimeInJam_UpdatesTicksPassed()
    {
        State = new(true, 0, 0, 0);
        await Tick(10000);

        State.TicksPassed.Should().Be(10000);
        State.SecondsPassed.Should().Be(10);
    }

    [Test]
    public async Task Tick_WhenOverJamTimeLimit_SendsJamEndedEvent()
    {
        State = new(true, 0, 0, 0);

        var result = await Tick(130 * 1000);

        var implicitEvent = result.Should().ContainSingle().Which.Should().BeAssignableTo<JamEnded>().Which;
        implicitEvent.Tick.Should().Be(120 * 1000);
    }
}
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TimeoutClockUnitTests : ReducerUnitTest<TimeoutClock, TimeoutClockState>
{
    [Test]
    public async Task JamStarted_WhenLineupClockIsRunning_StopsClock()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(true, randomTick, 0, 0, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 10000);

        await Subject.Handle(new JamStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
        State.EndTick.Should().Be(secondRandomTick);
        State.TicksPassed.Should().Be(secondRandomTick - randomTick);
    }

    [Test]
    public async Task JamStarted_WhenLineupClockNotRunning_DoesNotChangeState()
    {
        State = (TimeoutClockState)Subject.GetDefaultState();

        var initialState = State;

        await Subject.Handle(new JamStarted(10000));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task TimeoutStarted_StartsNewTimeout()
    {
        State = new(false, 0, 0, 0, 0);

        var randomTick = Random.Shared.Next(10000, 200000);

        await Subject.Handle(new TimeoutStarted(randomTick));

        State.StartTick.Should().Be(randomTick);
    }

    [Test]
    public async Task TimeoutEnded_WhenClockRunningAndEndTickIsZero_SetsEndTick()
    {
        State = new(true, 10000, 0, 20000, 20);
        var initialState = State;

        await Subject.Handle(new TimeoutEnded(30000));

        State.Should().Be(initialState with { EndTick = 30000 });
    }

    [Test]
    public async Task TimeoutEnded_WhenClockRunningAndEndTickIsNonZero_DoesNotChangeState()
    {
        State = new(true, 10000, 30000, 40000, 40);
        var initialState = State;

        await Subject.Handle(new TimeoutEnded(50000));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task TimeoutEnded_WhenClockNotRunning_DoesNotChangeState()
    {
        State = new(false, 0, 0, 0, 0);
        var initialState = State;

        var randomTick = Random.Shared.Next((int)initialState.StartTick + 10000, (int)initialState.StartTick + 100000);

        await Subject.Handle(new TimeoutEnded(randomTick));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task TimeoutClockSet_SetsTimeoutClock()
    {
        State = new TimeoutClockState(true, 0, 0, 10000, 10);

        await Subject.Handle(new TimeoutClockSet(20000, new(30)));

        State.StartTick.Should().Be(20000 - 30000);
        State.TicksPassed.Should().Be(30000);
        State.SecondsPassed.Should().Be(30);
    }

    [Test]
    public async Task Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        State = new(true, 0, 0, 0, 0);
        await Tick(10000);

        State.TicksPassed.Should().Be(10000);
    }

    [Test]
    public async Task Tick_ClockStopped_DoesNotChangeState()
    {
        State = new(false, 0, 0, 0, 0);
        await Tick(130 * 1000);

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(0);
        State.TicksPassed.Should().Be(0);
    }
}
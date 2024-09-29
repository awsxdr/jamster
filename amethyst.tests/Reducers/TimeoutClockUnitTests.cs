using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Moq;

namespace amethyst.tests.Reducers;

public class TimeoutClockUnitTests : UnitTest<TimeoutClock>
{
    private TimeoutClockState _state;

    protected override void Setup()
    {
        base.Setup();

        _state = (TimeoutClockState)Subject.GetDefaultState();

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<TimeoutClockState>())
            .Returns(() => _state);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<TimeoutClockState>()))
            .Callback((TimeoutClockState s) => _state = s);
    }

    [Test]
    public void JamStarted_WhenLineupClockIsRunning_StopsClock()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        _state = new(true, randomTick, 0, 0, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 10000);

        Subject.Handle(new JamStarted(secondRandomTick));

        _state.IsRunning.Should().BeFalse();
        _state.StartTick.Should().Be(randomTick);
        _state.EndTick.Should().Be(secondRandomTick);
        _state.TicksPassed.Should().Be(secondRandomTick - randomTick);
    }

    [Test]
    public void JamStarted_WhenLineupClockNotRunning_DoesNotChangeState()
    {
        _state = (TimeoutClockState)Subject.GetDefaultState();

        var initialState = _state;

        Subject.Handle(new JamStarted(10000));

        _state.Should().Be(initialState);
    }

    [Test]
    public void TimeoutStarted_StartsNewTimeout()
    {
        _state = new(false, 0, 0, 0, 0);

        var randomTick = Random.Shared.Next(10000, 200000);

        Subject.Handle(new TimeoutStarted(randomTick));

        _state.StartTick.Should().Be(randomTick);
    }

    [Test]
    public void TimeoutEnded_WhenClockRunningAndEndTickIsZero_SetsEndTick()
    {
        _state = new(true, Random.Shared.Next(0, 100000), 0, 0, 0);
        var initialState = _state;

        var randomTick = Random.Shared.Next((int)initialState.StartTick + 10000, (int)initialState.StartTick + 100000);

        Subject.Handle(new TimeoutEnded(randomTick));

        _state.Should().Be(initialState with { EndTick = randomTick });
    }

    [Test]
    public void TimeoutEnded_WhenClockRunningAndEndTickIsNonZero_DoesNotChangeState()
    {
        _state = new(true, Random.Shared.Next(0, 100000), Random.Shared.Next(100001, 200000), 0, 0);
        var initialState = _state;

        var randomTick = Random.Shared.Next((int)initialState.StartTick + 10000, (int)initialState.StartTick + 100000);

        Subject.Handle(new TimeoutEnded(randomTick));

        _state.Should().Be(initialState);
    }

    [Test]
    public void TimeoutEnded_WhenClockNotRunning_DoesNotChangeState()
    {
        _state = new(false, 0, 0, 0, 0);
        var initialState = _state;

        var randomTick = Random.Shared.Next((int)initialState.StartTick + 10000, (int)initialState.StartTick + 100000);

        Subject.Handle(new TimeoutEnded(randomTick));

        _state.Should().Be(initialState);
    }

    [Test]
    public void Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        _state = new(true, 0, 0, 0, 0);
        Subject.Tick(10000, 10000);

        _state.TicksPassed.Should().Be(10000);
    }

    [Test]
    public void Tick_ClockStopped_DoesNotChangeState()
    {
        _state = new(false, 0, 0, 0, 0);
        Subject.Tick(130 * 1000, 130 * 1000);

        _state.IsRunning.Should().BeFalse();
        _state.StartTick.Should().Be(0);
        _state.TicksPassed.Should().Be(0);
    }
}
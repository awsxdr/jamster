using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Moq;

namespace amethyst.tests.Reducers;

public class LineupClockUnitTests : UnitTest<LineupClock>
{
    private LineupClockState _state;

    protected override void Setup()
    {
        base.Setup();

        _state = (LineupClockState)Subject.GetDefaultState();

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<LineupClockState>())
            .Returns(() => _state);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<LineupClockState>()))
            .Callback((LineupClockState s) => _state = s);
    }

    [Test]
    public void JamStart_WhenClockRunning_StopsLineup()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        _state = new(true, randomTick, randomTick);

        var secondRandomTick = Random.Shared.Next(0, 100000);

        Subject.Handle(new JamStarted(randomTick));

        _state.IsRunning.Should().BeFalse();
        _state.StartTick.Should().Be(randomTick);
    }

    [Test]
    public void JamStart_WhenClockStopped_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        _state = new(false, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 10000);
        Subject.Handle(new JamStarted(secondRandomTick));

        _state.IsRunning.Should().BeFalse();
    }

    [Test]
    public void JamEnded_WhenClockStopped_StartsLineup()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        _state = new(false, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        _state.IsRunning.Should().BeTrue();
        _state.StartTick.Should().Be(secondRandomTick);
        _state.TicksPassed.Should().Be(0);
    }

    [Test]
    public void JamEnded_WhenLineupAlreadyRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        _state = new(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        _state.IsRunning.Should().BeTrue();
        _state.StartTick.Should().Be(randomTick);
        _state.TicksPassed.Should().Be(randomTick);
    }

    [Test]
    public void Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        _state = new(true, 0, 0);
        Subject.Tick(10000, 10000);

        _state.TicksPassed.Should().Be(10000);
    }

    [Test]
    public void Tick_ClockStopped_DoesNotChangeState()
    {
        _state = new(false, 0, 0);
        Subject.Tick(130 * 1000, 130 * 1000);

        _state.IsRunning.Should().BeFalse();
        _state.StartTick.Should().Be(0);
        _state.TicksPassed.Should().Be(0);
    }
}
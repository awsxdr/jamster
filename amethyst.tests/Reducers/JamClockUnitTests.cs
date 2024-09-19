using amethyst.DataStores;

namespace amethyst.tests.Reducers;

using amethyst.Reducers;
using Events;
using FluentAssertions;
using Moq;
using Services;

public class JamClockUnitTests : UnitTest<JamClock>
{
    private JamClockState _state;

    protected override void Setup()
    {
        base.Setup();

        _state = (JamClockState) Subject.GetDefaultState();

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<JamClockState>())
            .Returns(() => _state);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<JamClockState>()))
            .Callback((JamClockState s) => _state = s);
    }

    [Test]
    public void JamStart_WhenClockStopped_StartsClock()
    {
        _state = new(false, 0, 0);

        var randomTick = Random.Shared.Next(0, 100000);

        Subject.Handle(new JamStarted(randomTick));

        _state.IsRunning.Should().BeTrue();
        _state.StartTick.Should().Be(randomTick);
        _state.TicksPassed.Should().Be(0);
    }

    [Test]
    public void JamStart_WhenClockRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        _state = new JamClockState(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamStarted(secondRandomTick));

        _state.IsRunning.Should().BeTrue();
        _state.StartTick.Should().Be(randomTick);
        _state.TicksPassed.Should().Be(randomTick);
    }

    [Test]
    public void JamEnded_WhenClockRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        _state = new(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        _state.IsRunning.Should().BeFalse();
    }

    [Test]
    public void JamEnded_WhenJamNotRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        _state = new(false, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        Subject.Handle(new JamEnded(secondRandomTick));

        _state.IsRunning.Should().BeFalse();
        _state.StartTick.Should().Be(randomTick);
        _state.TicksPassed.Should().Be(randomTick);
    }

    [Test]
    public void Tick_WhenStillTimeInJam_UpdatesTicksPassed()
    {
        _state = new(true, 0, 0);
        Subject.Tick(10000, 10000);

        _state.TicksPassed.Should().Be(10000);
    }

    [Test]
    public void Tick_WhenOverJamTimeLimit_SendsJamEndedEvent()
    {
        _state = new(true, 0, 0);
        Subject.Tick(130 * 1000, 130 * 1000);

        GetMock<IEventBus>()
            .Verify(mock => mock.AddEvent(
                It.IsAny<GameInfo>(), 
                It.Is<JamEnded>(e => e.Tick == 120 * 1000)
            ), Times.Once);
    }
}
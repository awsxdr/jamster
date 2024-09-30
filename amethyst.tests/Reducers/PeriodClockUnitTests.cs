using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Moq;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.Reducers;

public class PeriodClockUnitTests : UnitTest<PeriodClock>
{
    private PeriodClockState _state;

    protected override void Setup()
    {
        base.Setup();

        _state = (PeriodClockState)Subject.GetDefaultState();

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<PeriodClockState>())
            .Returns(() => _state);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<PeriodClockState>()))
            .Callback((PeriodClockState s) => _state = s);
    }

    [Test]
    public void JamStart_WhenPeriodClockStopped_StartsPeriod()
    {
        var randomTick = GetRandomTick();

        Subject.Handle(new JamStarted(randomTick));

        _state.IsRunning.Should().BeTrue();
        _state.LastStartTick.Should().Be(randomTick);
        _state.TicksPassed.Should().Be(0);
        _state.SecondsPassed.Should().Be(0);
        _state.TicksPassedAtLastStart.Should().Be(0);
    }
}
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Moq;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.Reducers;

public class PeriodClockUnitTests : ReducerUnitTest<PeriodClock, PeriodClockState>
{
    [Test]
    public async Task JamStart_WhenPeriodClockStopped_StartsPeriod()
    {
        var randomTick = GetRandomTick();

        await Subject.Handle(new JamStarted(randomTick));

        State.IsRunning.Should().BeTrue();
        State.LastStartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
        State.TicksPassedAtLastStart.Should().Be(0);
    }

    [Test]
    public async Task JamStart_WhenPeriodClockRunning_DoesNotChangeState()
    {
        var randomTick = GetRandomTick();
        var ticksPassed = Random.Shared.Next(1000);
        State = new(true, randomTick, ticksPassed, 1234, 1);

        var originalState = State;

        await Subject.Handle(new JamStarted(GetRandomTickFollowing(randomTick)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockExpired_StopsPeriodClock()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 10000;

        State = new(
            true, 
            lastStartTick, 
            ticksPassedAtLastStart, 
            ticksPassedAtLastStart + 12000,
            (int) ((ticksPassedAtLastStart + 12000) / 1000));

        await Subject.Handle(new JamEnded(lastStartTick + 12100));

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(ticksPassedAtLastStart + 12100);
        State.SecondsPassed.Should().Be((int) (PeriodClock.PeriodLengthInTicks / 1000) + 2);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockExpired_SendsPeriodEndedEvent()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 10000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        await Subject.Handle(new JamEnded(lastStartTick + 12100));

        GetMock<IEventBus>()
            .Verify(mock => mock.AddEvent(
                It.IsAny<GameInfo>(),
                It.Is<PeriodEnded>(e => e.Tick == lastStartTick + 12100)
            ), Times.Once);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockNotExpired_DoesNotChangeState()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 20000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        var originalState = State;

        await Subject.Handle(new JamEnded(lastStartTick + 12100));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 20000;

        State = new(
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        var originalState = State;

        await Subject.Handle(new JamEnded(lastStartTick + 12100));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TimeoutStarted_WhenPeriodClockRunning_StopsPeriodClock()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 20000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        await Subject.Handle(new TimeoutStarted(lastStartTick + 12100));

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(ticksPassedAtLastStart + 12100);
        State.SecondsPassed.Should().Be((int)(PeriodClock.PeriodLengthInTicks / 1000) - 8);
    }

    [Test]
    public async Task TimeoutStarted_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks - 20000;

        State = new(
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        var originalState = State;

        await Subject.Handle(new TimeoutStarted(lastStartTick + 12100));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TimeoutEnded_WhenLineupStartedWhenTimeRemainingLessThanLineupDuration_StartsPeriodClock()
    {
        MockState(new LineupClockState(false, PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks + 1000, 1000, 1));

        State = new(
            false,
            0,
            0,
            PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks + 1000,
            (int) ((PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks + 1000) / 1000));

        await Subject.Handle(new TimeoutEnded(PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks / 2));

        State.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task TimeoutEnded_WhenTimeRemainingMoreThanLineupDuration_DoesNotStartPeriodClock()
    {
        MockState(new LineupClockState(false, PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks - 1000, 1000, 1));

        var ticksPassed = PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks - 1000;

        State = new(
            false,
            0,
            ticksPassed,
            ticksPassed,
            ticksPassed.Seconds
        );

        await Subject.Handle(new TimeoutEnded(PeriodClock.PeriodLengthInTicks - LineupClock.LineupDurationInTicks + 2000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task PeriodFinalized_ResetsPeriodClock()
    {
        State = new(false, 0, 0, PeriodClock.PeriodLengthInTicks, (int) (PeriodClock.PeriodLengthInTicks / 1000));

        await Subject.Handle(new PeriodFinalized(PeriodClock.PeriodLengthInTicks + 30000));

        State.IsRunning.Should().BeFalse();
        State.TicksPassedAtLastStart.Should().Be(0);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task PeriodFinalized_WhenPeriodNotExpired_EndsPeriod()
    {
        State = new(true, 0, 0, 0, 0);

        await Subject.Handle(new PeriodFinalized(10000));

        VerifyEventSent<PeriodEnded>(10000);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockNotExpired_UpdatesTicksPassedBasedOnLastStartTick()
    {
        MockState(new JamClockState(true, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks / 2;
        var ticksPassed = PeriodClock.PeriodLengthInTicks / 3;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            ticksPassed.Seconds
        );

        await ((ITickReceiver)Subject).Tick(lastStartTick + ticksPassed + 1000);

        State.IsRunning.Should().BeTrue();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(ticksPassedAtLastStart + ticksPassed + 1000);
        State.SecondsPassed.Should().Be((int) ((ticksPassedAtLastStart + ticksPassed) / 1000) + 1);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockExpired_AndJamRunning_SetsTicksPassedToPeriodDuration_AndDoesNotStopClock()
    {
        MockState(new JamClockState(true, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks / 2;
        var ticksPassed = PeriodClock.PeriodLengthInTicks / 2 + 10000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        await ((ITickReceiver)Subject).Tick(lastStartTick + ticksPassed + 1000);

        State.IsRunning.Should().BeTrue();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(PeriodClock.PeriodLengthInTicks);
        State.SecondsPassed.Should().Be((int)(PeriodClock.PeriodLengthInTicks / 1000));
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockExpired_AndJamNotRunning_RaisesPeriodEndedEvent()
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<JamClockState>())
            .Returns(new JamClockState(false, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks / 2;
        var ticksPassed = PeriodClock.PeriodLengthInTicks / 2 + 10000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        var updateTick = lastStartTick + ticksPassed + 1000;
        await ((ITickReceiver)Subject).Tick(updateTick);

        GetMock<IEventBus>()
            .Verify(mock => mock.AddEvent(It.IsAny<GameInfo>(),  It.Is<PeriodEnded>(e => e.Tick == updateTick)), Times.Once);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockExpired_AndJamNotRunning_StopsPeriodClock()
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<JamClockState>())
            .Returns(new JamClockState(false, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = PeriodClock.PeriodLengthInTicks / 2;
        var ticksPassed = PeriodClock.PeriodLengthInTicks / 2 + 10000;

        State = new(
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        await ((ITickReceiver)Subject).Tick(lastStartTick + ticksPassed + 1000);

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(PeriodClock.PeriodLengthInTicks);
        State.SecondsPassed.Should().Be((int)(PeriodClock.PeriodLengthInTicks / 1000));
    }

    [Test]
    public async Task Tick_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        State = new(
            false,
            GetRandomTick(),
            PeriodClock.PeriodLengthInTicks / 2,
            PeriodClock.PeriodLengthInTicks / 2,
            (int) (PeriodClock.PeriodLengthInTicks / 2 / 1000));

        var originalState = State;

        await ((ITickReceiver)Subject).Tick(State.LastStartTick + 1);

        State.Should().Be(originalState);
    }
}
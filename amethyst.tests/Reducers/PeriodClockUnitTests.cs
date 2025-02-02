using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;

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
    public async Task JamStart_WhenPeriodClockStopped_AlignsSecondsToStartTick()
    {
        State = new(false, false, 0, 1234, 1234, 1);

        await Subject.Handle(new JamStarted(10789));

        State.LastStartTick.Should().Be(10789);
        State.TicksPassedAtLastStart.Should().Be(1000);
        State.TicksPassed.Should().Be(1000);
        State.SecondsPassed.Should().Be(1);
    }

    [Test]
    public async Task JamStart_WhenPeriodClockRunning_DoesNotChangeState()
    {
        Tick ticksPassed = Random.Shared.Next(1000);
        State = new(true, false, 0, 0, ticksPassed, ticksPassed.Seconds);

        MockState(new JamClockState(false, 0, ticksPassed, ticksPassed.Seconds));

        var originalState = State;

        await Subject.Handle(new JamStarted(ticksPassed));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockExpired_StopsPeriodClock()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength - 10000;

        State = new(
            true, 
            false,
            lastStartTick, 
            ticksPassedAtLastStart, 
            ticksPassedAtLastStart + 12000,
            (int) ((ticksPassedAtLastStart + 12000) / 1000));

        MockState(new JamClockState(true, 0, 10000, 10));

        await Subject.Handle(new JamEnded(lastStartTick + 12100));

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(ticksPassedAtLastStart + 12100);
        State.SecondsPassed.Should().Be(periodLength.Seconds + 2);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockExpired_SendsPeriodEndedEvent()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var ticksPassedAtLastStart = periodLength - 10000;

        State = new(
            true,
            false,
            0,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        MockState(new JamClockState(false, 0, 0, 0));

        var result = await Subject.Handle(new JamEnded(12100));

        result.Should().ContainSingle().Which.Should().BeOfType<PeriodEnded>().Which.Tick.Should().Be(12100);
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockRunning_AndPeriodClockNotExpired_DoesNotChangeState()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength - 20000;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        var originalState = State;

        await ((ITickReceiver)Subject).TickAsync(lastStartTick + 12100);
        await Subject.Handle(new JamEnded(lastStartTick + 12101));

        State.Should().Be(originalState with { TicksPassed = ticksPassedAtLastStart + 12100 /* Ticks passed will update due to call to Tick */});
    }

    [Test]
    public async Task JamEnded_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = Rules.DefaultRules.PeriodRules.Duration - 20000;

        State = new(
            false,
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
    public async Task TimeoutStarted_WhenPeriodClockRunning_StopsPeriodClock()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength - 20000;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        await Subject.Handle(new TimeoutStarted(lastStartTick + 12100));

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(ticksPassedAtLastStart + 12100);
        State.SecondsPassed.Should().Be(periodLength.Seconds - 8);
    }

    [Test]
    public async Task TimeoutStarted_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = Rules.DefaultRules.PeriodRules.Duration - 20000;

        State = new(
            false,
            true,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassedAtLastStart + 12000,
            (int)((ticksPassedAtLastStart + 12000) / 1000));

        var originalState = State;

        await Subject.Handle(new TimeoutStarted(lastStartTick + 12100));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TimeoutStarted_WhenAllTimeoutTypesDoNotStopClockDueToRules_DoesNotStopClock()
    {
        var originalState = State = new(true, false, 0, 0, 0, 0);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with
            {
                PeriodClockBehavior = TimeoutPeriodClockStopBehavior.OfficialReview
            }
        }));

        await Subject.Handle(new TimeoutStarted(10000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TimeoutTypeSet_WhenTimeoutTypeShouldStopPeriodClock_StopsPeriodClockAtTimeoutStart()
    {
        State = new(true, false, 0, 0, 10000, 10);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with
            {
                PeriodClockBehavior = TimeoutPeriodClockStopBehavior.OfficialTimeout
            }
        }));
        MockState<TimeoutTypeState>(new(CompoundTimeoutType.Untyped, 7000));

        await Subject.Handle(new TimeoutTypeSet(12000, new(TimeoutType.Official, TeamSide.Home)));

        State.Should().Be(new PeriodClockState(false, false, 0, 0, 7000, 7));
    }

    [Test]
    public async Task TimeoutEnded_WhenPeriodExpired_SendsPeriodEnded()
    {
        State = State with { HasExpired = true, IsRunning = false };

        var implicitEvents = await Subject.Handle(new TimeoutEnded(Rules.DefaultRules.PeriodRules.Duration + 10000));

        implicitEvents.OfType<PeriodEnded>().Should().ContainSingle();
    }

    [Test]
    public async Task PeriodFinalized_ResetsPeriodClock()
    {
        State = new(false, true, 0, 0, Rules.DefaultRules.PeriodRules.Duration, (int) (Rules.DefaultRules.PeriodRules.Duration / 1000));

        await Subject.Handle(new PeriodFinalized(Rules.DefaultRules.PeriodRules.Duration + 30000));

        State.IsRunning.Should().BeFalse();
        State.TicksPassedAtLastStart.Should().Be(0);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task PeriodFinalized_WhenPeriodNotExpired_EndsPeriod()
    {
        State = new(true, false, 0, 0, 0, 0);

        var result = await Subject.Handle(new PeriodFinalized(10000));

        var implicitEvent = result.Should().ContainSingle().Which.Should().BeAssignableTo<PeriodEnded>().Which;
        implicitEvent.Tick.Should().Be(10000);
    }

    [Test]
    public async Task PeriodClockSet_SetsPeriodClock()
    {
        State = new(true, false, 30000, 40000, 100000, 100);
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        await Subject.Handle(new PeriodClockSet(120000, new(30)));

        (periodLength - State.TicksPassed).Should().Be(30000);
        (periodLength.Seconds - State.SecondsPassed).Should().Be(30);
    }

    [TestCase(true, 0, true)]
    [TestCase(true, 10, false)]
    [TestCase(false, 0, true)]
    [TestCase(false, 10, false)]
    public async Task PeriodClockSet_SetsHasExpiredAsExpected(bool initialHasExpired, int secondsRemaining, bool expectedHasExpired)
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        State = new(
            true, 
            initialHasExpired, 
            0, 
            initialHasExpired ? periodLength : 0,
            initialHasExpired ? periodLength : 0,
            initialHasExpired ? periodLength.Seconds : 0);

        await Subject.Handle(new PeriodClockSet(10000, new(secondsRemaining)));

        State.HasExpired.Should().Be(expectedHasExpired);
    }

    [Test]
    public async Task PeriodClockSet_FollowedByTick_RespectsPeriodClockValue()
    {
        State = new(true, false, 0, 10000, 20000, 20);
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        await Subject.Handle(new PeriodClockSet(30000, new(10)));

        (periodLength.Seconds - State.SecondsPassed).Should().Be(10);

        await Tick(31000);

        (periodLength.Seconds - State.SecondsPassed).Should().Be(9);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockNotExpired_UpdatesTicksPassedBasedOnLastStartTick()
    {
        MockState(new JamClockState(true, 0, 0, 0));
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength / 2;
        var ticksPassed = periodLength / 3;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            ticksPassed.Seconds
        );

        await Tick(lastStartTick + ticksPassed + 1000);

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
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength / 2;
        var ticksPassed = periodLength / 2 + 10000;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        await Tick(lastStartTick + ticksPassed + 1000);

        State.IsRunning.Should().BeTrue();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(periodLength);
        State.SecondsPassed.Should().Be(periodLength.Seconds);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockExpired_AndJamNotRunning_RaisesPeriodEndedEvent()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<JamClockState>())
            .Returns(new JamClockState(false, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength / 2;
        var ticksPassed = periodLength / 2 + 10000;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        var updateTick = lastStartTick + ticksPassed + 1000;
        var result = await Tick(updateTick);

        var implicitEvent = result.Should().ContainSingle().Which.Should().BeAssignableTo<PeriodEnded>().Which;
        implicitEvent.Tick.Should().Be(lastStartTick + periodLength / 2);
    }

    [Test]
    public async Task Tick_WhenPeriodClockRunning_AndPeriodClockExpired_AndJamNotRunning_StopsPeriodClock()
    {
        var periodLength = Domain.Tick.FromSeconds(1234);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            PeriodRules = Rules.DefaultRules.PeriodRules with
            {
                Duration = periodLength,
            }
        }));

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<JamClockState>())
            .Returns(new JamClockState(false, 0, 0, 0));

        var lastStartTick = GetRandomTick();
        var ticksPassedAtLastStart = periodLength / 2;
        var ticksPassed = periodLength / 2 + 10000;

        State = new(
            true,
            false,
            lastStartTick,
            ticksPassedAtLastStart,
            ticksPassed,
            (int)(ticksPassed / 1000)
        );

        await Tick(lastStartTick + ticksPassed + 1000);

        State.IsRunning.Should().BeFalse();
        State.LastStartTick.Should().Be(lastStartTick);
        State.TicksPassedAtLastStart.Should().Be(ticksPassedAtLastStart);
        State.TicksPassed.Should().Be(periodLength);
        State.SecondsPassed.Should().Be(periodLength.Seconds);
    }

    [Test]
    public async Task Tick_WhenPeriodClockNotRunning_DoesNotChangeState()
    {
        State = new(
            false,
            true,
            GetRandomTick(),
            Rules.DefaultRules.PeriodRules.Duration / 2,
            Rules.DefaultRules.PeriodRules.Duration / 2,
            (int) (Rules.DefaultRules.PeriodRules.Duration / 2 / 1000));

        var originalState = State;

        await Tick(State.LastStartTick + 1);

        State.Should().Be(originalState);
    }
}
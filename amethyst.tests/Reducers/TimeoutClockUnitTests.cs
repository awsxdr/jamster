using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TimeoutClockUnitTests : ReducerUnitTest<TimeoutClock, TimeoutClockState>
{
    [Test]
    public async Task JamStarted_WhenClockRunning_AndEndTickNotSet_SendsTimeoutEndedEvent()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(true, randomTick, 0, TimeoutClockStopReason.None, 0, 0);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 10000);

        var implicitEvents = await Subject.Handle(new JamStarted(secondRandomTick));

        implicitEvents.OfType<TimeoutEnded>().Should().ContainSingle().Which.Tick.Should().Be(secondRandomTick);
    }

    [Test]
    public async Task JamStarted_WhenClockNotRunning_DoesNotSendTimeoutEndedEvent()
    {
        State = (TimeoutClockState)Subject.GetDefaultState();

        var implicitEvents = await Subject.Handle(new JamStarted(10000));

        implicitEvents.OfType<TimeoutEnded>().Should().BeEmpty();
    }

    [Test]
    public async Task JamStarted_WhenClockRunning_AndEndTickSet_DoesNotSendTimeoutEndedEvent()
    {
        State = new(true, 0, 9000, TimeoutClockStopReason.Other, 9000, 9);

        var implicitEvents = await Subject.Handle(new JamStarted(10000));

        implicitEvents.OfType<TimeoutEnded>().Should().BeEmpty();
    }

    [Test]
    public async Task JamStarted_WhenClock_RegardlessOfEndTickSet_StopsTimeoutClock([Values] bool endTickSet)
    {
        State = new(true, 0, endTickSet ? 9000 : 0, endTickSet ? TimeoutClockStopReason.Other : TimeoutClockStopReason.None, endTickSet ? 9000 : 10000, endTickSet ? 9 : 10);

        await Subject.Handle(new JamStarted(10000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutStarted_StartsNewTimeout()
    {
        State = new(false, 0, 0, TimeoutClockStopReason.None, 0, 0);

        var randomTick = Random.Shared.Next(10000, 200000);

        await Subject.Handle(new TimeoutStarted(randomTick));

        State.StartTick.Should().Be(randomTick);
    }

    [Test]
    public async Task TimeoutEnded_WhenClockRunningAndEndTickIsZero_SetsEndTick()
    {
        State = new(true, 10000, 0, TimeoutClockStopReason.None, 20000, 20);
        MockState(new PeriodClockState(false, false, 0, 0, 0, 0));
        var initialState = State;

        await Subject.Handle(new TimeoutEnded(30000));

        State.Should().Be(initialState with { EndTick = 30000, StopReason = TimeoutClockStopReason.Other });
    }

    [Test]
    public async Task TimeoutEnded_WhenClockRunningAndEndTickIsNonZero_DoesNotChangeState()
    {
        State = new(true, 10000, 30000, TimeoutClockStopReason.Other, 40000, 40);
        var initialState = State;

        await Subject.Handle(new TimeoutEnded(50000));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task PeriodEnded_StopsClock()
    {
        State = new(true, 0, 0, TimeoutClockStopReason.None, 0, 0);

        await Subject.Handle(new PeriodEnded(10000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutClockSet_SetsTimeoutClock()
    {
        State = new TimeoutClockState(true, 0, 0, TimeoutClockStopReason.None, 10000, 10);

        await Subject.Handle(new TimeoutClockSet(20000, new(30)));

        State.StartTick.Should().Be(20000 - 30000);
        State.TicksPassed.Should().Be(30000);
        State.SecondsPassed.Should().Be(30);
    }

    [Test]
    public async Task PeriodFinalized_StopsClock()
    {
        State = new TimeoutClockState(true, 0, 0, TimeoutClockStopReason.None, 10000, 10);

        await Subject.Handle(new PeriodFinalized(11000));

        State.Should().Be(new TimeoutClockState(false, 0, 11000, TimeoutClockStopReason.PeriodFinalized, 11000, 11));
    }

    [Test]
    public async Task Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        State = new(true, 0, 0, TimeoutClockStopReason.None, 0, 0);
        await Tick(10000);

        State.TicksPassed.Should().Be(10000);
    }

    [Test]
    public async Task Tick_ClockStopped_DoesNotChangeState()
    {
        State = new(false, 0, 0, TimeoutClockStopReason.None, 0, 0);
        await Tick(130 * 1000);

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(0);
        State.TicksPassed.Should().Be(0);
    }

    [TestCase(TimeoutType.Official, false, true, TimeoutClockStopReason.None, 0, 20000)]
    [TestCase(TimeoutType.Team, false, true, TimeoutClockStopReason.None, 0, 20000)]
    [TestCase(TimeoutType.Official, true, true, TimeoutClockStopReason.None, 0, 20000)]
    [TestCase(TimeoutType.Team, true, false, TimeoutClockStopReason.PeriodExpired, 10000, 10000)]
    public async Task TimeoutTypeSet_SetsStateCorrectly(
        TimeoutType timeoutType,
        bool periodHasExpired,
        bool expectedIsRunning,
        TimeoutClockStopReason expectedStopReason,
        int expectedEndTick,
        int expectedDuration)
    {
        State = new(true, 0, 0, TimeoutClockStopReason.None, 20000, 20);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with
            {
                PeriodClockBehavior = TimeoutPeriodClockStopBehavior.OfficialTimeout
            }
        }));
        MockState<TimeoutTypeState>(new(timeoutType == TimeoutType.Official ? CompoundTimeoutType.HomeTeamTimeout : CompoundTimeoutType.OfficialTimeout, 0));
        MockState<PeriodClockState>(new(!periodHasExpired, periodHasExpired, 0, 0, 20000, 20));

        await Subject.Handle(new TimeoutTypeSet(10000, new(timeoutType, TeamSide.Home)));
        await Tick(20000);

        State.Should().Be(new TimeoutClockState(
            expectedIsRunning,
            0,
            expectedEndTick,
            expectedStopReason,
            expectedDuration,
            expectedDuration / Domain.Tick.TicksPerSecond
        ));
    }
}
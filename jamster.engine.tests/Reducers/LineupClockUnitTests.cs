using AwesomeAssertions;

using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

using DomainTick = jamster.engine.Domain.Tick;

using static jamster.engine.tests.DataGenerator;

namespace jamster.engine.tests.Reducers;

public class LineupClockUnitTests : ReducerUnitTest<LineupClock, LineupClockState>
{
    [Test]
    public async Task JamStart_WhenClockRunning_StopsLineup()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);

        await Subject.Handle(new JamStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
    }

    [Test]
    public async Task JamStart_WhenClockStopped_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(false, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 10000);
        await Subject.Handle(new JamStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task JamEnded_WhenClockStopped_AndPeriodClockNotExpired_StartsLineup()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(false, randomTick, randomTick);

        MockState(new TimeoutClockState(false, 0, 0, TimeoutClockStopReason.None, 0));
        MockState(new PeriodClockState(true, false, true, 0, 0, DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds - 10)));

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(secondRandomTick);
        State.TicksPassed.Should().Be(0);
    }

    [Test]
    public async Task JamEnded_WhenLineupAlreadyRunning_DoesNotChangeState()
    {
        var randomTick = GetRandomTick();
        State = new(true, randomTick, 0);

        var secondTick = randomTick + 100000;
        var ticksPassed = secondTick - randomTick;

        await ((ITickReceiver)Subject).TickAsync(secondTick);
        await Subject.Handle(new JamEnded(secondTick + 1));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(ticksPassed);
        State.SecondsPassed.Should().Be(ticksPassed.Seconds);
    }

    [Test]
    public async Task PeriodEnded_WhenClockRunning_StopsLineupClock()
    {
        var randomTick = Random.Shared.Next(0, 100000);
        State = new(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new PeriodEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
    }

    [Test]
    public async Task PeriodEnded_WhenClockStopped_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);
        State = new(false, randomTick, randomTick);

        var originalState = State;

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new PeriodEnded(secondRandomTick));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task LineupClockSet_SetsLineupClock()
    {
        State = new LineupClockState(true, 0, 10000);

        await Subject.Handle(new LineupClockSet(20000, new(30)));

        State.StartTick.Should().Be(20000 - 30000);
        State.TicksPassed.Should().Be(30000);
        State.SecondsPassed.Should().Be(30);
    }

    [Test]
    public async Task Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        State = new(true, 0, 0);
        await Tick(10000);

        State.TicksPassed.Should().Be(10000);
    }

    [Test]
    public async Task Tick_ClockStopped_DoesNotChangeState()
    {
        State = new(false, 0, 0);
        await Tick(130 * 1000);

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(0);
        State.TicksPassed.Should().Be(0);
    }

    [Test]
    public async Task TimeoutStarted_WhenClockRunning_StopsLineup()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(true, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);

        await Subject.Handle(new TimeoutStarted(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
    }

    [Test]
    public async Task TimeoutStarted_WhenClockStopped_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(0, 100000);
        State = new(false, randomTick, randomTick);

        var originalState = State;

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new TimeoutStarted(secondRandomTick));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamEnded_WhenTimeoutClockRunning_DoesNotStartLineup()
    {
        var randomTick = Random.Shared.Next(0, 100000);

        State = new(false, randomTick, randomTick);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<TimeoutClockState>())
            .Returns(() => new(true, 0, 0, TimeoutClockStopReason.None, 0));

        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task IntermissionEnded_StartsLineupClock()
    {
        MockState<JamClockState>(new(false, 0, 0, false, false));

        await Subject.Handle(new IntermissionEnded(10000));

        State.Should().Be(new LineupClockState(true, 10000, 0));
    }

    [Test]
    public async Task IntermissionEnded_WhenJamClockRunning_DoesNotStartLineupClock()
    {
        MockState<JamClockState>(new(true, 0, 0, false, false));

        var originalState = State;

        await Subject.Handle(new IntermissionEnded(10000));

        State.Should().Be(originalState);
    }

    [Test]
    public void State_ShouldSerializeCorrectly()
    {
        var state = new LineupClockState(true, 1234, 4321);

        var serialized = System.Text.Json.JsonSerializer.Serialize(state, Program.JsonSerializerOptions);

        var deserialized = System.Text.Json.Nodes.JsonNode.Parse(serialized)!;

        deserialized["isRunning"]!.AsValue().GetValue<bool>().Should().BeTrue();
        deserialized["startTick"]!.AsValue().GetValue<int>().Should().Be(1234);
        deserialized["ticksPassed"]!.AsValue().GetValue<int>().Should().Be(4321);
        deserialized["secondsPassed"]!.AsValue().GetValue<int>().Should().Be(4);
    }

    [Test]
    public async Task OvertimeStarted_WhenClockStopped_StartsLineupClock()
    {
        var tick = GetRandomTick();
        State = new LineupClockState(false, 0, 0);

        await Subject.Handle(new OvertimeStarted(tick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(tick);
        State.TicksPassed.Should().Be(0);
    }

    [Test]
    public async Task OvertimeStarted_WhenClockAlreadyRunning_DoesNotChangeState()
    {
        var tick = GetRandomTick();
        State = new LineupClockState(true, tick, tick / 2);

        var originalState = State;

        var overtimeTick = GetRandomTickFollowing(tick);
        await Subject.Handle(new OvertimeStarted(overtimeTick));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task OvertimeEnded_WhenClockRunning_StopsLineupClock()
    {
        var tick = GetRandomTick();
        State = new LineupClockState(true, tick, tick / 2);

        var overtimeEndTick = GetRandomTickFollowing(tick);
        await Subject.Handle(new OvertimeEnded(overtimeEndTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(tick);
    }

    [Test]
    public async Task OvertimeEnded_WhenClockStopped_DoesNotChangeState()
    {
        var tick = GetRandomTick();
        State = new LineupClockState(false, tick, tick / 2);

        var originalState = State;

        var overtimeEndTick = GetRandomTickFollowing(tick);
        await Subject.Handle(new OvertimeEnded(overtimeEndTick));

        State.Should().Be(originalState);
    }
}
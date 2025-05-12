using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using DomainTick = amethyst.Domain.Tick;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.Reducers;

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
        MockState(new PeriodClockState(true, false, 0, 0, DomainTick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds - 10)));

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
}
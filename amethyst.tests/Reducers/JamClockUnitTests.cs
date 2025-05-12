using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using DomainTick = amethyst.Domain.Tick;

using static amethyst.tests.DataGenerator;

namespace amethyst.tests.Reducers;

public class JamClockUnitTests : ReducerUnitTest<JamClock, JamClockState>
{
    [Test]
    public async Task JamStart_WhenClockStopped_StartsJam()
    {
        State = new(false, 0, 0, true, false);

        var randomTick = Random.Shared.Next(0, 100000);

        await Subject.Handle(new JamStarted(randomTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task JamStart_WhenClockRunning_DoesNotChangeState()
    {
        var randomTick = GetRandomTick();

        State = new JamClockState(true, randomTick, 0, true, false);
        MockState<RulesState>(new(Rules.DefaultRules));

        var secondTick = randomTick + 10000;
        var ticksPassed = secondTick - randomTick;

        await ((ITickReceiver)Subject).TickAsync(secondTick);
        await Subject.Handle(new JamStarted(secondTick + 1));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(ticksPassed);
        State.SecondsPassed.Should().Be(ticksPassed.Seconds);
    }

    [Test]
    public async Task JamStart_WhenClockRunning_AndPreviousJamRanToLength_StartsNewJam()
    {
        State = new JamClockState(true, 0, 0, true, false);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            JamRules = Rules.DefaultRules.JamRules with { DurationInSeconds = 33 },
            LineupRules = Rules.DefaultRules.LineupRules with { DurationInSeconds = 44 }
        }));

        var newJamStartTick = DomainTick.FromSeconds(33 + 44);

        await ((ITickReceiver)Subject).TickAsync(newJamStartTick - 1);
        await Subject.Handle(new JamStarted(newJamStartTick));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(newJamStartTick);
        State.TicksPassed.Should().Be(0);
        State.SecondsPassed.Should().Be(0);
    }

    [Test]
    public async Task JamEnded_WhenClockRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, true, false);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task JamEnded_WhenJamNotRunning_DoesNotChangeState()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(false, randomTick, randomTick, true, false);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        await Subject.Handle(new JamEnded(secondRandomTick));

        State.IsRunning.Should().BeFalse();
        State.StartTick.Should().Be(randomTick);
        State.TicksPassed.Should().Be(randomTick);
        State.SecondsPassed.Should().Be(randomTick / 1000);
    }

    [Test]
    public async Task JamEnded_WhenJamMarkedToNotExpire_SetsNextJamToAutoExpire()
    {
        State = new(true, 0, 100, false, false);

        await Subject.Handle(new JamEnded(0));

        State.AutoExpire.Should().BeTrue();
    }

    [Test]
    public async Task JamExpired_RaisesJamEndedEvent()
    {
        var implicitEvents = await Subject.Handle(new JamExpired(1234));

        implicitEvents.OfType<JamEnded>().Should().ContainSingle().Which.Tick.Should().Be(1234);
    }

    [Test]
    public async Task JamAutoExpiryDisabled_MarksCurrentJamToNotExpire()
    {
        State = new(true, 0, 100, true, false);

        await Subject.Handle(new JamAutoExpiryDisabled(0));

        State.AutoExpire.Should().BeFalse();
    }

    [Test]
    public async Task TimeoutStarted_WhenJamRunning_EndsJam()
    {
        var randomTick = Random.Shared.Next(1, 100000);
        State = new(true, randomTick, randomTick, true, false);

        var secondRandomTick = randomTick + Random.Shared.Next(1, 100000);
        var implicitEvents = await Subject.Handle(new TimeoutStarted(secondRandomTick));

        implicitEvents.OfType<JamEnded>().Should().ContainSingle().Which.Tick.Should().Be(secondRandomTick);
    }

    [Test]
    public async Task CallMarked_WhenJamRunning_AndCallTrue_EndsJam()
    {
        State = new(true, 0, 10000, true, false);

        var implicitEvents = await Subject.Handle(new CallMarked(20000, new(TeamSide.Home, true)));

        implicitEvents.Should().HaveCount(1)
            .And.Subject.Single().Should().BeOfType<JamEnded>()
            .Which.Tick.Should().Be(20000);
    }

    [Test]
    public async Task CallMarked_WhenJamNotRunning_DoesNotEndJam()
    {
        State = new(false, 0, 10000, true, false);

        var implicitEvents = await Subject.Handle(new CallMarked(20000, new(TeamSide.Home, true)));

        implicitEvents.Should().BeEmpty();
    }

    [Test]
    public async Task CallMarked_WhenJamRunning_AndCallFalse_DoesNotEndJam()
    {
        State = new(true, 0, 10000, true, false);

        var implicitEvents = await Subject.Handle(new CallMarked(20000, new(TeamSide.Home, false)));

        implicitEvents.Should().BeEmpty();
    }

    [Test]
    public async Task JamClockSet_SetsJamClock()
    {
        State = new JamClockState(true, 0, 10000, true, false);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            JamRules = Rules.DefaultRules.JamRules with
            {
                DurationInSeconds = 123
            }
        }));

        await Subject.Handle(new JamClockSet(20000, new(30)));

        State.StartTick.Should().Be(20000 - DomainTick.FromSeconds(123 - 30));
        State.TicksPassed.Should().Be(DomainTick.FromSeconds(123 - 30));
        State.SecondsPassed.Should().Be(DomainTick.FromSeconds(123 - 30).Seconds);
    }

    [Test]
    public async Task Tick_WhenStillTimeInJam_UpdatesTicksPassed()
    {
        State = new(true, 0, 0, true, false);
        MockState<RulesState>(new(Rules.DefaultRules));

        await Tick(10000);

        State.TicksPassed.Should().Be(10000);
        State.SecondsPassed.Should().Be(10);
    }

    [Test]
    public async Task Tick_WhenOverJamTimeLimit_AndJamSetToAutoExpire_SendsJamExpiredEvent()
    {
        State = new(true, 0, 0, true, true);
        MockState<RulesState>(new(Rules.DefaultRules));

        var result = await Tick(130 * 1000);

        result.OfType<JamExpired>().Should().ContainSingle().Which.Tick.Should().Be(120 * 1000);
    }

    [Test]
    public async Task Tick_WhenOverJamTimeLimit_AndJamNotSetToAutoExpire_DoesNotSendJamExpiredEvent()
    {
        State = new(true, 0, 0, false, false);
        MockState<RulesState>(new(Rules.DefaultRules));

        var result = await Tick(130 * 1000);

        result.OfType<JamExpired>().Should().BeEmpty();
    }

    [Test]
    public void State_ShouldSerializeCorrectly()
    {
        var state = new JamClockState(true, 1234, 4321, true, false);

        var serialized = System.Text.Json.JsonSerializer.Serialize(state, Program.JsonSerializerOptions);

        var deserialized = System.Text.Json.Nodes.JsonNode.Parse(serialized)!;

        deserialized["isRunning"]!.AsValue().GetValue<bool>().Should().BeTrue();
        deserialized["startTick"]!.AsValue().GetValue<int>().Should().Be(1234);
        deserialized["ticksPassed"]!.AsValue().GetValue<int>().Should().Be(4321);
        deserialized["secondsPassed"]!.AsValue().GetValue<int>().Should().Be(4);
        deserialized["autoExpire"]!.AsValue().GetValue<bool>().Should().BeTrue();
        deserialized["expired"]!.AsValue().GetValue<bool>().Should().BeFalse();
    }
}
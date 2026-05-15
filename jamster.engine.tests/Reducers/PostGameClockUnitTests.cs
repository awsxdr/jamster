using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class PostGameClockUnitTests : ReducerUnitTest<PostGameClock, PostGameClockState>
{
    [Test]
    public async Task PeriodEnded_WhenLastPeriod_StartsTheClock()
    {
        State = PostGameClockState.Default;
        MockState(new GameStageState(Stage.Jam, 2, 5, 10, false, false));
        MockState(new OvertimeState(false));
        MockState(new RulesState(Rules.DefaultRules));

        await Subject.Handle(new PeriodEnded(50000));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(50000);
        State.EndTick.Should().Be(0);
    }

    [Test]
    public async Task PeriodEnded_WhenNotLastPeriod_DoesNotStartTheClock()
    {
        State = PostGameClockState.Default;
        MockState(new GameStageState(Stage.Jam, 1, 5, 5, false, false));
        MockState(new OvertimeState(false));
        MockState(new RulesState(Rules.DefaultRules));

        await Subject.Handle(new PeriodEnded(50000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task PeriodEnded_WhenInOvertime_StartsTheClock()
    {
        State = PostGameClockState.Default;
        MockState(new GameStageState(Stage.Jam, 2, 1, 11, false, false));
        MockState(new OvertimeState(true));
        MockState(new RulesState(Rules.DefaultRules));

        await Subject.Handle(new PeriodEnded(60000));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(60000);
    }

    [Test]
    public async Task PeriodFinalized_WhenClockRunning_StopsClock()
    {
        State = new PostGameClockState(true, 50000, 0, 0);

        await Subject.Handle(new PeriodFinalized(80000));

        State.IsRunning.Should().BeFalse();
        State.EndTick.Should().Be(80000);
        State.TicksPassed.Should().Be(30000);
        State.SecondsPassed.Should().Be(30);
    }

    [Test]
    public async Task PeriodFinalized_WhenClockNotRunning_DoesNotChangeState()
    {
        State = PostGameClockState.Default;

        await Subject.Handle(new PeriodFinalized(80000));

        State.Should().Be(PostGameClockState.Default);
    }

    [Test]
    public async Task Tick_WhenClockRunning_UpdatesTicksPassed()
    {
        State = new PostGameClockState(true, 50000, 0, 0);

        await Tick(70000);

        State.TicksPassed.Should().Be(20000);
        State.SecondsPassed.Should().Be(20);
    }

    [Test]
    public async Task Tick_WhenClockNotRunning_DoesNotChangeState()
    {
        State = PostGameClockState.Default;

        await Tick(70000);

        State.Should().Be(PostGameClockState.Default);
    }

    [Test]
    public void State_ShouldSerializeCorrectly()
    {
        var state = new PostGameClockState(true, 50000, 80000, 30000);

        var serialized = System.Text.Json.JsonSerializer.Serialize(state, Program.JsonSerializerOptions);

        var deserialized = System.Text.Json.Nodes.JsonNode.Parse(serialized)!;

        deserialized["isRunning"]!.AsValue().GetValue<bool>().Should().BeTrue();
        deserialized["startTick"]!.AsValue().GetValue<int>().Should().Be(50000);
        deserialized["endTick"]!.AsValue().GetValue<int>().Should().Be(80000);
        deserialized["ticksPassed"]!.AsValue().GetValue<int>().Should().Be(30000);
        deserialized["secondsPassed"]!.AsValue().GetValue<int>().Should().Be(30);
    }

    [Test]
    public async Task OvertimeStarted_WhenClockRunning_StopsClock()
    {
        State = new PostGameClockState(true, 50000, 0, 0);

        await Subject.Handle(new OvertimeStarted(80000));

        State.IsRunning.Should().BeFalse();
    }

    [Test]
    public async Task OvertimeStarted_WhenClockNotRunning_DoesNotChangeState()
    {
        State = PostGameClockState.Default;

        var originalState = State;

        await Subject.Handle(new OvertimeStarted(80000));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task OvertimeEnded_StartsClockFromZero()
    {
        State = new(false, 1000, 3000, 2000);

        await Subject.Handle(new OvertimeEnded(80000));

        State.IsRunning.Should().BeTrue();
        State.StartTick.Should().Be(80000);
        State.EndTick.Should().Be(0);
        State.TicksPassed.Should().Be(0);
    }
}

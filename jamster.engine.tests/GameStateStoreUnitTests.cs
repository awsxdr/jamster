using FluentAssertions;
using Func;

using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests;

public class GameStateStoreUnitTests : UnitTest<GameStateStore>
{
    [Test]
    public void GetState_ReturnsPreviouslySetState()
    {
        var state = new TestState(Guid.NewGuid(), 1);
        Subject.LoadDefaultStates([new TestReducer()]);
        Subject.SetState(state);

        var returnedState = Subject.GetState<TestState>();

        returnedState.Should().Be(state);
    }

    [Test]
    public void SetState_NotifiesWatchingThreads()
    {
        Subject.LoadDefaultStates([new TestReducer()]);

        var hasBeenCalled = false;
        Subject.WatchState<TestState>("TestState", _ =>
        {
            hasBeenCalled = true;
            return Task.CompletedTask;
        });

        Subject.SetState(new TestState(Guid.NewGuid(), 1));

        hasBeenCalled.Should().BeTrue();
    }

    [Test]
    public void SetState_IgnoresChangesToAppropriatelyTaggedProperties()
    {
        Subject.LoadDefaultStates([new TestReducer()]);
        var initialState = Subject.GetState<TestState>();

        var callCount = 0;
        Subject.WatchState<TestState>("TestState", _ =>
        {
            ++callCount;
            return Task.CompletedTask;
        });

        Subject.SetState(initialState with { Ignored = 2 });
        Subject.SetState(initialState with { Ignored = 3 });

        callCount.Should().Be(0);
    }

    private record TestState(Guid Id, [property: IgnoreChange] int Ignored);

    private class TestReducer : IReducer<TestState>
    {
        public object GetDefaultState() => new TestState(Guid.NewGuid(), 1);
        public Option<string> GetStateKey() => Option.None<string>();
    }
}
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Func;

namespace amethyst.tests;

public class GameStateStoreUnitTests : UnitTest<GameStateStore>
{
    [Test]
    public void GetState_ReturnsPreviouslySetState()
    {
        var state = new TestState(Guid.NewGuid());
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
        Subject.WatchState<TestState>(state =>
        {
            hasBeenCalled = true;
            return Task.CompletedTask;
        });

        Subject.SetState(new TestState(Guid.NewGuid()));

        hasBeenCalled.Should().BeTrue();
    }

    private record TestState(Guid Id);

    private class TestReducer : IReducer<TestState>
    {
        public object GetDefaultState() => new TestState(Guid.NewGuid());
        public Option<string> GetStateKey() => Option.None<string>();
    }
}
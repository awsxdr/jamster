using FluentAssertions;
using Func;

using jamster.engine.Reducers;

using static jamster.engine.Extensions.ReducerSortExtensions;

namespace jamster.engine.tests.Extensions;

[TestFixture]
public class ReducerSortExtensionsUnitTests
{
    [Test]
    public void SortReducers_WithValidDependencyGraph_SortsReducersByDependency()
    {
        var reducers =
            new IReducer [] 
            {
                new TestReducer1(),
                new TestReducer2(),
                new TestReducer3(),
                new TestReducer4(),
            };

        var sortedReducers = reducers.SortReducers().ToArray();

        sortedReducers[0].Should().BeOfType<TestReducer1>();
        sortedReducers[1].Should().BeOfType<TestReducer3>();
        sortedReducers[2].Should().BeOfType<TestReducer2>();
        sortedReducers[3].Should().BeOfType<TestReducer4>();
    }

    [Test]
    public void SortReducers_WithCyclicalDependencyGraph_ThrowsCyclicalReducerDependenciesException()
    {
        var reducers =
            new IReducer[]
            {
                new TestReducer1(),
                new TestReducer2(),
                new TestReducer3(),
                new TestReducer4(),
                new TestReducer5(),
                new TestReducer6(),
            };

        Action sort = () => _ = reducers.SortReducers().ToArray();

        sort.Should().Throw<CyclicalReducerDependenciesException>();
    }

    private abstract class TestReducer<TState> : IReducer<TState>, IDependsOnState<TState>
        where TState : class, new()
    {
        public object GetDefaultState() => new TState();
        public Option<string> GetStateKey() => Option.None<string>();
    }

    private sealed class TestState1;
    private sealed class TestState2;
    private sealed class TestState3;
    private sealed class TestState4;
    private sealed class TestState5;
    private sealed class TestState6;

    private sealed class TestReducer1 : TestReducer<TestState1>;
    private sealed class TestReducer2 : TestReducer<TestState2>, IDependsOnState<TestState1>, IDependsOnState<TestState3>;
    private sealed class TestReducer3 : TestReducer<TestState3>, IDependsOnState<TestState1>;
    private sealed class TestReducer4 : TestReducer<TestState4>, IDependsOnState<TestState1>, IDependsOnState<TestState2>, IDependsOnState<TestState3>;
    private sealed class TestReducer5 : TestReducer<TestState5>, IDependsOnState<TestState1>, IDependsOnState<TestState6>;
    private sealed class TestReducer6 : TestReducer<TestState6>, IDependsOnState<TestState5>;
}
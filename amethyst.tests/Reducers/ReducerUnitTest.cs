using amethyst.Reducers;
using amethyst.Services;
using Moq;
using Result = Func.Result;

namespace amethyst.tests.Reducers;

public abstract class ReducerUnitTest<TReducer, TState> : UnitTest<TReducer> 
    where TReducer : class, IReducer<TState>
    where TState : class
{
    protected TState State { get; set; }

    protected override void Setup()
    {
        base.Setup();

        State = (TState)Subject.GetDefaultState();

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<TState>())
            .Returns(() => State);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<TState>()))
            .Callback((TState s) => State = s);
    }

    protected void MockState<TOtherState>(TOtherState state) where TOtherState : class
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<TOtherState>())
            .Returns(state);

        var stateName = state.GetType().Name;

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetStateByName(stateName))
            .Returns(Result.Succeed<object>(state));
    }
}
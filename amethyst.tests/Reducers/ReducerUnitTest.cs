using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
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

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetCachedState<TOtherState>())
            .Returns(state);

        var stateName = state.GetType().Name;

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetStateByName(stateName))
            .Returns(Result.Succeed<object>(state));
    }

    protected void VerifyEventSent<TEvent>(Tick tick) where TEvent : Event
    {
        GetMock<IEventBus>()
            .Verify(mock => mock.AddEvent(
                It.IsAny<GameInfo>(),
                It.Is<TEvent>(e => e.Tick == tick)
            ), Times.Once);
    }

    protected void VerifyEventSent<TEvent, TBody>(TEvent @event) where TEvent : Event<TBody>
    {
        GetMock<IEventBus>()
            .Verify(mock => mock.AddEvent(
                It.IsAny<GameInfo>(),
                It.Is<TEvent>(e => e.Tick == @event.Tick && e.Body!.Equals(@event.Body))
            ), Times.Once);
    }
}
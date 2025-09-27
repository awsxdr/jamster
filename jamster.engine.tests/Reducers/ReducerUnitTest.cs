﻿using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

using Moq;
using Result = Func.Result;

namespace jamster.engine.tests.Reducers;

public abstract class ReducerUnitTest<TReducer, TState> : UnitTest<TReducer, IReducer<TState>> 
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
            .Setup(mock => mock.GetKeyedState<TState>(It.IsAny<string>()))
            .Returns(() => State);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetState(It.IsAny<TState>()))
            .Callback((TState s) => State = s);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.SetKeyedState<TState>(It.IsAny<string>(), It.IsAny<TState>()))
            .Callback((string _, TState s) => State = s);

        GetMock<IEventBus>()
            .Setup(mock => mock.AddEvent(It.IsAny<GameInfo>(), It.IsAny<Event>()))
            .Returns(async (GameInfo _, Event @event) =>
            {
                await Subject.HandleUntyped(@event);
                return @event;
            });

        GetMock<IEventBus>()
            .Setup(mock => mock.AddEventWithoutPersisting(It.IsAny<GameInfo>(), It.IsAny<Event>(), It.IsAny<Guid7>()))
            .Returns(async (GameInfo _, Event @event, Guid7 rootEventId) =>
            {
                await Subject.HandleUntyped(@event, rootEventId);
                return @event;
            });
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

    protected void MockKeyedState<TOtherState>(string key, TOtherState state) where TOtherState : class
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetKeyedState<TOtherState>(key))
            .Returns(state);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetCachedKeyedState<TOtherState>(key))
            .Returns(state);
    }

    protected void VerifyEventSent<TEvent>(Tick tick) where TEvent : Event
    {
        GetMock<IEventBus>()
            .Verify(mock => mock.AddEventWithoutPersisting(
                It.IsAny<GameInfo>(),
                It.Is<TEvent>(e => e.Tick == tick),
                It.IsAny<Guid7>()
            ), Times.Once);
    }

    protected void VerifyEventSent<TEvent, TBody>(TEvent @event) where TEvent : Event<TBody>
    {
        GetMock<IEventBus>()
            .Verify(mock => mock.AddEventWithoutPersisting(
                It.IsAny<GameInfo>(),
                It.Is<TEvent>(e => e.Tick == @event.Tick && e.Body!.Equals(@event.Body)),
                It.IsAny<Guid7>()
            ), Times.Once);
    }

    protected Task<IEnumerable<Event>> Tick(Tick tick)
    {
        if (Subject is not ITickReceiverAsync tickReceiver)
            throw new SubjectNotTickReceiverException();

        return tickReceiver.TickAsync(tick);
    }

    public class SubjectNotTickReceiverException : Exception;
}
namespace amethyst.Reducers;

using Events;
using Services;

public delegate IReducer ReducerFactory(IGameStateStore stateStore);

public interface IReducer
{
    object GetDefaultState();

    public async Task Handle<TEvent>(TEvent @event) where TEvent : class
    {
        if (this is not IHandlesEventAsync<TEvent> handler)
            return;

        await handler.HandleAsync(@event);
    }

    public Task HandleUntyped(Event @event)
    {
        var handleTask = (Task) typeof(IReducer)
            .GetMethod(nameof(Handle))
            !.MakeGenericMethod(@event.GetType())
            .Invoke(this, [@event])!;

        return handleTask;
    }
}

public interface IReducer<out TState> : IReducer
    where TState : class
{
}

public interface IHandlesEvent<in TEvent> : IHandlesEventAsync<TEvent>
{
    void Handle(TEvent @event);

    Task IHandlesEventAsync<TEvent>.HandleAsync(TEvent @event)
    {
        Handle(@event);
        return Task.CompletedTask;
    }
}

public interface IHandlesEventAsync<in TEvent>
{
    Task HandleAsync(TEvent @event);
}

public abstract class Reducer<TState>(IGameStateStore stateStore) : IReducer<TState>
    where TState : class
{
    protected abstract TState DefaultState { get; }

    public object GetDefaultState() => DefaultState;

    protected TState GetState() => GetState<TState>();

    protected TOtherState GetState<TOtherState>() where TOtherState : class =>
        stateStore.GetState<TOtherState>();

    protected void SetState(TState state) =>
        stateStore.SetState(state);
}

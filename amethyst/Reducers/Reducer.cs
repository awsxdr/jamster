namespace amethyst.Reducers;

using Events;
using Services;

public delegate IReducer ReducerFactory(GameContext gameContext);

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

public interface IReducer<out TState> : IReducer where TState : class;

public interface IHandlesEvent<in TEvent> : IHandlesEventAsync<TEvent>
    where TEvent : Event
{
    void Handle(TEvent @event);

    Task IHandlesEventAsync<TEvent>.HandleAsync(TEvent @event)
    {
        if (this is ITickReceiver tickReceiver)
            tickReceiver.Tick(@event.Id.Tick);

        Handle(@event);
        return Task.CompletedTask;
    }
}

public interface IHandlesEventAsync<in TEvent>
{
    Task HandleAsync(TEvent @event);
}

public abstract class Reducer<TState>(GameContext context) : IReducer<TState>
    where TState : class
{
    protected GameContext Context { get; } = context;

    protected abstract TState DefaultState { get; }

    public object GetDefaultState() => DefaultState;

    protected TState GetState() => GetState<TState>();

    protected TOtherState GetState<TOtherState>() where TOtherState : class =>
        Context.StateStore.GetState<TOtherState>();

    protected TOtherState GetCachedState<TOtherState>() where TOtherState : class =>
        Context.StateStore.GetCachedState<TOtherState>();

    protected void SetState(TState state) =>
        Context.StateStore.SetState(state);

    protected bool SetStateIfDifferent(TState state)
    {
        if (state != GetState())
        {
            SetState(state);
            return true;
        }

        return false;
    }
}

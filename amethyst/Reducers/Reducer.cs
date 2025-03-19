using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public delegate IReducer ReducerFactory(ReducerGameContext gameContext);

public interface IReducer
{
    Type StateType { get; }

    object GetDefaultState();
    Option<string> GetStateKey();

    public async Task<IEnumerable<Event>> Handle<TEvent>(TEvent @event, Guid7? sourceEventId = null) where TEvent : Event
    {
        if (this is IHandlesAllEventsAsync allEventsHandler)
            return await allEventsHandler.HandleAsync(@event, sourceEventId);

        if (this is not IHandlesEventAsync<TEvent> handler)
            return [];

        return await handler.HandleAsync(@event);
    }

    public Task<IEnumerable<Event>> HandleUntyped(Event @event, Guid7? sourceEventId = null)
    {
        var handleTask = (Task<IEnumerable<Event>>) typeof(IReducer)
            .GetMethod(nameof(Handle))
            !.MakeGenericMethod(@event.GetType())
            .Invoke(this, [@event, sourceEventId])!;

        return handleTask;
    }
}

// ReSharper disable once UnusedTypeParameter - Type is used to mark interface for DI
public interface IReducer<out TState> : IReducer where TState : class
{
    Type IReducer.StateType => typeof(TState);
}

public interface IHandlesEvent<in TEvent> : IHandlesEventAsync<TEvent>
    where TEvent : Event
{
    IEnumerable<Event> Handle(TEvent @event);

    Task<IEnumerable<Event>> IHandlesEventAsync<TEvent>.HandleAsync(TEvent @event) => Handle(@event).ToTask();
}

public interface IHandlesEventAsync<in TEvent>
    where TEvent : Event
{
    Task<IEnumerable<Event>> HandleAsync(TEvent @event);
}

public interface IHandlesAllEvents : IHandlesAllEventsAsync
{
    IEnumerable<Event> Handle(Event @event, Guid7? sourceEventId);

    async Task<IEnumerable<Event>> IHandlesAllEventsAsync.HandleAsync(Event @event, Guid7? sourceEventId)
    {
        var implicitEvents = new List<Event>();

        if (this is ITickReceiverAsync tickReceiver)
            implicitEvents.AddRange(await tickReceiver.TickAsync(@event.Id.Tick));

        implicitEvents.AddRange(Handle(@event, sourceEventId));

        return implicitEvents;
    }
}

public interface IHandlesAllEventsAsync
{
    Task<IEnumerable<Event>> HandleAsync(Event @event, Guid7? sourceEventId);
}

public interface IDependsOnState;
// ReSharper disable once UnusedTypeParameter
public interface IDependsOnState<in TState> : IDependsOnState where TState : class;

public sealed class GettingStateWithoutDependencyException()
    : Exception(
        "Attempt to get state when reducer is not marked as dependent on that state. Mark reducer with IDependsOnState.");

public abstract class Reducer<TState>(ReducerGameContext context) : IReducer<TState>, IDependsOnState<TState>
    where TState : class
{
    protected ReducerGameContext Context { get; } = context;

    protected abstract TState DefaultState { get; }

    public object GetDefaultState() => DefaultState;
    public virtual Option<string> GetStateKey() => Option.None<string>();

    protected TState GetState() =>
        GetStateKey() switch
        {
            Some<string> s => GetKeyedState<TState>(s.Value),
            _ => GetState<TState>()
        };

    protected TOtherState GetState<TOtherState>() where TOtherState : class =>
        this is IDependsOnState<TOtherState>
            ? Context.StateStore.GetState<TOtherState>()
            : throw new GettingStateWithoutDependencyException();

    protected TOtherState GetKeyedState<TOtherState>(string key) where TOtherState : class =>
        this is IDependsOnState<TOtherState> or Reducer<TOtherState>
            ? Context.StateStore.GetKeyedState<TOtherState>(key)
            : throw new GettingStateWithoutDependencyException();

    protected TOtherState GetCachedState<TOtherState>() where TOtherState : class =>
        this is IDependsOnState<TOtherState>
            ? Context.StateStore.GetCachedState<TOtherState>()
            : throw new GettingStateWithoutDependencyException();

    protected TOtherState GetCachedKeyedState<TOtherState>(string key) where TOtherState : class =>
        this is IDependsOnState<TOtherState>
            ? Context.StateStore.GetCachedKeyedState<TOtherState>(key)
            : throw new GettingStateWithoutDependencyException();

    protected void SetState(TState state)
    {
        switch (GetStateKey())
        {
            case Some<string> s:
                Context.StateStore.SetKeyedState(s.Value, state);
                break;

            default:
                Context.StateStore.SetState(state);
                break;
        }
    }

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

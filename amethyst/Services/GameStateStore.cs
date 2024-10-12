using System.Collections.Immutable;
using System.Reflection;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using Func;

namespace amethyst.Services;

public delegate IGameStateStore GameStateStoreFactory();

public interface IGameStateStore
{
    TState GetState<TState>() where TState : class;
    TState GetKeyedState<TState>(string key) where TState : class;
    TState GetCachedState<TState>() where TState : class;
    TState GetCachedKeyedState<TState>(string key) where TState : class;
    void SetState<TState>(TState state) where TState : class;
    void SetKeyedState<TState>(string key, TState state) where TState : class;
    void LoadDefaultStates(IImmutableList<IReducer> reducers);
    Task ApplyEvents(IImmutableList<IReducer> reducers, params Event[] events);
    Result<object> GetStateByName(string stateName);
    void WatchState<TState>(Func<TState, Task> onStateUpdate) where TState : class;
    void WatchStateByName(string stateName, Func<object, Task> onStateUpdate);
}

public class GameStateStore(ILogger<GameStateStore> logger) : IGameStateStore
{
    private readonly Dictionary<string, object> _states = new();
    private readonly Dictionary<string, object> _cachedStates = new();
    private readonly Dictionary<string, IStateUpdatedEventSource> _stateEventStream = new();

    public TState GetState<TState>() where TState : class =>
        (TState)_states[GetStateName<TState>()];

    public TState GetKeyedState<TState>(string key) where TState : class =>
        (TState) _states[$"{GetStateName<TState>()}_{key}"];

    public TState GetCachedState<TState>() where TState : class =>
        (TState)_cachedStates[GetStateName<TState>()];

    public TState GetCachedKeyedState<TState>(string key) where TState : class =>
        (TState)_cachedStates[$"{GetStateName<TState>()}_{key}"];

    public Result<object> GetStateByName(string stateName) =>
        _states.TryGetValue(stateName, out var state)
            ? Result.Succeed(state)
            : Result<object>.Fail<StateNotFoundError>();

    public void SetState<TState>(TState state) where TState : class =>
        SetState(GetStateName<TState>(), state);

    public void SetKeyedState<TState>(string key, TState state) where TState : class =>
        SetState($"{GetStateName<TState>()}_{key}", state);

    public void LoadDefaultStates(IImmutableList<IReducer> reducers)
    {
        foreach (var reducer in reducers)
        {
            var state = reducer.GetDefaultState();
            var stateName = reducer.GetStateKey() switch
            {
                Some<string> s => $"{GetStateName(state.GetType())}_{s.Value}",
                _ => GetStateName(state.GetType())
            };
            _states[stateName] = state;
            _stateEventStream[stateName] = MakeEventSource(state.GetType());
        }
    }

    public void WatchStateByName(string stateName, Func<object, Task> onStateUpdate)
    {
        var stateType = _states[stateName].GetType();
        var updateHandler = 
            GetType().GetMethod(nameof(MapStateUpdateHandler), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(stateType)
                .Invoke(this, [onStateUpdate])!;

        GetType().GetMethod(nameof(WatchState))!
            .MakeGenericMethod(stateType)
            .Invoke(this, [updateHandler]);
    }

    public void WatchState<TState>(Func<TState, Task> onStateUpdate) where TState : class =>
        GetEventSource<TState>(GetStateName<TState>()).StateUpdated += (_, e) => onStateUpdate(e.State);

    public async Task ApplyEvents(IImmutableList<IReducer> reducers, params Event[] events)
    {
        CacheStates();

        try
        {
            var queuedEvents = new Queue<Event>(events.OrderBy(e => e.Id));

            while (queuedEvents.TryDequeue(out var @event))
            {
                var implicitEvents = await HandleEvent(reducers, @event);
                implicitEvents = implicitEvents.ToArray();

                if (implicitEvents.Any())
                    queuedEvents = new Queue<Event>(queuedEvents.Concat(implicitEvents).OrderBy(e => e.Id));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while applying events");

            throw;
        }
        finally
        {
            ClearCache();
        }
    }

    private static bool DetectChanges<TState>(TState initialState, TState newState) where TState : class
    {
        if (initialState == newState) return false;

        return initialState.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetCustomAttribute<IgnoreChangeAttribute>() is null)
            .Select(property => (Initial: property.GetValue(initialState), New: property.GetValue(newState)))
            .Any(states => !states.Initial!.Equals(states.New));
    }

    private static string GetStateName<TState>() => GetStateName(typeof(TState));
    private static string GetStateName(Type stateType) => stateType.Name;

    private static Func<TState, Task> MapStateUpdateHandler<TState>(Func<object, Task> onStateUpdate) where TState : class =>
        onStateUpdate;

    private void SetState<TState>(string stateName, TState state) where TState : class
    {
        var previousState = _states[stateName];
        _states[stateName] = state;

        var hasChanged = DetectChanges(previousState, state);
        if (hasChanged)
        {
            GetEventSource<TState>(stateName).Update(state);
        }
    }

    private void CacheStates()
    {
        lock (_cachedStates)
        {
            if (_cachedStates.Any()) return;

            foreach (var (key, value) in _states)
                _cachedStates[key] = value;
        }
    }

    private void ClearCache()
    {
        lock (_cachedStates)
        {
            _cachedStates.Clear();
        }
    }

    private StateUpdateEventSource<TState> GetEventSource<TState>(string stateName) =>
        (StateUpdateEventSource<TState>)_stateEventStream[stateName];

    private static IStateUpdatedEventSource MakeEventSource(Type stateType) =>
        (IStateUpdatedEventSource)typeof(StateUpdateEventSource<>).MakeGenericType(stateType).GetConstructor([])!.Invoke([]);

    private async Task<IEnumerable<Event>> HandleEvent(IImmutableList<IReducer> reducers, Event @event)
    {
        logger.BeginScope("Handling {event}", @event);

        var implicitEvents = new List<Event>();

        foreach (var reducer in reducers)
        {
            try
            {
                implicitEvents.AddRange(await reducer.HandleUntyped(@event));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while applying {eventType} with reducer {reducerType}. Game state may now be invalid.", @event.GetType().Name, reducer.GetType().Name);
            }
        }

        return implicitEvents;
    }

    private interface IStateUpdatedEventSource;
    private class StateUpdateEventSource<TState> : IStateUpdatedEventSource
    {
        public event EventHandler<StateUpdatedEventArgs>? StateUpdated;

        public class StateUpdatedEventArgs(TState state) : EventArgs
        {
            public TState State { get; } = state;
        }

        internal void Update(TState state)
        {
            StateUpdated?.Invoke(this, new StateUpdatedEventArgs(state));
        }
    }
}

public sealed class StateNotFoundError : NotFoundError;
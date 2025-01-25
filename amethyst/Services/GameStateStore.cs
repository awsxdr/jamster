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
    void WatchState<TState>(string stateName, Func<TState, Task> onStateUpdate) where TState : class;
    void WatchStateByName(string stateName, Func<object, Task> onStateUpdate);
    void EnableNotifications();
    void DisableNotifications();
    void ForceNotify();
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
            .Invoke(this, [stateName, updateHandler]);
    }

    public void WatchState<TState>(string stateName, Func<TState, Task> onStateUpdate) where TState : class =>
        GetEventSource<TState>(stateName).StateUpdated += (_, e) => onStateUpdate(e.State);

    private record EventDetails(Event Event, Guid7? SourceEventId);

    public async Task ApplyEvents(IImmutableList<IReducer> reducers, params Event[] events)
    {
        CacheStates();

        try
        {
            var queuedEvents = new Queue<EventDetails>(events.OrderBy(e => e.Id).Select(e => new EventDetails(e, null)));

            while (queuedEvents.TryDequeue(out var @event))
            {
                var tickImplicitEvents = (await Tick(reducers, @event.Event.Tick - 1)).ToArray();

                if (tickImplicitEvents.Any())
                {
                    queuedEvents = new Queue<EventDetails>(
                        queuedEvents
                            .Concat(tickImplicitEvents.Select(e => new EventDetails(e, null)))
                            .Append(@event)
                            .OrderBy(e => e.Event.Id));

                    continue;
                }

                var implicitEvents = await HandleEvent(reducers, @event.Event, @event.SourceEventId);
                implicitEvents = implicitEvents.ToArray();

                if (implicitEvents.Any())
                    // ReSharper disable once AccessToModifiedClosure
                    queuedEvents = new Queue<EventDetails>(queuedEvents.Concat(implicitEvents.Select(e => new EventDetails(e, @event.Event.Id))).OrderBy(e => e.Event.Id));
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

    public void EnableNotifications() => _stateEventStream.Values.ForEach(s => s.NotifyOnStateChange = true).Evaluate();
    public void DisableNotifications() => _stateEventStream.Values.ForEach(s => s.NotifyOnStateChange = false).Evaluate();

    public void ForceNotify()
    {
        foreach (var (key, stream) in _stateEventStream)
        {
            var state = _states[key];
            stream.InvokeStateUpdated(state);
        }
    }

    private static bool DetectChanges<TState>(TState initialState, TState newState) where TState : class
    {
        if (initialState == newState) return false;

        return initialState.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetCustomAttribute<IgnoreChangeAttribute>() is null)
            .Select(property => (Initial: property.GetValue(initialState), New: property.GetValue(newState)))
            .Any(states => !((states.Initial is null && states.New is null) || (states.Initial?.Equals(states.New) ?? false) || (states is { Initial: Array i, New: Array n } && i.Cast<object>().SequenceEqual(n.Cast<object>()))));
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

    private StateUpdateEventSource<TState> GetEventSource<TState>(string stateName)
    {
        if (!_stateEventStream.ContainsKey(stateName))
            _stateEventStream[stateName] = MakeEventSource(typeof(TState));

        return (StateUpdateEventSource<TState>)_stateEventStream[stateName];
    }

    private static IStateUpdatedEventSource MakeEventSource(Type stateType) =>
        (IStateUpdatedEventSource)typeof(StateUpdateEventSource<>).MakeGenericType(stateType).GetConstructor([])!.Invoke([]);

    private async Task<IEnumerable<Event>> Tick(IImmutableList<IReducer> reducers, Tick tick)
    {
        var implicitEvents = new List<Event>();

        foreach (var reducer in reducers.OfType<ITickReceiver>())
        {
            implicitEvents.AddRange(await reducer.TickAsync(tick));
        }

        return implicitEvents;
    }

    private async Task<IEnumerable<Event>> HandleEvent(IImmutableList<IReducer> reducers, Event @event, Guid7? sourceEventId)
    {
        logger.BeginScope("Handling {event}", @event);

        var implicitEvents = new List<Event>();
        var periodClock = GetState<PeriodClockState>();

        foreach (var reducer in reducers)
        {
            try
            {
                var newImplicitEvents = (await reducer.HandleUntyped(@event, sourceEventId)).ToArray();

                if (periodClock.IsRunning)
                {
                    foreach (var implicitEvent in newImplicitEvents.Where(e => e is IPeriodClockAligned))
                    {
                        implicitEvent.Id = Guid7.FromTick(GetAlignedTick(implicitEvent));
                    }
                }

                implicitEvents.AddRange(newImplicitEvents);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while applying {eventType} with reducer {reducerType}. Game state may now be invalid.", @event.GetType().Name, reducer.GetType().Name);
            }
        }

        return implicitEvents;

        Tick GetAlignedTick(Event alignEvent) =>
            (long)(Math.Round((alignEvent.Tick - periodClock.LastStartTick) / 1000.0) * 1000) + periodClock.LastStartTick;
    }

    private interface IStateUpdatedEventSource
    {
        bool NotifyOnStateChange { get; set; }

        void InvokeStateUpdated(object state);
    }

    private class StateUpdateEventSource<TState> : IStateUpdatedEventSource
    {
        public event EventHandler<StateUpdatedEventArgs>? StateUpdated;

        public bool NotifyOnStateChange { get; set; } = true;

        public class StateUpdatedEventArgs(TState state) : EventArgs
        {
            public TState State { get; } = state;
        }

        public void InvokeStateUpdated(object state)
        {
            if (state is not TState typedState) return;

            StateUpdated?.Invoke(this, new StateUpdatedEventArgs(typedState));
        }

        internal void Update(TState state)
        {
            if (!NotifyOnStateChange) return;

            StateUpdated?.Invoke(this, new StateUpdatedEventArgs(state));
        }
    }
}

public sealed class StateNotFoundError : NotFoundError;
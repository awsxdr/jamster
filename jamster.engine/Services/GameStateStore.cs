﻿using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.Services;

public delegate IGameStateStore GameStateStoreFactory();

public sealed class EventHandledEventArgs : EventArgs
{
    public required Tick Tick { get; init; }
    public required int Index { get; init; }
}

public interface IGameStateStore
{
    event EventHandler<EventHandledEventArgs> EventHandled;

    object GetState(Type stateType);
    TState GetState<TState>() where TState : class;
    object GetKeyedState(string key, Type stateType);
    TState GetKeyedState<TState>(string key) where TState : class;
    ReadOnlyDictionary<string, object> GetAllStates();
    TState GetCachedState<TState>() where TState : class;
    TState GetCachedKeyedState<TState>(string key) where TState : class;
    void SetState<TState>(TState state) where TState : class;
    void SetKeyedState<TState>(string key, TState state) where TState : class;
    void LoadDefaultStates(IImmutableList<IReducer> reducers);
    void ApplyKeyFrame(IImmutableList<IReducer> reducers, KeyFrame keyFrame);
    Task<IEnumerable<Event>> ApplyEvents(IImmutableList<IReducer> reducers, Guid7? rootSourceEventId, params Event[] events);
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

    private int _eventCount;

    public event EventHandler<EventHandledEventArgs>? EventHandled;

    public object GetState(Type stateType) =>
        _states[GetStateName(stateType)];

    public TState GetState<TState>() where TState : class =>
        (TState)_states[GetStateName<TState>()];

    public object GetKeyedState(string key, Type stateType) =>
        _states[$"{GetStateName(stateType)}_{key}"];

    public TState GetKeyedState<TState>(string key) where TState : class =>
        (TState) _states[$"{GetStateName<TState>()}_{key}"];

    public ReadOnlyDictionary<string, object> GetAllStates() =>
        _states.AsReadOnly();

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

    public async Task<IEnumerable<Event>> ApplyEvents(IImmutableList<IReducer> reducers, Guid7? rootSourceEventId, params Event[] events)
    {
        CacheStates();

        try
        {
            var eventsToPersist = new List<Event>();
            var queuedEvents = new Queue<EventDetails>(events.OrderBy(e => e.Id).Select(e => new EventDetails(e, rootSourceEventId)));

            while (queuedEvents.TryDequeue(out var @event))
            {
                using var _ = logger.BeginScope(new Dictionary<string, object> { ["eventType"] = @event.GetType().Name, ["tick"] = @event.Event.Tick });

                var tickImplicitEvents = (await Tick(reducers, @event.Event.Tick - 1)).ToArray();

                eventsToPersist.AddRange(tickImplicitEvents.Where(e => e is IAlwaysPersisted));

                if (tickImplicitEvents.Length > 0)
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
                    queuedEvents = new Queue<EventDetails>(queuedEvents.Concat(
                        implicitEvents
                            .Select(e => new EventDetails(e, GetSourceEventId(e, @event))))
                        .OrderBy(e => e.Event.Id));
            }

            return eventsToPersist;
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

        Guid7? GetSourceEventId(Event @event, EventDetails sourceEventDetails) =>
            @event is IAlwaysPersisted ? null
                //: sourceEventDetails.SourceEventId != null && sourceEventDetails.SourceEventId == GameClock.TickEventId ? null
                : sourceEventDetails.SourceEventId
                ?? rootSourceEventId
                ?? sourceEventDetails.Event.Id;
    }

    public void ApplyKeyFrame(IImmutableList<IReducer> reducers, KeyFrame keyFrame)
    {
        LoadDefaultStates(reducers);

        foreach (var key in keyFrame.Keys)
        {
            _states[key] = keyFrame[key];
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

    private static async Task<IEnumerable<Event>> Tick(IImmutableList<IReducer> reducers, Tick tick)
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

        EventHandled?.Invoke(this, new() { Tick = @event.Tick, Index = ++_eventCount });

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
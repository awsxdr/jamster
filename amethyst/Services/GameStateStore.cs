namespace amethyst.Services;

using System.Collections.Immutable;using amethyst.DataStores;
using amethyst.Events;
using Microsoft.Extensions.Logging;
using Reducers;

public interface IGameStateStore
{
    TState GetState<TState>() where TState : class;
    void SetState<TState>(TState state) where TState : class;
    void LoadDefaultStates(IImmutableList<IReducer> reducers);
    Task ApplyEvents(IImmutableList<IReducer> reducers, params Event[] events);
}

public class GameStateStore(ILogger<GameStateStore> logger) : IGameStateStore
{
    private readonly Dictionary<Type, object> _states = new();

    public event EventHandler<StateUpdatedEventArgs>? StateUpdated;

    public TState GetState<TState>() where TState : class =>
        (TState)_states[typeof(TState)];

    public void SetState<TState>(TState state) where TState : class
    {
        _states[typeof(TState)] = state;
        StateUpdated?.Invoke(this, new(state));
    }

    public void LoadDefaultStates(IImmutableList<IReducer> reducers)
    {
        foreach (var reducer in reducers)
        {
            var state = reducer.GetDefaultState();
            _states[state.GetType()] = state;
        }
    }

    public async Task ApplyEvents(IImmutableList<IReducer> reducers, params Event[] events)
    {
        foreach (var @event in events)
        {
            await HandleEvent(reducers, @event);
        }
    }

    private async Task HandleEvent(IImmutableList<IReducer> reducers, Event @event)
    {
        foreach (var reducer in reducers)
        {
            try
            {
                await reducer.HandleUntyped(@event);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while applying {eventType} with reducer {reducerType}. Game state may now be invalid.", @event.GetType().Name, reducer.GetType().Name);
            }
        }
    }

    public class StateUpdatedEventArgs(object state) : EventArgs
    {
        public string StateName { get; } = state.GetType().Name;
        public Type StateType { get; } = state.GetType();
        public object State { get; } = state;
    }
}
namespace amethyst.Services;

using System.Collections.Immutable;
using Events;
using Microsoft.Extensions.Logging;
using Reducers;

public interface IGameStateStore
{
    TState GetState<TState>() where TState : class;
    void SetState<TState>(TState state) where TState : class;
    Task ApplyEvents(params Event[] events);
}

public class GameStateStore : IGameStateStore
{
    private readonly IImmutableList<IReducer> _reducers;
    private readonly ILogger<GameStateStore> _logger;
    private readonly IDictionary<Type, object> _states;

    public event EventHandler<StateUpdatedEventArgs>? StateUpdated;

    public GameStateStore(
        IImmutableList<IReducer> reducers,
        ILogger<GameStateStore> logger)
    {
        _reducers = reducers
            .Where(r => r.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReducer<>)))
            .ToImmutableList();
        _logger = logger;

        _states = _reducers.ToDictionary(
            k => k.GetType().GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReducer<>))
                .GetGenericArguments()
                .Single(), 
            v => v.GetDefaultState());
    }

    public TState GetState<TState>() where TState : class =>
        (TState)_states[typeof(TState)];

    public void SetState<TState>(TState state) where TState : class
    {
        _states[typeof(TState)] = state;
        StateUpdated?.Invoke(this, new(state));
    }

    public async Task ApplyEvents(params Event[] events)
    {
        foreach (var @event in events)
        {
            await HandleEvent(@event);
        }
    }

    private async Task HandleEvent(Event @event)
    {
        foreach (var reducer in _reducers)
        {
            try
            {
                await reducer.Handle(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while applying {eventType} with reducer {reducerType}. Game state may now be invalid.", @event.GetType().Name, reducer.GetType().Name);
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
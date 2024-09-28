namespace amethyst.Services;

using DataStores;
using Events;
using Func;

public interface IEventBus
{
    Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event);
    Task<Event> AddEvent(GameInfo game, Event @event);
}

public class EventBus(
    IGameContextFactory contextFactory,
    GameStoreFactory gameStoreFactory,
    ILogger<EventBus> logger) : IEventBus
{
    public Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event)
    {
        @event.Id = Guid7.FromTick(IGameClock.GetTick());
        return AddEvent(game, @event);
    }

    public async Task<Event> AddEvent(GameInfo game, Event @event)
    {
        var stateStore = contextFactory.GetGame(game);

        if (PersistEventToDatabase(game, @event) is not Success<Guid> persistResult)
            return @event;

        @event.Id = persistResult.Value;

        await stateStore.StateStore.ApplyEvents(stateStore.Reducers, @event);

        return @event;
    }

    private Result<Guid> PersistEventToDatabase(GameInfo game, Event @event)
    {
        var databaseName = IGameDiscoveryService.GetGameFileName(game);

        try
        {
            var dataStore = gameStoreFactory(databaseName);
            var eventId = dataStore.AddEvent(@event);
            return Result.Succeed(eventId);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unable to write event {eventType} to game database {databaseName}. Event has been dropped.", @event.GetType().Name, databaseName);
            return Result<Guid>.Fail<EventPersistenceFailedError>();
        }
    }

    private class EventPersistenceFailedError : ResultError;
}
namespace amethyst.Services;

using DataStores;
using Events;
using Func;
using Reducers;

public class EventBus(
    IGameStateStoreFactory stateStoreFactory,
    GameStoreFactory gameStoreFactory,
    ILogger<EventBus> logger)
{
    public Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event)
    {
        //TODO: Set current tick
        return AddEvent(game, @event);
    }

    public async Task<Event> AddEvent(GameInfo game, Event @event)
    {
        var stateStore = stateStoreFactory.GetGame(game);

        if (PersistEventToDatabase(game, @event) is not Success<Guid> persistResult)
            return @event;

        //TODO: Store ID

        await stateStore.ApplyEvents(@event);

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
using System.Collections.Concurrent;
using amethyst.Domain;
using amethyst.Reducers;
using DotNext.Threading;

namespace amethyst.Services;

using DataStores;
using Events;
using Func;

public interface IEventBus
{
    Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event);
    Task<Event> AddEvent(GameInfo game, Event @event);
    Task AddEventWithoutPersisting(GameInfo game, Event @event);
    Task<Result<Event>> MoveEvent(GameInfo game, Event @event, Tick newTick);
    Task<Result> RemoveEvent(GameInfo game, Guid eventId);
}

[Singleton]
public class EventBus(
    IGameContextFactory contextFactory,
    IGameDataStoreFactory gameStoreFactory,
    ISystemTime systemTime,
    ILogger<EventBus> logger) 
    : IEventBus
{
    private readonly ConcurrentDictionary<Guid, Lazy<AsyncManualResetEvent>> _gameLocks = new();

    public Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event)
    {
        @event.Id = Guid7.FromTick(systemTime.GetTick());
        return AddEvent(game, @event);
    }

    public async Task<Event> AddEvent(GameInfo game, Event @event)
    {
        using var @lock = await AcquireLock(game.Id);

        logger.LogDebug("Adding event {event} for game {gameId}", @event, game.Id);

        var gameContext = contextFactory.GetGame(game);

        if (@event is IPeriodClockAligned)
        {
            logger.LogDebug("Aligning event to period clock");

            var periodClock = gameContext.StateStore.GetState<PeriodClockState>();
            if (periodClock.IsRunning)
            {
                Tick alignedTick = (long)(Math.Round((@event.Tick - periodClock.LastStartTick) / 1000.0) * 1000) + periodClock.LastStartTick;
                @event.Id = Guid7.FromTick(alignedTick);
            }
        }

        if (await PersistEventToDatabase(game, @event) is not Success<Guid> persistResult)
            return @event;

        @event.Id = persistResult.Value;

        logger.LogDebug("Applying event {eventId} ({eventType}) to game {gameId}", @event.Id, @event.GetType().Name, game.Id);
        var additionalEventsToPersist = await gameContext.StateStore.ApplyEvents(gameContext.Reducers, @event);

        foreach (var additionalEvent in additionalEventsToPersist)
        {
            var result = await PersistEventToDatabase(game, additionalEvent);

            if(result is Failure failure)
                logger.LogError("Error persisting additional event to database: {error}", failure.GetError());
        }

        return @event;
    }

    public async Task AddEventWithoutPersisting(GameInfo game, Event @event)
    {
        using var @lock = await AcquireLock(game.Id);

        var gameContext = contextFactory.GetGame(game);

        await gameContext.StateStore.ApplyEvents(gameContext.Reducers, @event);
    }

    public async Task<Result<Event>> MoveEvent(GameInfo game, Event @event, Tick newTick)
    {
        using var @lock = await AcquireLock(game.Id);

        return await
            RemoveEventFromDatabase(game, @event.Id)
                .OnSuccess(() => @event.Id = Guid7.FromTick(newTick))
                .Then(() => PersistEventToDatabase(game, @event))
                .Then(() =>
                {
                    contextFactory.ReloadGame(game);
                    return Result.Succeed(@event);
                });
    }

    public async Task<Result> RemoveEvent(GameInfo game, Guid eventId)
    {
        using var @lock = await AcquireLock(game.Id);

        return await RemoveEventFromDatabase(game, eventId)
            .Then(() =>
            {
                contextFactory.ReloadGame(game);
                return Result.Succeed();
            });
    }

    private async Task<AsyncLock.Holder> AcquireLock(Guid gameId)
    {
        logger.LogTrace("Acquiring game event lock for {gameId}", gameId);
        var eventLock = _gameLocks.GetOrAdd(gameId, _ => new(() => new AsyncManualResetEvent(false))).Value;
        return await eventLock.AcquireLockAsync();
    }

    private async Task<Result<Guid>> PersistEventToDatabase(GameInfo game, Event @event)
    {
        var databaseName = IGameDiscoveryService.GetGameFileName(game);

        try
        {
            var dataStore = await gameStoreFactory.GetDataStore(databaseName);
            var eventId = dataStore.AddEvent(@event);
            return Result.Succeed(eventId);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unable to write event {eventType} to game database {databaseName}. Event has been dropped.", @event.GetType().Name, databaseName);
            return Result<Guid>.Fail<EventPersistenceFailedError>();
        }
    }

    private async Task<Result> RemoveEventFromDatabase(GameInfo game, Guid eventId)
    {
        var databaseName = IGameDiscoveryService.GetGameFileName(game);

        try
        {
            var dataStore = await gameStoreFactory.GetDataStore(databaseName);
            return dataStore.GetEvent(eventId).Then(@event =>
            {
                if (@event is IReplaceOnDelete replace)
                {
                    var newEvent = replace.GetDeletionReplacement();
                    dataStore.AddEvent(newEvent);
                }

                dataStore.DeleteEvent(eventId);
                return Result.Succeed();
            });
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unable to delete event {eventId} from game database {databaseName}.", eventId, databaseName);
            return Result.Fail<EventDeletionFailedError>();
        }
    }

    private class EventPersistenceFailedError : ResultError;

    public class EventDeletionFailedError : ResultError;
}
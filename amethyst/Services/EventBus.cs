using System.Collections.Concurrent;
using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using DotNext.Threading;

namespace amethyst.Services;

public interface IEventBus
{
    Task<Event> AddEventAtCurrentTick(GameInfo game, Event @event);
    Task<Event> AddEvent(GameInfo game, Event @event);
    Task AddEventWithoutPersisting(GameInfo game, Event @event);
    Task<Result<Event>> MoveEvent(GameInfo game, Event @event, Tick newTick);
    Task<Result<Event>> ReplaceEvent(GameInfo game, Guid7 eventId, Event newEvent);
    Task<Result> RemoveEvent(GameInfo game, Guid7 eventId);
    Task<Result> RemoveEventsStartingAt(GameInfo game, Guid7 startEventId);
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

        logger.LogTrace("Adding event {event} for game {gameId}", @event, game.Id);

        var gameContext = contextFactory.GetGame(game);

        if (@event is IPeriodClockAligned)
        {
            logger.LogTrace("Aligning event to period clock");

            var periodClock = gameContext.StateStore.GetState<PeriodClockState>();
            if (periodClock.IsRunning)
            {
                Tick alignedTick = (long)(Math.Round((@event.Tick - periodClock.LastStartTick) / (float) Tick.TicksPerSecond) * Tick.TicksPerSecond) + periodClock.LastStartTick;
                @event.Id = Guid7.FromTick(alignedTick);
            }
        }

        if (await PersistEventToDatabase(game, @event) is not Success<Guid> persistResult)
            return @event;

        @event.Id = persistResult.Value;

        logger.LogTrace("Applying event {eventId} ({eventType}) to game {gameId} at tick {tick}", @event.Id, @event.GetType().Name, game.Id, @event.Tick);

        using var _ = logger.BeginScope("Event {event} at {tick}", @event.GetType().Name, @event.Tick);

        var additionalEventsToPersist = await gameContext.StateStore.ApplyEvents(gameContext.Reducers, @event);

        foreach (var additionalEvent in additionalEventsToPersist)
        {
            var result = await PersistEventToDatabase(game, additionalEvent);

            if (result is Failure failure)
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
                .Then(() => IntegrateChangeAtTick(game, @event.Tick))
                .Then(() => Result.Succeed(@event));
    }

    public async Task<Result<Event>> ReplaceEvent(GameInfo game, Guid7 eventId, Event newEvent)
    {
        using var @lock = await AcquireLock(game.Id);

        return await
            RemoveEventFromDatabase(game, eventId)
                .OnSuccess(@event => newEvent.Id = Guid7.FromTick(@event.Tick))
                .Then(() => PersistEventToDatabase(game, newEvent))
                .Then(() => IntegrateChangeAtTick(game, newEvent.Tick))
                .Then(() => Result.Succeed(newEvent));
    }

    public async Task<Result> RemoveEvent(GameInfo game, Guid7 eventId)
    {
        using var @lock = await AcquireLock(game.Id);

        return await RemoveEventFromDatabase(game, eventId)
            .Then(() => IntegrateChangeAtTick(game, eventId.Tick));
    }

    public async Task<Result> RemoveEventsStartingAt(GameInfo game, Guid7 startEventId)
    {
        using var @lock = await AcquireLock(game.Id);

        return await RemoveEventsFromDatabaseStartingAt(game, startEventId)
            .Then(async () =>
            {
                var gameContext = contextFactory.GetGame(game);
                var nearestKeyFrameMaybe = gameContext.KeyFrameService.GetKeyFrameBefore(startEventId.Tick);

                if (nearestKeyFrameMaybe is Some<KeyFrame> nearestKeyFrame)
                    await contextFactory.ApplyKeyFrame(game, nearestKeyFrame.Value);
                else
                    await contextFactory.ReloadGame(game);

                return Result.Succeed();
            });
    }

    private async Task<AsyncLock.Holder> AcquireLock(Guid gameId)
    {
        logger.LogTrace("Acquiring game event lock for {gameId}", gameId);
        var eventLock = _gameLocks.GetOrAdd(gameId, _ => new(() => new AsyncManualResetEvent(false))).Value;
        return await eventLock.AcquireLockAsync();
    }

    private Task<Result<Guid>> PersistEventToDatabase(GameInfo game, Event @event) =>
        WithDataStore(game,
            dataStore =>
            {
                var eventId = dataStore.AddEvent(@event);
                return Result.Succeed(eventId).ToTask();
            },
            ex =>
            {
                logger.LogCritical(ex, "Unable to write event {eventType} to game database for game {gameId}. Event has been dropped.", @event.GetType().Name, game.Id);
                return Result<Guid>.Fail<EventPersistenceFailedError>();
            });

    private Task<Result<Event>> RemoveEventFromDatabase(GameInfo game, Guid eventId) =>
        WithDataStore(game,
            dataStore =>
                dataStore.GetEvent(eventId).Then(@event =>
                {
                    if (@event is IReplaceOnDelete replace)
                    {
                        var newEvent = replace.GetDeletionReplacement();
                        dataStore.AddEvent(newEvent);
                    }

                    dataStore.DeleteEvent(eventId);
                    return Result.Succeed(@event);
                }).ToTask(),
            ex =>
            {
                logger.LogCritical(ex, "Unable to delete event {eventId} from game database for game {gameId}.", eventId, game.Id);
                return Result<Event>.Fail<EventDeletionFailedError>();
            });

    private Task<Result> RemoveEventsFromDatabaseStartingAt(GameInfo game, Guid7 startEventId) =>
        WithDataStore(game,
            dataStore =>
            {
                var eventsAfter = dataStore.GetEvents().Where(e => e.Id.Tick > startEventId.Tick);
                foreach(var @event in eventsAfter)
                    dataStore.DeleteEvent(@event.Id);

                return dataStore.GetEvent(startEventId).Then(@event =>
                {
                    if (@event is IReplaceOnDelete replace)
                    {
                        logger.LogDebug("Replacing event being deleted");
                        var newEvent = replace.GetDeletionReplacement();
                        dataStore.AddEvent(newEvent);
                    }

                    dataStore.DeleteEvent(startEventId);
                    return Result.Succeed();
                }).ToTask();
            },
            ex =>
            {
                logger.LogCritical(ex, "Unable to delete events starting at {eventId} from game database for game {gameId}.", startEventId, game.Id);
                return Result.Fail<EventDeletionFailedError>();
            });

    private async Task<TResult> WithDataStore<TResult>(GameInfo game, Func<IGameDataStore, Task<TResult>> action, Func<Exception, TResult> onFailure)
    {
        var databaseName = IGameDiscoveryService.GetGameFileName(game);

        try
        {
            var dataStore = await gameStoreFactory.GetDataStore(databaseName);

            return await action(dataStore);
        }
        catch (Exception ex)
        {
            return onFailure(ex);
        }
    }

    private async Task<Result> IntegrateChangeAtTick(GameInfo game, Tick tick)
    {
        var gameContext = contextFactory.GetGame(game);
        var nearestKeyFrameMaybe = gameContext.KeyFrameService.GetKeyFrameBefore(tick);

        if (nearestKeyFrameMaybe is Some<KeyFrame> nearestKeyFrame)
        {
            logger.LogDebug("Integrating change using key frame at tick {tick}", nearestKeyFrame.Value.Tick);
            await contextFactory.ApplyKeyFrame(game, nearestKeyFrame.Value);
        }
        else
        {
            logger.LogDebug("Integrating change by reloading game");
            await contextFactory.ReloadGame(game);
        }

        return Result.Succeed();
    }

    private class EventPersistenceFailedError : ResultError;

    public class EventDeletionFailedError : ResultError;
}
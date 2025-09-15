using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/games/{gameId:guid}/events")]
public class EventsController(
    IGameDiscoveryService gameDiscoveryService,
    IGameDataStoreFactory gameDataStoreFactory,
    IEventConverter eventConverter,
    IEventBus eventBus,
    ILogger<EventsController> logger
    ) : Controller
{
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<EventModel>>> GetEvents(Guid gameId, [FromQuery] int skip = 0, [FromQuery] int? maxCount = null, [FromQuery] SortOrder sortOrder = SortOrder.Desc)
    {
        logger.LogDebug("Getting events for game {gameId}", gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
                .Then(async game =>
                    (await gameDataStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(game)))
                        .GetEvents()
                        .Select(e => (EventModel)e)
                        .Map(e => sortOrder == SortOrder.Asc ? e.OrderBy(x => x.Id) : e.OrderByDescending(x => x.Id))
                        .Skip(skip)
                        .Map(e => maxCount is null ? e : e.Take((int)maxCount))
                        .Map(Result.Succeed))
            switch
            {
                Success<IEnumerable<EventModel>> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpGet("undo")]
    public async Task<ActionResult<IEnumerable<EventModel>>> GetUndoEvents(Guid gameId, [FromQuery] int? maxCount = null, [FromQuery] SortOrder sortOrder = SortOrder.Desc)
    {
        logger.LogDebug("Getting undo events for game {gameId}", gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
            .Then(async game =>
                (await gameDataStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(game)))
                .GetEvents()
                .Where(e => e is IShownInUndo)
                .Select(e => (EventModel)e)
                .Map(e => sortOrder == SortOrder.Asc ? e.OrderBy(x => x.Id) : e.OrderByDescending(x => x.Id))
                .Map(e => maxCount is null ? e : e.Take((int)maxCount))
                .Map(Result.Succeed))
            switch
            {
                Success<IEnumerable<EventModel>> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpPost("")]
    public async Task<ActionResult<EventModel>> AddEvent(Guid gameId, [FromBody] CreateEventModel model)
    {
        logger.LogDebug("Adding event {eventType} to game {gameId}", model.Type, gameId);

        return
            (await eventConverter.DecodeEvent(model.AsUntypedEvent())
                    .And(await gameDiscoveryService.GetExistingGame(gameId))
                    .ThenMap(x => eventBus.AddEventAtCurrentTick(x.Item2, x.Item1)))
                switch
                {
                    Success<Event> s => Accepted((EventModel)s.Value),
                    Failure<EventTypeNotKnownError> => BadRequest(),
                    Failure<BodyFormatIncorrectError> => BadRequest(),
                    Failure<GameFileNotFoundForIdError> => NotFound(),
                    Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                    var r => throw new UnexpectedResultException(r)
                };
    }

    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<EventModel>> GetEvent(Guid gameId, Guid eventId)
    {
        logger.LogDebug("Getting event {eventId} for game {gameId}", eventId, gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
            .Then(async game =>
                (await gameDataStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(game)))
                .GetEvent(eventId)
                .ThenMap(e => (EventModel)e))
            switch
            {
                Success<EventModel> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                Failure<GameDataStore.EventNotFoundError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid gameId, Guid eventId, [FromQuery] bool deleteFollowing)
    {
        if(deleteFollowing)
            logger.LogDebug("Deleting event {eventId} and all subsequent events from game {gameId}", eventId, gameId);
        else
            logger.LogDebug("Deleting event {eventId} from game {gameId}", eventId, gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
                .Then(game => deleteFollowing ? eventBus.RemoveEventsStartingAt(game, eventId) : eventBus.RemoveEvent(game, eventId))
            switch
            {
                Success => NoContent(),
                Failure<GameDataStore.EventNotFoundError> => NotFound(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                Failure<EventBus.EventDeletionFailedError> => StatusCode(500),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpPut("{eventId:guid}")]
    public async Task<IActionResult> ReplaceEvent(Guid gameId, Guid eventId, [FromBody] CreateEventModel model)
    {
        logger.LogDebug("Replacing event {eventId} in game {gameId} with {eventType}", eventId, gameId, model.Type);

        return await
                eventConverter.DecodeEvent(model.AsUntypedEvent())
                    .And(await gameDiscoveryService.GetExistingGame(gameId))
                    .Then(x => eventBus.ReplaceEvent(x.Item2, eventId, x.Item1)
                    )
            switch
            {
                Success<Event> s => Ok((EventModel)s.Value),
                Failure<GameDataStore.EventNotFoundError> => NotFound(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpPut("{eventId:guid}/tick")]
    public async Task<ActionResult<EventTickSetModel>> SetEventTick(Guid gameId, Guid eventId, [FromBody] SetEventTickModel model)
    {
        logger.LogDebug("Setting tick for event {eventId} in game {gameId} to {tick}", eventId, gameId, model.Tick);

        return await gameDiscoveryService.GetExistingGame(gameId)
                .Then(async game =>
                    await (await gameDataStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(game)))
                    .GetEvent(eventId)
                    .Then(async @event => 
                        model.OffsetFollowing
                        ? await eventBus.OffsetEventsAfter(game, @event, (int)(model.Tick - @event.Tick))
                        : await eventBus.MoveEvent(game, @event, model.Tick).ThenMap(e => new EventBus.OffsetEventsResult(e, []))
                    )
                )
            switch
            {
                Success<EventBus.OffsetEventsResult> s => Ok(new EventTickSetModel((EventModel) s.Value.NewEvent, s.Value.OtherModifiedEvents.Select(e => (EventModel)e).ToArray())),
                Failure<GameDataStore.EventNotFoundError> => NotFound(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                var r => throw new UnexpectedResultException(r)
            };
    }

    public record EventModel(string Type, Guid Id, object? Body)
    {
        public IUntypedEvent AsUntypedEvent() =>
            Body is null
                ? new UntypedEvent(Type, Id)
                : new UntypedEventWithBody(Type, Id, JsonSerializer.SerializeToNode(Body)!.AsObject());

        public static explicit operator EventModel(Event @event) => new(@event.GetType().Name, @event.Id, @event.GetBodyObject());
    }

    public record CreateEventModel(string Type, JsonObject? Body)
    {
        public IUntypedEvent AsUntypedEvent() =>
            Body is null
                ? new UntypedEvent(Type)
                : new UntypedEventWithBody(Type, Body);
    }

    public record SetEventTickModel(long Tick, bool OffsetFollowing = false);

    public record EventTickSetModel(EventModel NewEvent, EventModel[] OtherChangedEvents);

    public enum SortOrder
    {
        Asc,
        Desc,
    }
}
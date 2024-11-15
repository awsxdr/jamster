using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;
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
    public ActionResult<IEnumerable<EventModel>> GetEvents(Guid gameId)
    {
        logger.LogDebug("Getting events for game {gameId}", gameId);

        return gameDiscoveryService.GetExistingGame(gameId)
                .Then(game =>
                    gameDataStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(game))
                        .GetEvents()
                        .Select(e => (EventModel)e)
                        .Map(Result.Succeed))
            switch
            {
                Success<IEnumerable<EventModel>> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                _ => throw new UnexpectedResultException()
            };
    }

    [HttpPost("")]
    public async Task<ActionResult<EventModel>> AddEvent(Guid gameId, [FromBody] CreateEventModel model)
    {
        logger.LogDebug("Adding event {eventType} to game {gameId}", model.Type, gameId);

        return
            (await eventConverter.DecodeEvent(model.AsUntypedEvent())
                    .And(gameDiscoveryService.GetExistingGame(gameId))
                    .ThenMap(x => eventBus.AddEventAtCurrentTick(x.Item2, x.Item1)))
                switch
                {
                    Success<Event> s => Accepted((EventModel)s.Value),
                    Failure<EventTypeNotKnownError> => BadRequest(),
                    Failure<BodyFormatIncorrectError> => BadRequest(),
                    Failure<GameFileNotFoundForIdError> => NotFound(),
                    Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                    _ => throw new UnexpectedResultException()
                };
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid gameId, Guid eventId)
    {
        logger.LogDebug("Deleting event {eventId} from game {gameId}", eventId, gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
                .Then(game => eventBus.RemoveEvent(game, eventId))
            switch
            {
                Success => NoContent(),
                Failure<GameDataStore.EventNotFoundError> => NotFound(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
                Failure<EventBus.EventDeletionFailedError> => StatusCode(500),
                _ => throw new UnexpectedResultException()
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

}
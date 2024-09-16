using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

using Events;

[ApiController, Route("api/[controller]")]
public class GamesController(
    IGameDiscoveryService gameDiscoveryService,
    IEventConverter eventConverter,
    ILogger<GamesController> logger
    ) : Controller
{
    [HttpGet]
    public ActionResult<GameModel[]> GetGames()
    {
        logger.LogDebug("Getting games");

        var games = gameDiscoveryService.GetGames()
            .Select(g => (GameModel) g)
            .ToArray();

        return Ok(games);
    }

    [HttpPost]
    public ActionResult<GameModel> CreateGame([FromBody] CreateGameModel model)
    {
        logger.LogInformation("Creating game with name: {name}", model.Name);

        var game = (GameModel) gameDiscoveryService.GetGame(new(Guid.NewGuid(), model.Name));
        return Created($"api/games/{game.Id}", game);
    }

    [HttpPost("{gameId:guid}")]
    public ActionResult<EventCreatedModel> AddEvent(Guid gameId, [FromBody] CreateEventModel model)
    {
        return eventConverter.DecodeEvent(model.AsUntypedEvent()) switch
        {
            Success => Accepted(new EventCreatedModel(Guid.NewGuid())),
            Failure<EventTypeNotKnownError> => BadRequest(),
            Failure<BodyFormatIncorrectError> => BadRequest(),
            _ => throw new UnexpectedResultException()
        };
    }
}

public record GameModel(Guid Id, string Name)
{
    public static explicit operator GameModel(GameInfo game) => new(game.Id, game.Name);
}

public record CreateGameModel(string Name)
{
    public static explicit operator GameInfo(CreateGameModel model) => new(Guid.NewGuid(), model.Name);
}

public record EventCreatedModel(Guid EventId);

public record CreateEventModel(string Type, System.Text.Json.Nodes.JsonObject? Body)
{
    public IUntypedEvent AsUntypedEvent() =>
        Body is null
            ? new UntypedEvent(Type)
            : new UntypedEventWithBody(Type, Body);
}
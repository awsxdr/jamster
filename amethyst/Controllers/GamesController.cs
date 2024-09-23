using System.Text.Json;
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
    IGameContextFactory contextFactory,
    IEventConverter eventConverter,
    IEventBus eventBus,
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

    [HttpPost("{gameId:guid}/events")]
    public async Task<ActionResult<EventCreatedModel>> AddEvent(Guid gameId, [FromBody] CreateEventModel model)
    {
        logger.LogDebug("Adding event {eventType} to game {gameId}", model.Type, gameId);

        return 
            (await eventConverter.DecodeEvent(model.AsUntypedEvent()) 
                    .And(gameDiscoveryService.GetExistingGame(gameId))
                    .ThenMap(x => eventBus.AddEventAtCurrentTick(x.Item2, x.Item1)))
                switch
        {
            Success => Accepted(new EventCreatedModel(Guid.NewGuid())),
            Failure<EventTypeNotKnownError> => BadRequest(),
            Failure<BodyFormatIncorrectError> => BadRequest(),
            Failure<GameFileNotFoundForIdError> => NotFound(),
            Failure<MultipleGameFilesFoundForIdError> => StatusCode(500),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpGet("{gameId:guid}/state/{stateName}")]
    public ActionResult GetState(Guid gameId, string stateName)
    {
        logger.LogDebug("Retrieving state {stateName} for game {gameId}", stateName, gameId);

        return gameDiscoveryService.GetExistingGame(gameId)
                .ThenMap(contextFactory.GetGame)
                .Then(c => c.StateStore.GetStateByName(stateName))
            switch
            {
                Success<object> s => Ok(s.Value),
                Failure<StateNotFoundError> => NotFound(),
                _ => throw new UnexpectedResultException()
            };
    }

    [HttpGet("{gameId:guid}/state/{stateName}/updates")]
    public async Task<ActionResult> GetUpdatesStream(Guid gameId, string stateName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding listened for state {stateName} in game {gameId}", stateName, gameId);

        var contextResult = gameDiscoveryService.GetExistingGame(gameId).ThenMap(contextFactory.GetGame);

        if (contextResult is not Success<GameContext> context)
        {
            return contextResult switch
            {
                Failure<StateNotFoundError> => NotFound(),
                _ => throw new UnexpectedResultException()
            };
        }

        Response.Headers.Append("Content-Type", "text/event-stream");

        context.Value.StateStore.WatchStateByName(stateName, async state =>
        {
            await Response.WriteAsync("data: ", cancellationToken);
            await JsonSerializer.SerializeAsync(Response.Body, state, cancellationToken: cancellationToken);
            await Response.WriteAsync("\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        });

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(10, cancellationToken);
        }

        return Ok();
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
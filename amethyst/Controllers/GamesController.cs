using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/games")]
public class GamesController(
    IGameDiscoveryService gameDiscoveryService,
    IGameContextFactory contextFactory,
    ISystemStateStore systemStateStore,
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

    [HttpGet("{gameId:guid}")]
    public ActionResult<GameModel> GetGame(Guid gameId)
    {
        logger.LogDebug("Getting game {gameId}", gameId);

        return gameDiscoveryService.GetExistingGame(gameId) switch
        {
            Success<GameInfo> s => (GameModel)s.Value,
            Failure<GameFileNotFoundForIdError> => NotFound(),
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

    [HttpGet("current")]
    public ActionResult<GameModel> GetCurrentGame()
    {
        logger.LogDebug("Getting current game");

        return
            systemStateStore.GetCurrentGame() switch
            {
                Success<GameInfo> s => (GameModel) s.Value,
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<SystemStateDataStore.CurrentGameNotFoundError> => NotFound(),
                _ => throw new UnexpectedResultException()
            };
    }

    [HttpPut("current")]
    public async Task<ActionResult<GameModel>> SetCurrentGame(SetCurrentGameModel model)
    {
        logger.LogInformation("Setting current game to {gameId}", model.GameId);

        return await systemStateStore.SetCurrentGame(model.GameId) switch
        {
            Success<GameInfo> s => (GameModel) s.Value,
            Failure<GameFileNotFoundForIdError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    public record GameModel(Guid Id, string Name)
    {
        public static explicit operator GameModel(GameInfo game) => new(game.Id, game.Name);
    }

    public record CreateGameModel(string Name)
    {
        public static explicit operator GameInfo(CreateGameModel model) => new(Guid.NewGuid(), model.Name);
    }

    public record SetCurrentGameModel(Guid GameId);
}

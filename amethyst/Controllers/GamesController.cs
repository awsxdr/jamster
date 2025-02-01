using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;
using amethyst.Services.Stats;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/games")]
public class GamesController(
    IGameDiscoveryService gameDiscoveryService,
    IGameContextFactory contextFactory,
    ISystemStateStore systemStateStore,
    IGameExporter gameExporter,
    IStatsBookSerializer statsBookSerializer,
    ILogger<GamesController> logger
    ) : Controller
{
    [HttpGet]
    public async Task<ActionResult<GameModel[]>> GetGames()
    {
        logger.LogDebug("Getting games");

        var games = (await gameDiscoveryService.GetGames())
            .Select(g => (GameModel) g)
            .ToArray();

        return Ok(games);
    }

    [HttpPost]
    public async Task<ActionResult<GameModel>> CreateGame([FromBody] CreateGameModel model)
    {
        logger.LogInformation("Creating game with name: {name}", model.Name);

        var game = (GameModel) await gameDiscoveryService.GetGame(new(Guid.NewGuid(), model.Name));
        return Created($"api/games/{game.Id}", game);
    }

    [HttpPost]
    public async Task<ActionResult<GameModel>> UploadGame(
        IFormFile statsBookFile, 
        [FromServices] IGameImporter gameImporter)
    {
        if (statsBookFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            return new UnsupportedMediaTypeResult();

        await using var readStream = statsBookFile.OpenReadStream();

        var statsBookDeserializeResult = await statsBookSerializer.DeserializeStream(readStream);

        if (statsBookDeserializeResult is not Success<StatsBook> statsBook)
            return BadRequest();

        var game = await gameImporter.Import(statsBook.Value);

        return Created($"api/games/{game.Id}", (GameModel) game);
    }

    [HttpGet("{gameId:guid}")]
    public async Task<ActionResult<GameModel>> GetGame(Guid gameId)
    {
        switch (HttpContext.Request.Headers.Accept)
        {
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
            {
                    return await gameDiscoveryService.GetExistingGame(gameId)
                            .ThenMap(gameExporter.Export)
                            .Then(statsBookSerializer.Serialize) switch
                    {
                        Success<byte[]> s => File(s.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                        Failure<GameFileNotFoundForIdError> => NotFound(),
                        var r => throw new UnexpectedResultException(r)
                    };
            }

            default:
            {
                logger.LogDebug("Getting game {gameId}", gameId);

                return await gameDiscoveryService.GetExistingGame(gameId) switch
                {
                    Success<GameInfo> s => (GameModel)s.Value,
                    Failure<GameFileNotFoundForIdError> => NotFound(),
                    var r => throw new UnexpectedResultException(r)
                };
            }
        }
    }

    [HttpDelete("{gameId:guid}")]
    public async Task<ActionResult> DeleteGame(Guid gameId)
    {
        logger.LogInformation("Archiving game {gameId}", gameId);

        return await gameDiscoveryService.ArchiveGame(gameId) switch
        {
            Success => NoContent(),
            Failure<GameFileNotFoundForIdError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpGet("{gameId:guid}/state/{stateName}")]
    public async Task<ActionResult> GetState(Guid gameId, string stateName)
    {
        logger.LogDebug("Retrieving state {stateName} for game {gameId}", stateName, gameId);

        return await gameDiscoveryService.GetExistingGame(gameId)
                .ThenMap(contextFactory.GetGame)
                .Then(c => c.StateStore.GetStateByName(stateName))
            switch
            {
                Success<object> s => Ok(s.Value),
                Failure<StateNotFoundError> => NotFound(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpGet("current")]
    public async Task<ActionResult<GameModel>> GetCurrentGame()
    {
        logger.LogDebug("Getting current game");

        return
            await systemStateStore.GetCurrentGame() switch
            {
                Success<GameInfo> s => (GameModel) s.Value,
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<SystemStateDataStore.CurrentGameNotFoundError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
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
            var r => throw new UnexpectedResultException(r)
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

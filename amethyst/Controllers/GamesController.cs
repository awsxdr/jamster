using amethyst.DataStores;
using amethyst.Services;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/[controller]")]
public class GamesController(
    IGameDiscoveryService gameDiscoveryService,
    GameStoreFactory gameStoreFactory,
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
}

public record GameModel(Guid Id, string Name)
{
    public static explicit operator GameModel(GameInfo game) => new(game.Id, game.Name);
}

public record CreateGameModel(string Name)
{
    public static explicit operator GameInfo(CreateGameModel model) => new(Guid.NewGuid(), model.Name);
}
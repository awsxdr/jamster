using System.Text.Json;

using jamster.engine.Carolina;
using jamster.engine.Domain;

using Microsoft.AspNetCore.Mvc;

namespace jamster.engine.Controllers;

[ApiController, Route("/api/v1/carolina")]
public class CarolinaController(IStateTracker carolinaStateTracker) : Controller
{
    [HttpGet("game/{gameId:guid}")]
    public async Task<IActionResult> GetGameState(Guid gameId) =>
        await carolinaStateTracker.GetGameState(gameId) switch
        {
            Success<IReadOnlyDictionary<string, object?>> s => Content(JsonSerializer.Serialize(new { state = s.Value }, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }), "application/json"),
            Failure<GameNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
}

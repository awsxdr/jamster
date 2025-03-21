using amethyst.Domain;
using amethyst.Hubs;
using amethyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Controllers;

[ApiController, Route("/api/clients")]
public class ClientsController(IConnectedClientsService connectedClientsService, ILogger<ClientsController> logger) : Controller
{
    [HttpGet("")]
    public ActionResult<ClientModel[]> GetConnectedClients()
    {
        logger.LogDebug("Retrieving list of connected clients");

        return Ok(
            connectedClientsService.GetConnectedClients()
                .Select(c => (ClientModel)c)
                .ToArray());
    }

    [HttpGet("{connectionId}")]
    public ActionResult<ClientModel> GetClient(string connectionId)
    {
        logger.LogDebug("Getting details for client {clientId}", connectionId);

        return connectedClientsService.GetClient(connectionId) switch
        {
            Success<ConnectedClient> s => Ok((ClientModel) s.Value),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpPut("{connectionId}/name")]
    public async Task<IActionResult> SetConnectionName(string connectionId, [FromBody] SetNameModel model)
    {
        logger.LogDebug("Setting connection name for {connectionId} to {name}", connectionId, model.Name);

        return await connectedClientsService.SetClientName(connectionId, model.Name) switch
        {
            Success => NoContent(),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpPut("{connectionId}/activity")]
    public async Task<IActionResult> SetConnectionActivity(string connectionId, [FromBody] SetActivityModel model)
    {
        logger.LogDebug("Setting connection activity for {connectionId} to {activity} for game ID {gameId}", connectionId, model.Activity, model.GameId);

        return await connectedClientsService.RequestClientActivityChange(connectionId, model.Activity, model.GameId) switch
        {
            Success => NoContent(),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    public record ClientModel(string Id, string Name, ClientActivity CurrentActivity, string Path, string? GameId, DateTimeOffset LastUpdateTime)
    {
        public static explicit operator ClientModel(ConnectedClient client) => 
            new(client.Id, client.Name.Name, client.CurrentActivity, client.Path, client.GameId, client.LastUpdateTime);
    }

    public record SetNameModel(string Name);
    public record SetActivityModel(ClientActivity Activity, string? GameId);
}

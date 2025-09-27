using System.Text.Json;
using System.Text.Json.Nodes;

using jamster.engine.Domain;
using jamster.engine.Services;

using Microsoft.AspNetCore.Mvc;

namespace jamster.engine.Controllers;

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

    [HttpGet("{clientName}")]
    public ActionResult<ClientModel> GetClient(string clientName)
    {
        logger.LogDebug("Getting details for client {clientName}", clientName);

        return connectedClientsService.GetClientByName(clientName) switch
        {
            Success<ConnectedClient> s => Ok((ClientModel) s.Value),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpPut("{clientName}/name")]
    public async Task<IActionResult> SetConnectionName(string clientName, [FromBody] SetNameModel model)
    {
        logger.LogDebug("Setting connection name from {connectionId} to {name}", clientName, model.Name);

        return await connectedClientsService.SetClientName(clientName, model.Name) switch
        {
            Success => NoContent(),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpPut("{clientName}/activity")]
    public async Task<IActionResult> SetConnectionActivity(string clientName, [FromBody] SetActivityModel model)
    {
        logger.LogDebug("Setting connection activity for {clientName} to {activity}", clientName, model.ActivityDetails[nameof(ActivityData.Activity)]);

        var baseActivityData = model.ActivityDetails.Deserialize<ActivityData>(Program.JsonSerializerOptions);

        if (baseActivityData == null)
            return BadRequest();

        var activityDetails = baseActivityData.Activity switch
        {
            ClientActivity.Scoreboard => model.ActivityDetails.Deserialize<ScoreboardActivity>(Program.JsonSerializerOptions),
            ClientActivity.StreamOverlay => model.ActivityDetails.Deserialize<StreamOverlayActivity>(Program.JsonSerializerOptions),
            _ => baseActivityData
        };

        if (activityDetails == null)
            return BadRequest();

        return await connectedClientsService.RequestClientActivityChange(clientName, activityDetails) switch
        {
            Success => NoContent(),
            Failure<ClientNotFoundError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    public sealed record ClientModel(string Name, string IpAddress, JsonObject ActivityInfo, DateTimeOffset LastUpdateTime)
    {
        public static explicit operator ClientModel(ConnectedClient client) => 
            new(
                client.Name.Name,
                client.IpAddress,
                JsonSerializer.SerializeToNode(
                    client.ActivityInfo as object /* cast to object to force serialization of derived properties */,
                    Program.JsonSerializerOptions
                )!.AsObject(),
                client.LastUpdateTime);

        public bool Equals(ClientModel? other) =>
            other is not null
            && other.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)
            && other.IpAddress.Equals(IpAddress)
            && other.LastUpdateTime.Equals(LastUpdateTime)
            && other.ActivityInfo.ToJsonString().Equals(ActivityInfo.ToJsonString());

        public override int GetHashCode() => 
            HashCode.Combine(Name, IpAddress, ActivityInfo, LastUpdateTime);
    }

    public record SetNameModel(string Name);
    public record SetActivityModel(JsonObject ActivityDetails);
}

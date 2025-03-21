using System.Net;
using amethyst.Controllers;
using amethyst.Hubs;
using amethyst.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace amethyst.tests.Controllers;

public class ClientsControllerIntegrationTests : ControllerIntegrationTest
{
    private const string HubAddress = "api/hubs/clients";

    [Test]
    public async Task GetConnectedClients_ReturnsAllConnectedClients()
    {
        await using var hubConnection1 = await GetHubConnection(HubAddress);
        await using var hubConnection2 = await GetHubConnection(HubAddress);

        await hubConnection1.InvokeAsync(nameof(ConnectedClientsHub.SetConnectionName), "Test Client");

        var clientsList = await Get<ClientsController.ClientModel[]>("/api/clients", HttpStatusCode.OK);

        clientsList.Should().BeEquivalentTo([
            (ClientsController.ClientModel)await hubConnection1.InvokeAsync<ConnectedClient>(nameof(ConnectedClientsHub.GetConnectionDetails)),
            (ClientsController.ClientModel)await hubConnection2.InvokeAsync<ConnectedClient>(nameof(ConnectedClientsHub.GetConnectionDetails))
        ]);
    }

    [Test]
    public async Task GetClient_WhenClientExists_ReturnsClientDetails()
    {
        await using var hubConnection = await GetHubConnection(HubAddress);

        await hubConnection.InvokeAsync(nameof(ConnectedClientsHub.SetConnectionName), "Test Client");

        var connectionDetails =
            (ClientsController.ClientModel)await hubConnection.InvokeAsync<ConnectedClient>(nameof(ConnectedClientsHub.GetConnectionDetails));

        var client = await Get<ClientsController.ClientModel>($"/api/clients/{connectionDetails.Id}", HttpStatusCode.OK);

        client.Should().Be(connectionDetails);
    }

    [Test]
    public async Task GetClient_WhenClientDoesNotExist_ReturnsNotFoundResponse()
    {
        await Get<ClientsController.ClientModel>("/api/clients/invalidClientId", HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetConnectionName_WhenClientExists_UpdatesClientName()
    {
        await using var hubConnection = await GetHubConnection(HubAddress);

        await Put($"/api/clients/{hubConnection.ConnectionId}/name", new ClientsController.SetNameModel("Test Name"), HttpStatusCode.NoContent);

        var client = await Get<ClientsController.ClientModel>($"/api/clients/{hubConnection.ConnectionId}", HttpStatusCode.OK);

        client!.Name.Should().Be("Test Name");
    }

    [Test]
    public async Task SetConnectionName_WhenClientDoesNotExist_ReturnsNotFoundResponse()
    {
        await Put("/api/clients/invalidClientId/name", new ClientsController.SetNameModel("Ignored"), HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetConnectionName_WhenClientExists_NotifiesClientsOfChange()
    {
        await using var hubConnection = await GetHubConnection(HubAddress);

        var completionSource = new TaskCompletionSource<ConnectedClient>();

        using var handler = hubConnection.On("ConnectedClientsChanged", [typeof(ConnectedClient[])], parameters =>
        {
            completionSource.SetResult(((ConnectedClient[])parameters[0]!).Single());

            return Task.CompletedTask;
        });

        await hubConnection.InvokeAsync(nameof(ConnectedClientsHub.WatchClientsList));

        await Put($"/api/clients/{hubConnection.ConnectionId}/name",
            new ClientsController.SetNameModel("Test Name"), HttpStatusCode.NoContent);

        var client = await Wait(completionSource.Task);

        client.Name.Name.Should().Be("Test Name");
    }

    [Test]
    public async Task SetConnectionActivity_WhenClientDoesNotExist_ReturnsNotFoundResponse()
    {
        await Put("/api/clients/invalidClientId/activity", new ClientsController.SetActivityModel(ClientActivity.PenaltyWhiteboard, Guid.NewGuid().ToString()), HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetConnectionActivity_WhenClientExists_NotifiesClientOfChange()
    {
        await using var hubConnection = await GetHubConnection(HubAddress);

        var completionSource = new TaskCompletionSource<(ClientActivity Activity, string? GameId)>();

        using var handler = hubConnection.On("ChangeActivity", [typeof(ClientActivity), typeof(string)], parameters =>
        {
            completionSource.SetResult((
                (ClientActivity)parameters[0]!,
                (string?)parameters[1]
            ));

            return Task.CompletedTask;
        });

        await hubConnection.InvokeAsync(nameof(ConnectedClientsHub.WatchClientsList));

        var gameId = Guid.NewGuid().ToString();

        await Put($"/api/clients/{hubConnection.ConnectionId}/activity",
            new ClientsController.SetActivityModel(ClientActivity.PenaltyWhiteboard, gameId), HttpStatusCode.NoContent);

        var result = await Wait(completionSource.Task);

        result.Activity.Should().Be(ClientActivity.PenaltyWhiteboard);
        result.GameId.Should().Be(gameId);
    }

    protected override void CleanDatabase()
    {
    }
}
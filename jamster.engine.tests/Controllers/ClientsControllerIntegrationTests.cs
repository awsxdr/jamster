using System.Net;
using System.Text.Json;
using jamster.Controllers;
using jamster.Domain;
using jamster.Hubs;
using jamster.Services;
using FluentAssertions;
using Func;
using Microsoft.AspNetCore.SignalR.Client;

namespace jamster.engine.tests.Controllers;

public class ClientsControllerIntegrationTests : ControllerIntegrationTest
{
    private const string HubAddress = "api/hubs/clients";

    [Test]
    public async Task GetConnectedClients_ReturnsAllConnectedClients()
    {
        await using var hubConnection1 = await GetHubConnection(HubAddress);
        await using var hubConnection2 = await GetHubConnection(HubAddress);

        await hubConnection1.InvokeAsync(nameof(ConnectedClientsHub.SetConnectionName), "Test Client");

        var clientsList = (await Get<ClientsController.ClientModel[]>("/api/clients", HttpStatusCode.OK))?.Select(v => JsonSerializer.Serialize(v, Program.JsonSerializerOptions)) ?? [];

        clientsList.Should().BeEquivalentTo([
            (await hubConnection1.InvokeAsync<ClientsController.ClientModel>(nameof(ConnectedClientsHub.GetConnectionDetails))).Map(v => JsonSerializer.Serialize(v, Program.JsonSerializerOptions)),
            (await hubConnection2.InvokeAsync<ClientsController.ClientModel>(nameof(ConnectedClientsHub.GetConnectionDetails))).Map(v => JsonSerializer.Serialize(v, Program.JsonSerializerOptions))
        ]);
    }

    [Test]
    public async Task GetClient_WhenClientExists_ReturnsClientDetails()
    {
        await using var hubConnection = await GetHubConnection(HubAddress);

        await hubConnection.InvokeAsync(nameof(ConnectedClientsHub.SetConnectionName), "Test Client");

        var connectionDetails =
            await hubConnection.InvokeAsync<ClientsController.ClientModel>(nameof(ConnectedClientsHub.GetConnectionDetails));

        var client = await Get<ClientsController.ClientModel>("/api/clients/Test%20Client", HttpStatusCode.OK);

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

        var clientName = await hubConnection.InvokeAsync<string>(nameof(ConnectedClientsHub.GetConnectionName));

        await Put($"/api/clients/{clientName}/name", new ClientsController.SetNameModel("Test Name"), HttpStatusCode.NoContent);

        var client = await Get<ClientsController.ClientModel>($"/api/clients/Test Name", HttpStatusCode.OK);

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

        var completionSource = new TaskCompletionSource<ClientsController.ClientModel>();

        using var handler = hubConnection.On("ConnectedClientsChanged", [typeof(ClientsController.ClientModel[])], parameters =>
        {
            completionSource.SetResult(((ClientsController.ClientModel[])parameters[0]!).Single());

            return Task.CompletedTask;
        });

        var clientName = await hubConnection.InvokeAsync<string>(nameof(ConnectedClientsHub.GetConnectionName));

        await hubConnection.InvokeAsync(nameof(ConnectedClientsHub.WatchClientsList));

        await Put($"/api/clients/{clientName}/name",
            new ClientsController.SetNameModel("Test Name"), HttpStatusCode.NoContent);

        var client = await Wait(completionSource.Task);

        client.Name.Should().Be("Test Name");
    }

    [Test]
    public async Task SetConnectionActivity_WhenClientDoesNotExist_ReturnsNotFoundResponse()
    {
        await Put(
            "/api/clients/invalidClientId/activity", 
            new ClientsController.SetActivityModel(JsonSerializer.SerializeToNode(new ScoreboardActivity(Guid.NewGuid().ToString(), "xx", true, true))!.AsObject()), 
            HttpStatusCode.NotFound);
    }

    protected override void CleanDatabase()
    {
    }
}
using FluentAssertions;

using jamster.engine.Controllers;
using jamster.engine.Hubs;

using Microsoft.AspNetCore.SignalR.Client;

namespace jamster.engine.tests.Controllers;

public class ConnectedClientsHubIntegrationTests : ControllerIntegrationTest
{
    private const string HubAddress = "api/hubs/clients";

    [Test]
    public async Task GetConnectionName_ReturnsCurrentConnectionName()
    {
        await using var connection = await GetHubConnection(HubAddress);

        var name = await connection.InvokeAsync<string>(nameof(ConnectedClientsHub.GetConnectionName));

        name.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task SetConnectionName_UpdatesConnectionName()
    {
        await using var connection = await GetHubConnection(HubAddress);

        await connection.InvokeAsync(nameof(ConnectedClientsHub.SetConnectionName), "Test Connection");

        var name = await connection.InvokeAsync<string>(nameof(ConnectedClientsHub.GetConnectionName));

        name.Should().Be("Test Connection");
    }

    [Test]
    public async Task Hub_WhenClientConnects_InvokesConnectedClientsChangedOnAllConnections()
    {
        HubConnection[] hubConnections =
        [
            await GetHubConnection(HubAddress),
            await GetHubConnection(HubAddress),
            await GetHubConnection(HubAddress),
        ];

        try
        {
            var completionSources = hubConnections.Select(_ => new TaskCompletionSource<ClientsController.ClientModel[]>()).ToArray();

            foreach (var (connection, completionSource) in hubConnections.Zip(completionSources))
            {
                connection.On("ConnectedClientsChanged", [typeof(ClientsController.ClientModel[])], parameters =>
                {
                    if (completionSource.Task.IsCompleted)
                        return Task.CompletedTask;

                    var result = (ClientsController.ClientModel[])parameters[0]!;
                    completionSource.SetResult(result);
                    return Task.CompletedTask;
                });

                await connection.InvokeAsync(nameof(ConnectedClientsHub.WatchClientsList));
            }

            await using var newConnection = await GetHubConnection(HubAddress);

            var expectedClientDetails = await Task.WhenAll(hubConnections.Append(newConnection).Select(h =>
                h.InvokeAsync<ClientsController.ClientModel>(nameof(ConnectedClientsHub.GetConnectionDetails))));

            var results = await WaitAll(completionSources.Select(c => c.Task).ToArray());
            
            results.Select(r => r.Select(m => new { m.Name, m.IpAddress, ActivityInfo = m.ActivityInfo.ToJsonString(), m.LastUpdateTime }))
                .Should().AllBeEquivalentTo(expectedClientDetails.Select(m => new { m.Name, m.IpAddress, ActivityInfo = m.ActivityInfo.ToJsonString(), m.LastUpdateTime }));
        }
        finally
        {
            foreach (var connection in hubConnections)
            {
                await connection.DisposeAsync();
            }
        }
    }

    [Test]
    public async Task RequestActivityChange_InvokesChangeActivityOnTargetConnection()
    {
        await using var connection1 = await GetHubConnection(HubAddress);
        await using var connection2 = await GetHubConnection(HubAddress);

        
    }

    protected override void CleanDatabase()
    {
    }
}
using amethyst.Domain;
using amethyst.Hubs;
using amethyst.Services;
using FluentAssertions;
using Func;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace amethyst.tests.Services;

public class ConnectedClientsServiceUnitTests : UnitTest<ConnectedClientsService>
{
    [Test]
    public async Task GetConnectedClients_ReturnsAllClientsAddedWithRegisterClient()
    {
        var client1 = await Subject.RegisterClient("client1", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value.Client, _ => throw new AssertionException("Register client failed") };
        var client2 = await Subject.RegisterClient("client2", "192.168.12.35") switch { Success<(ConnectedClient Client, Guid)> c => c.Value.Client, _ => throw new AssertionException("Register client failed") };

        var clients = Subject.GetConnectedClients().ToArray();

        clients.Should().BeEquivalentTo(new ConnectedClient[]
        {
            new("client1", client1.Name, "192.168.12.34", new UnknownActivity(), clients.Single(c => c.ConnectionId == "client1").LastUpdateTime),
            new("client2", client2.Name, "192.168.12.35", new UnknownActivity(), clients.Single(c => c.ConnectionId == "client2").LastUpdateTime),
        });
    }

    [Test]
    public async Task RegisterClient_RaisesConnectedClientsChangedEvent()
    {
        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.RegisterClient("testClient", "192.168.12.34");

        var result = await Wait(completionSource.Task);
        result.Should().ContainSingle().Which.ConnectionId.Should().Be("testClient");
    }

    [Test]
    public async Task RegisterClient_ShouldAlwaysProduceUniqueName()
    {
        const int clientCount = 10_000;

        var clientNames = await Task.WhenAll(Enumerable.Range(0, clientCount).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}")));
        
        clientNames.Distinct().Count().Should().Be(clientCount);
    }

    [Test]
    public async Task RegisterClient_WhenClientAlreadyRegistered_FailsWithConnectionIdAlreadyRegisteredError()
    {
        var result1 = await Subject.RegisterClient("testClient", "192.168.12.34");
        var result2 = await Subject.RegisterClient("testClient", "192.168.12.34");

        result1.Should().BeSuccess();
        result2.Should().BeFailure<ConnectionIdAlreadyRegisteredError>();
    }

    [Test]
    public async Task UnregisterClient_RemovesClientFromList()
    {
        var clients = (await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}"))))
            .OfType<Success<(ConnectedClient Client, Guid Id)>>()
            .Select(s => s.Value)
            .ToArray();

        clients.Should().HaveCount(5);

        await Subject.UnregisterClient(clients[2].Id);

        var connectedClients = Subject.GetConnectedClients().ToArray();

        connectedClients.Select(c => c.Name.Name).Should().BeEquivalentTo(((int[]) [0, 1, 3, 4]).Select(i => clients[i].Client.Name.Name));
    }

    [Test]
    public async Task UnregisterClient_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        var clients = (await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}"))))
            .OfType<Success<(ConnectedClient Client, Guid Id)>>()
            .Select(s => s.Value)
            .ToArray();

        clients.Should().HaveCount(5);

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.UnregisterClient(clients[2].Id);

        var result = await Wait(completionSource.Task);

        var eventClientNames = result.Select(c => c.Name.Name).ToArray();

        eventClientNames.Should().BeEquivalentTo(((int[])[0, 1, 3, 4]).Select(i => clients[i].Client.Name.Name));
    }

    [Test]
    public async Task UnregisterClient_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.UnregisterClient(Guid.NewGuid());

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task SetClientActivity_UpdatesActivity()
    {
        var (client1, client1Id) = await Subject.RegisterClient("client1", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };
        var (client2, client2Id) = await Subject.RegisterClient("client2", "192.168.12.35") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        var game1Id = Guid.NewGuid().ToString();

        await Subject.SetClientActivity(client1Id, new ScoreboardActivity(game1Id, "xx", true, true));
        await Subject.SetClientActivity(client2Id, new PenaltyLineupControlActivity(null, "xx"));

        var clients = Subject.GetConnectedClients().ToArray();

        clients.Should().BeEquivalentTo(new ConnectedClient[]
        {
            new("client1", client1.Name, "192.168.12.34", new ScoreboardActivity(game1Id, "xx", true, true), clients.Single(c => c.ConnectionId == "client1").LastUpdateTime),
            new("client2", client2.Name, "192.168.12.35", new PenaltyLineupControlActivity(null, "xx"), clients.Single(c => c.ConnectionId == "client2").LastUpdateTime),
        });
    }

    [Test]
    public async Task SetClientActivity_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        var (client1, client1Id) = await Subject.RegisterClient("client1", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };
        var (client2, _) = await Subject.RegisterClient("client2", "192.168.12.35") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.SetClientActivity(client1Id, new ScoreboardActivity(Guid.NewGuid().ToString(), "xx", true, true));

        var result = await Wait(completionSource.Task);

        result.Select(c => new { c.ConnectionId, c.Name, c.ActivityInfo.Activity })
            .Should().BeEquivalentTo([
                new { ConnectionId = "client1", client1.Name, Activity = ClientActivity.Scoreboard },
                new { ConnectionId = "client2", client2.Name, Activity = ClientActivity.Unknown },
            ]);
    }

    [Test]
    public async Task SetClientActivity_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.SetClientActivity(Guid.NewGuid(), new BoxTimingActivity(null, "xx"));

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task RequestClientActivityChange_WhenClientExists_NotifiesClientOfRequest()
    {
        GetMock<IHubContext<ConnectedClientsHub>>()
            .Setup(mock => mock.Clients.Client("testClient"))
            .Returns(() => GetMock<ISingleClientProxy>().Object);

        GetMock<ISingleClientProxy>()
            .Setup(mock => mock.SendCoreAsync("ChangeActivity", new object?[] { It.IsAny<ClientActivity>(), It.IsAny<string?>(), It.IsAny<string>() }, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (client, _) = await Subject.RegisterClient("testClient", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        var gameId = Guid.NewGuid().ToString();
        await Subject.RequestClientActivityChange(client.Name.Name, new LineupControlActivity(gameId, "xx"));

        GetMock<ISingleClientProxy>()
            .Verify(mock => mock.SendCoreAsync("ChangeActivity", new object?[] { new LineupControlActivity(gameId, "xx") }, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SetClientName_UpdatesName()
    {
        var (client, _) = await Subject.RegisterClient("testClient", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        await Subject.SetClientName(client.Name.Name, "Custom Name");

        Subject.GetConnectedClients().Should().ContainSingle()
            .Which.Name.Should().Be(new ClientName("Custom Name", true));
    }

    [Test]
    public async Task SetClientName_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        var (client1, _) = await Subject.RegisterClient("client1", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") }; ;
        var (client2, _) = await Subject.RegisterClient("client2", "192.168.12.35") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.SetClientName(client1.Name.Name, "Client 1");

        var result = await Wait(completionSource.Task);

        result.Select(c => new { c.ConnectionId, c.Name, c.ActivityInfo.Activity })
            .Should().BeEquivalentTo([
                new { ConnectionId = "client1", Name = new ClientName("Client 1", true), Activity = ClientActivity.Unknown },
                new { ConnectionId = "client2", client2.Name, Activity = ClientActivity.Unknown },
            ]);
    }

    [Test]
    public async Task SetClientName_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}", $"192.168.12.{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.SetClientName(Guid.NewGuid().ToString(), "Test");

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task GetClientById_ReturnsClientDetails()
    {
        var (client, clientId) = await Subject.RegisterClient("testClient", "192.168.12.34") switch { Success<(ConnectedClient Client, Guid)> c => c.Value, _ => throw new AssertionException("Register client failed") };

        var clientDetails = Subject.GetClientById(clientId);
        clientDetails.Should().BeSuccess<ConnectedClient>(out var result);
            
        result.Name.Should().Be(client.Name);
        result.ConnectionId.Should().Be("testClient");
        result.ActivityInfo.Should().BeOfType<UnknownActivity>();
    }
}
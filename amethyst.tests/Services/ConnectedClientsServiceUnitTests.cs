using amethyst.Hubs;
using amethyst.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace amethyst.tests.Services;

public class ConnectedClientsServiceUnitTests : UnitTest<ConnectedClientsService>
{
    [Test]
    public async Task GetConnectedClients_ReturnsAllClientsAddedWithRegisterClient()
    {
        var client1Name = await Subject.RegisterClient("client1");
        var client2Name = await Subject.RegisterClient("client2");

        var clients = Subject.GetConnectedClients().ToArray();

        clients.Should().BeEquivalentTo(new ConnectedClient[]
        {
            new("client1", client1Name, ClientActivity.Unknown, string.Empty, null, clients.Single(c => c.Id == "client1").LastUpdateTime),
            new("client2", client2Name, ClientActivity.Unknown, string.Empty, null, clients.Single(c => c.Id == "client2").LastUpdateTime),
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

        await Subject.RegisterClient("testClient");

        var result = await Wait(completionSource.Task);
        result.Should().ContainSingle().Which.Id.Should().Be("testClient");
    }

    [Test]
    public async Task RegisterClient_ShouldAlwaysProduceUniqueName()
    {
        const int clientCount = 10_000;

        var clientNames = await Task.WhenAll(Enumerable.Range(0, clientCount).Select(i => Subject.RegisterClient($"testClient{i}")));
        
        clientNames.Distinct().Count().Should().Be(clientCount);
    }

    [Test]
    public async Task RegisterClient_WhenClientAlreadyRegistered_UpdatesExistingClient()
    {
        var originalClientName = await Subject.RegisterClient("testClient");
        var originalUpdateTime = Subject.GetConnectedClients().Single().LastUpdateTime;

        await Task.Delay(1); // Ensure at least 1 millisecond passes so update time is different

        var newClientName = await Subject.RegisterClient("testClient");
        var newUpdateTime = Subject.GetConnectedClients().Single().LastUpdateTime;

        originalClientName.Should().Be(newClientName);
        newUpdateTime.Should().BeAfter(originalUpdateTime);
    }

    [Test]
    public async Task UnregisterClient_RemovesClientFromList()
    {
        var clientNames = await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}")));

        await Subject.UnregisterClient("testClient2");

        var clients = Subject.GetConnectedClients().ToArray();

        clients.Select(c => c.Name.Name).Should().BeEquivalentTo(clientNames[0], clientNames[1], clientNames[3], clientNames[4]);
    }

    [Test]
    public async Task UnregisterClient_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        var clientNames = await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}")));

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.UnregisterClient("testClient2");

        var result = await Wait(completionSource.Task);

        var eventClientNames = result.Select(c => c.Name.Name).ToArray();

        eventClientNames.Should().BeEquivalentTo(clientNames[0], clientNames[1], clientNames[3], clientNames[4]);
    }

    [Test]
    public async Task UnregisterClient_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.UnregisterClient(Guid.NewGuid().ToString());

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task SetClientActivity_UpdatesActivity()
    {
        var client1Name = await Subject.RegisterClient("client1");
        var client2Name = await Subject.RegisterClient("client2");

        var game1Id = Guid.NewGuid().ToString();

        await Subject.SetClientActivity("client1", ClientActivity.ScoreboardOperator, "TestPath/1", game1Id);
        await Subject.SetClientActivity("client2", ClientActivity.PenaltyLineupControl, "TestPath/2", null);

        var clients = Subject.GetConnectedClients().ToArray();

        clients.Should().BeEquivalentTo(new ConnectedClient[]
        {
            new("client1", client1Name, ClientActivity.ScoreboardOperator, "TestPath/1", game1Id, clients.Single(c => c.Id == "client1").LastUpdateTime),
            new("client2", client2Name, ClientActivity.PenaltyLineupControl, "TestPath/2", null, clients.Single(c => c.Id == "client2").LastUpdateTime),
        });
    }

    [Test]
    public async Task SetClientActivity_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        var client1Name = await Subject.RegisterClient("client1");
        var client2Name = await Subject.RegisterClient("client2");

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.SetClientActivity("client1", ClientActivity.ScoreboardOperator, "TestPath/1", Guid.NewGuid().ToString());

        var result = await Wait(completionSource.Task);

        result.Select(c => new { c.Id, c.Name, c.CurrentActivity, c.Path })
            .Should().BeEquivalentTo([
                new { Id = "client1", Name = new ClientName(client1Name, false), CurrentActivity = ClientActivity.ScoreboardOperator, Path = "TestPath/1" },
                new { Id = "client2", Name = new ClientName(client2Name, false), CurrentActivity = ClientActivity.Unknown, Path = string.Empty },
            ]);
    }

    [Test]
    public async Task SetClientActivity_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.SetClientActivity(Guid.NewGuid().ToString(), ClientActivity.BoxTiming, string.Empty, null);

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task RequestClientActivityChange_WhenClientExists_NotifiesClientOfRequest()
    {
        GetMock<IHubContext<ConnectedClientsHub>>()
            .Setup(mock => mock.Clients.Client("testClient"))
            .Returns(() => GetMock<ISingleClientProxy>().Object);

        GetMock<ISingleClientProxy>()
            .Setup(mock => mock.SendCoreAsync("ChangeActivity", new object?[] { It.IsAny<ClientActivity>(), It.IsAny<string?>() }, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Subject.RegisterClient("testClient");

        var gameId = Guid.NewGuid().ToString();
        await Subject.RequestClientActivityChange("testClient", ClientActivity.LineupControl, gameId);

        GetMock<ISingleClientProxy>()
            .Verify(mock => mock.SendCoreAsync("ChangeActivity", new object?[] { ClientActivity.LineupControl, gameId }, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SetClientName_UpdatesName()
    {
        await Subject.RegisterClient("testClient");

        await Subject.SetClientName("testClient", "Custom Name");

        Subject.GetConnectedClients().Should().ContainSingle()
            .Which.Name.Should().Be(new ClientName("Custom Name", true));
    }

    [Test]
    public async Task SetClientName_WhenClientExists_RaisesConnectedClientsChangedEvent()
    {
        await Subject.RegisterClient("client1");
        var client2Name = await Subject.RegisterClient("client2");

        var completionSource = new TaskCompletionSource<ConnectedClient[]>();

        Subject.ConnectedClientsChanged += (_, e) =>
        {
            completionSource.SetResult(e.Clients);

            return Task.CompletedTask;
        };

        await Subject.SetClientName("client1", "Client 1");

        var result = await Wait(completionSource.Task);

        result.Select(c => new { c.Id, c.Name, c.CurrentActivity, c.Path })
            .Should().BeEquivalentTo([
                new { Id = "client1", Name = new ClientName("Client 1", true), CurrentActivity = ClientActivity.Unknown, Path = string.Empty },
                new { Id = "client2", Name = new ClientName(client2Name, false), CurrentActivity = ClientActivity.Unknown, Path = string.Empty },
            ]);
    }

    [Test]
    public async Task SetClientName_WhenClientDoesNotExist_DoesNotRaiseConnectedClientsChangedEvent()
    {
        await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Subject.RegisterClient($"testClient{i}")));

        using var monitoredSubject = Subject.Monitor();

        await Subject.SetClientName(Guid.NewGuid().ToString(), "Test");

        monitoredSubject.Should().NotRaise(nameof(Subject.ConnectedClientsChanged));
    }

    [Test]
    public async Task GetClient_ReturnsClientDetails()
    {
        var clientName = await Subject.RegisterClient("testClient");

        var clientDetails = Subject.GetClient("testClient");
        clientDetails.Should().BeSuccess<ConnectedClient>(out var client);
            
        client.Name.Should().Be(new ClientName(clientName, false));
        client.Id.Should().Be("testClient");
        client.CurrentActivity.Should().Be(ClientActivity.Unknown);
        client.Path.Should().BeEmpty();
    }
}
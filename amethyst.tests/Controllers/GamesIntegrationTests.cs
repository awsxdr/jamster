using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Controllers;
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Hubs;
using amethyst.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace amethyst.tests.Controllers;

using amethyst.Reducers;

public class GamesIntegrationTests : ControllerIntegrationTest
{
    private GamesController.GameModel _game;

    public override void Setup()
    {
        base.Setup();

        _game = Post<GamesController.GameModel>("/api/games", new GamesController.CreateGameModel("Test game"), HttpStatusCode.Created).Result!;
    }

    [Test]
    public async Task NewGameAddedToGamesList()
    {
        var gamesList = await Get<GamesController.GameModel[]>("/api/games", HttpStatusCode.OK);
        gamesList.Should().NotBeNull();
        gamesList!.Length.Should().Be(1);
        gamesList.Should().BeEquivalentTo(new[] { _game });
    }

    [Test]
    public async Task SendEvents()
    {
        await Post<GamesController.EventCreatedModel>(
            $"/api/games/{_game.Id}/events",
            new GamesController.CreateEventModel(nameof(TestEvent),
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
            HttpStatusCode.Accepted);

        await Post<GamesController.EventCreatedModel>(
            $"/api/games/{_game.Id}/events",
            new GamesController.CreateEventModel($"InvalidEvent_{Guid.NewGuid()}",
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
            HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task EventsChangeState()
    {
        var state = (await Get<JamClockState>($"/api/games/{_game.Id}/state/{nameof(JamClockState)}", HttpStatusCode.OK))!;

        state.IsRunning.Should().BeFalse();

        await Post<GamesController.EventCreatedModel>(
            $"/api/games/{_game.Id}/events",
            new GamesController.CreateEventModel(nameof(JamStarted), null),
            HttpStatusCode.Accepted);

        state = (await Get<JamClockState>($"/api/games/{_game.Id}/state/{nameof(JamClockState)}", HttpStatusCode.OK))!;

        state.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task GetCurrentGame_WhenNotSet_ReturnsNotFoundStatus()
    {
        var result = await Get("/api/games/current");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetCurrentGame_AfterSet_ReturnsCurrentGame()
    {
        await Post("/api/games/current", new GamesController.SetCurrentGameModel(_game.Id), HttpStatusCode.OK);
        var result = await Get<GamesController.GameModel>("/api/games/current", HttpStatusCode.OK);

        result.Should().Be(_game);
    }

    [Test]
    public async Task SetCurrentGame_NotifiesWatchingClients()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(
                Client.BaseAddress + "api/Hubs/System",
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                })
            .Build();

        await connection.StartAsync();

        await connection.InvokeAsync(nameof(SystemStateHub.WatchSystemState));

        var taskCompletionSource = new TaskCompletionSource<Guid>();

        connection.On("CurrentGameChanged", (Guid newGameId) =>
        {
            taskCompletionSource.SetResult(newGameId);
        });

        await Post("/api/games/current", new GamesController.SetCurrentGameModel(_game.Id), HttpStatusCode.OK);

        var gameId = await taskCompletionSource.Task;

        gameId.Should().Be(_game.Id);
    }

    protected override void CleanDatabase()
    {
        GameDataStoreFactory?.ReleaseConnections();
        GC.Collect(); // Force SQLite to release database files

        foreach (var databaseFile in Directory.GetFiles(GameDataStore.GamesFolder, "*.db"))
        {
            File.Delete(databaseFile);
        }
    }
}

public class TestEvent(Guid7 id, TestEventBody body) : Event<TestEventBody>(id, body);

public class TestEventBody
{
    public string Value { get; set; } = string.Empty;
}
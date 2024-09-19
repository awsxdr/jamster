using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Controllers;
using amethyst.DataStores;
using amethyst.Events;
using FluentAssertions;

namespace amethyst.tests.Controllers;

using amethyst.Reducers;

public class GamesIntegrationTests : ControllerIntegrationTest
{
    [Test]
    public async Task NewGameAddedToGamesList()
    {
        var gamesList = await Get<GameModel[]>("/api/games", HttpStatusCode.OK);

        gamesList.Should().NotBeNull().And.BeEmpty();

        var postResult = await Post<GameModel>("/api/games", new CreateGameModel("Test game"), HttpStatusCode.Created);
        postResult?.Name.Should().Be("Test game");

        gamesList = await Get<GameModel[]>("/api/games", HttpStatusCode.OK);
        gamesList.Should().NotBeNull();
        gamesList!.Length.Should().Be(1);
        gamesList.Should().BeEquivalentTo(new[] { postResult });
    }

    [Test]
    public async Task SendEvents()
    {
        var game = (await Post<GameModel>("/api/games", new CreateGameModel("Test game"), HttpStatusCode.Created))!;
        await Post<EventCreatedModel>(
            $"/api/games/{game.Id}/events",
            new CreateEventModel(nameof(TestEvent),
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
            HttpStatusCode.Accepted);

        await Post<EventCreatedModel>(
            $"/api/games/{game.Id}/events",
            new CreateEventModel($"InvalidEvent_{Guid.NewGuid()}",
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
            HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task EventsChangeState()
    {
        var game = (await Post<GameModel>("/api/games", new CreateGameModel("Test game"), HttpStatusCode.Created))!;

        var state = (await Get<JamClockState>($"/api/games/{game.Id}/state/{nameof(JamClockState)}", HttpStatusCode.OK))!;

        state.IsRunning.Should().BeFalse();

        await Post<EventCreatedModel>(
            $"/api/games/{game.Id}/events",
            new CreateEventModel(nameof(JamStarted), null),
            HttpStatusCode.Accepted);

        state = (await Get<JamClockState>($"/api/games/{game.Id}/state/{nameof(JamClockState)}", HttpStatusCode.OK))!;

        state.IsRunning.Should().BeTrue();
    }

    protected override void CleanDatabase()
    {
        GC.Collect(); // Force SQLite to release database files
        foreach (var databaseFile in Directory.GetFiles(GameDataStore.GamesFolder, "*.db"))
        {
            File.Delete(databaseFile);
        }
    }
}

public class TestEvent(long tick, TestEventBody body) : Event<TestEventBody>(tick, body);

public class TestEventBody
{
    public string Value { get; set; }
}
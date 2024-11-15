using amethyst.Controllers;
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Domain;
using FluentAssertions;
using Func;

namespace amethyst.tests.Controllers;

public class EventsControllerIntegrationTests : ControllerIntegrationTest
{
    private GamesController.GameModel _game;

    public override void Setup()
    {
        base.Setup();

        _game = Post<GamesController.GameModel>("/api/games", new GamesController.CreateGameModel("Test game"), HttpStatusCode.Created).Result!;
    }

    [Test]
    public async Task AddEvent_ReturnsExpectedResult()
    {
        await AddEvent(new TestEvent(Guid.Empty, new TestEventBody {Value = "Hello, World!"}));

        await Post<EventsController.EventModel>(
            $"/api/games/{_game.Id}/events",
            new EventsController.CreateEventModel($"InvalidEvent_{Guid.NewGuid()}",
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
        HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task EventsChangeState()
    {
        var state = await GetState<JamClockState>();

        state.IsRunning.Should().BeFalse();

        await AddEvent(new JamStarted(Guid.Empty));

        state = await GetState<JamClockState>();

        state.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task GetEvents_ReturnsGameEvents()
    {
        const int testEventCount = 10;

        var testEvents = new List<TestEvent>();

        for (var i = 0; i < testEventCount; i++)
        {
            var body = new TestEventBody {Value = i.ToString()};
            var createResult = await AddEvent(new TestEvent(Guid.Empty, body));

            testEvents.Add(new TestEvent(createResult!.Id, body));
        }

        var events = await GetEvents();
        var typedEvents = events
            .Select(e => e.AsUntypedEvent().AsEvent(typeof(TestEvent)))
            .Select(r => r switch
            {
                Success<Event> s => s.Value,
                _ => throw new AssertionException("Event conversion failed")
            })
            .ToArray();

        typedEvents.Should().BeEquivalentTo(testEvents);
    }

    [Test]
    public async Task DeleteEvent_UpdateStateAsExpected()
    {
        var scoreEvent = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 1)));
        _ = await AddEvent(new JamStarted(Guid.Empty));
        _ = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 2)));
        var timeoutStartedEvent = await AddEvent(new TimeoutStarted(Guid.Empty));
        _ = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 3)));

        var gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Timeout);

        var homeTeamScore = await GetState<TeamScoreState>(TeamSide.Home);
        homeTeamScore.Score.Should().Be(6);

        await DeleteEvent(timeoutStartedEvent.Id);

        gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Jam);

        homeTeamScore = await GetState<TeamScoreState>(TeamSide.Home);
        homeTeamScore.Score.Should().Be(6);

        await DeleteEvent(scoreEvent.Id);

        gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Jam);

        homeTeamScore = await GetState<TeamScoreState>(TeamSide.Home);
        homeTeamScore.Score.Should().Be(5);
    }

    private async Task<EventsController.EventModel[]> GetEvents() =>
        (await Get<EventsController.EventModel[]>($"/api/games/{_game.Id}/events", HttpStatusCode.OK))!;

    private async Task<EventsController.EventModel> AddEvent<TEvent>(TEvent @event) where TEvent : Event =>
        (await Post<EventsController.EventModel>(
            $"/api/games/{_game.Id}/events",
            new EventsController.CreateEventModel(
                @event.GetType().Name,
                @event.GetBodyObject()?.Map(body => JsonObject.Create(JsonSerializer.SerializeToElement(body)))),
            HttpStatusCode.Accepted))!;

    private Task DeleteEvent(Guid eventId) =>
        Delete($"/api/games/{_game.Id}/events/{eventId}", HttpStatusCode.NoContent);

    private async Task<TState> GetState<TState>() =>
        (await Get<TState>($"/api/games/{_game.Id}/state/{typeof(TState).Name}", HttpStatusCode.OK))!;

    private async Task<TState> GetState<TState>(TeamSide side) =>
        (await Get<TState>($"/api/games/{_game.Id}/state/{typeof(TState).Name}_{side}", HttpStatusCode.OK))!;

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
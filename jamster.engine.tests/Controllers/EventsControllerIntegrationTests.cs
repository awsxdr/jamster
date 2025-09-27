using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using FluentAssertions;
using Func;

using jamster.engine.Controllers;
using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Hubs;
using jamster.engine.Reducers;
using jamster.engine.Services;

using Microsoft.AspNetCore.SignalR.Client;

namespace jamster.engine.tests.Controllers;

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
            new EventsController.CreateEventModel(
                $"InvalidEvent_{Guid.NewGuid()}",
                JsonObject.Create(JsonSerializer.SerializeToElement(new TestEventBody { Value = "Hello, World!" }))),
            HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AddEvent_WhenGameNotFound_ReturnsNotFound() =>
        await AddEvent(new TestEvent(Guid.Empty, new TestEventBody {Value = "Hello, World!"}), gameId: Guid.NewGuid(), expectedResult: HttpStatusCode.NotFound);

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
    public async Task GetEvents_WhenGameFound_ReturnsGameEvents()
    {
        const int testEventCount = 10;

        var testEvents = new List<TestEvent>();

        for (var i = 0; i < testEventCount; ++i)
        {
            var body = new TestEventBody {Value = i.ToString()};
            var createResult = await AddEvent(new TestEvent(Guid.Empty, body));

            testEvents.Add(new TestEvent(createResult.Id, body));
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
    public async Task GetEvents_WhenGameNotFound_ReturnsNotFound()
    {
        var result = await Get($"/api/games/{Guid.NewGuid()}/events");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetUndoEvents_WhenGameFound_ReturnsGameUndoEvents()
    {
        const int testEventCount = 10;
        const int undoEventSpacing = 3;

        var testEvents = new List<Event>();

        for (var i = 0; i < testEventCount; ++i)
        {
            if (i % undoEventSpacing == 0)
            {
                var createResult = await AddEvent(new TestUndoEvent(Guid.Empty));

                testEvents.Add(new TestUndoEvent(createResult.Id));
            }
            else
            {
                var body = new TestEventBody();
                var createResult = await AddEvent(new TestEvent(Guid.Empty, body));

                testEvents.Add(new TestEvent(createResult.Id, body));
            }
        }

        var events = await GetUndoEvents();
        var typedEvents = events
            .Select(e => e.AsUntypedEvent().AsEvent(typeof(TestUndoEvent)))
            .Select(r => r switch
            {
                Success<Event> s => s.Value,
                _ => throw new AssertionException("Event conversion failed")
            })
            .ToArray();

        typedEvents.Should().BeEquivalentTo(testEvents.OfType<TestUndoEvent>());
    }

    [Test]
    public async Task GetUndoEvents_ObeysSortOrder([Values] bool descending)
    {
        const int testEventCount = 10;

        for (var i = 0; i < testEventCount; ++i)
        {
            await AddEvent(new TestUndoEvent(Guid.Empty));
        }

        var events = await GetUndoEvents(query: $"sortOrder={(descending ? "Desc" : "Asc")}");
        var typedEvents = events
            .Select(e => e.AsUntypedEvent().AsEvent(typeof(TestUndoEvent)))
            .Select(r => r switch
            {
                Success<Event> s => s.Value,
                _ => throw new AssertionException("Event conversion failed")
            })
            .ToArray();

        if (descending)
            typedEvents.Should().BeInDescendingOrder(x => x.Id);
        else
            typedEvents.Should().BeInAscendingOrder(x => x.Id);
    }

    [TestCase(5, 5)]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(10, 10)]
    [TestCase(11, 10)]
    [TestCase(15, 10)]
    [TestCase(-1, 0)]
    public async Task GetUndoEvents_ObeysMaxCount(int maxCount, int expectedCount)
    {
        const int testEventCount = 10;

        for (var i = 0; i < testEventCount; ++i)
        {
            await AddEvent(new TestUndoEvent(Guid.Empty));
        }

        var events = await GetUndoEvents(query: $"maxCount={maxCount}");
        var typedEvents = events
            .Select(e => e.AsUntypedEvent().AsEvent(typeof(TestUndoEvent)))
            .Select(r => r switch
            {
                Success<Event> s => s.Value,
                _ => throw new AssertionException("Event conversion failed")
            })
            .ToArray();

        typedEvents.Should().HaveCount(expectedCount);
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

    [Test]
    public async Task DeleteEvent_NotifiesStateWatchers()
    {
        _ = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 1)));
        _ = await AddEvent(new JamStarted(Guid.Empty));
        _ = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 2)));
        var scoreEvent = await AddEvent(new ScoreModifiedRelative(Guid.Empty, new(TeamSide.Home, 3)));

        var homeTeamScore = await GetState<TeamScoreState>(TeamSide.Home);
        homeTeamScore.Score.Should().Be(6);

        var hub = await GetHubConnection($"api/Hubs/Game/{_game.Id}");
        await hub.InvokeAsync(nameof(GameStatesHub.WatchState), $"{nameof(TeamScoreState)}_{nameof(TeamSide.Home)}");

        var taskCompletionSource = new TaskCompletionSource<TeamScoreState>();

        hub.On("StateChanged", (string _, TeamScoreState state) =>
        {
            taskCompletionSource.SetResult(state);
        });

        await DeleteEvent(scoreEvent.Id);

        var passedState = await Wait(taskCompletionSource.Task);

        passedState.Should().NotBeNull();
        passedState.Score.Should().Be(3);
    }

    [Test]
    public async Task DeleteEvent_WhenEventNotFound_ReturnsNotFound() =>
        await DeleteEvent(Guid.NewGuid(), expectedResult: HttpStatusCode.NotFound);
    
    [Test]
    public async Task DeleteEvent_WhenGameNotFound_ReturnsNotFound()
    {
        var @event = await AddEvent(new JamStarted(Guid.Empty));
        await DeleteEvent(@event.Id, gameId: Guid.NewGuid(), expectedResult: HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteEvent_WhenDeletingAutomaticJamEnd_CorrectlyResumesJam()
    {
        _ = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamEnded(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamEnded(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamEnded(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamEnded(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamStarted(0));
        Thread.Sleep(100);

        var scoreSheetDuringJam = await GetState<ScoreSheetState>(TeamSide.Home);

        Tick += Tick.FromSeconds(Rules.DefaultRules.JamRules.DurationInSeconds + 10);
        Thread.Sleep(100);

        var gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Lineup);

        var undoEvents = await GetUndoEvents();

        await DeleteEvent(undoEvents.First().Id);
        Thread.Sleep(100);

        var scoreSheetAfterUndo = await GetState<ScoreSheetState>(TeamSide.Home);

        scoreSheetDuringJam.Jams.Length.Should().Be(scoreSheetAfterUndo.Jams.Length);

        gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Jam);
        gameStage.JamNumber.Should().Be(5);
    }

    [Test]
    public async Task ReplaceEvent_UpdatesStateAsExpected()
    {
        Tick += Tick.FromSeconds(10);
        _ = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(30);
        _ = await AddEvent(new JamEnded(0));
        Tick += Tick.FromSeconds(30);

        var gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Lineup);

        var @event = await AddEvent(new JamStarted(0));
        Tick += Tick.FromSeconds(5);

        gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Jam);

        await ReplaceEvent(@event.Id, new TimeoutStarted(0));

        gameStage = await GetState<GameStageState>();
        gameStage.Stage.Should().Be(Stage.Timeout);
    }

    [Test]
    public async Task ReplaceEvent_WhenEventNotFound_ReturnsNotFound() =>
        await ReplaceEvent(Guid.NewGuid(), new TestEvent(0, new()), expectedResult: HttpStatusCode.NotFound);

    [Test]
    public async Task ReplaceEvent_WhenGameNotFound_ReturnsNotFound()
    {
        var @event = await AddEvent(new JamStarted(0));
        await ReplaceEvent(@event.Id, new JamStarted(0), gameId: Guid.NewGuid(), expectedResult: HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetEventTick_WhenChangingSingleEvent_SetsEventTick()
    {
        _ = await AddEvent(new JamStarted(0));
        Tick += 30_000;
        var endEvent = await AddEvent(new JamEnded(0));
        Tick += 25_000;
        var timeoutEvent = await AddEvent(new TimeoutStarted(0));

        var endEventDetails = await GetEvent(endEvent.Id);
        var timeoutEventDetails = await GetEvent(timeoutEvent.Id);

        ((Guid7)endEventDetails.Id).Tick.Should().Be(30_000);
        ((Guid7)timeoutEventDetails.Id).Tick.Should().Be(55_000);

        var newEndEvent = await SetEventTick(endEvent.Id, 35_000, false);
        ((Guid7)newEndEvent.NewEvent.Id).Tick.Should().Be(35_000);

        endEventDetails = await GetEvent(newEndEvent.NewEvent.Id);
        timeoutEventDetails = await GetEvent(timeoutEvent.Id);

        ((Guid7)endEventDetails.Id).Tick.Should().Be(35_000);
        ((Guid7)timeoutEventDetails.Id).Tick.Should().Be(55_000);
    }

    [Test]
    public async Task SetEventTick_WhenChangingAllSubsequentEvents_SetsAllEventTicksAsExpected()
    {
        _ = await AddEvent(new JamStarted(0));
        Tick += 30_000;
        var endEvent = await AddEvent(new JamEnded(0));
        Tick += 25_000;
        var timeoutEvent = await AddEvent(new TimeoutStarted(0));

        var endEventDetails = await GetEvent(endEvent.Id);
        var timeoutEventDetails = await GetEvent(timeoutEvent.Id);

        ((Guid7)endEventDetails.Id).Tick.Should().Be(30_000);
        ((Guid7)timeoutEventDetails.Id).Tick.Should().Be(55_000);

        var setEventResult = await SetEventTick(endEvent.Id, 35_000, true);
        setEventResult.OtherChangedEvents.Length.Should().Be(1);

        var newEndEvent = setEventResult.NewEvent;
        var newTimeoutEvent = setEventResult.OtherChangedEvents[0];

        ((Guid7)newEndEvent.Id).Tick.Should().Be(35_000);

        endEventDetails = await GetEvent(newEndEvent.Id);
        timeoutEventDetails = await GetEvent(newTimeoutEvent.Id);

        ((Guid7)endEventDetails.Id).Tick.Should().Be(35_000);
        ((Guid7)timeoutEventDetails.Id).Tick.Should().Be(60_000);
    }

    private async Task<EventsController.EventModel> GetEvent(Guid eventId, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.OK) =>
        (await Get<EventsController.EventModel>($"/api/games/{gameId ?? _game.Id}/events/{eventId}", expectedResult))!;

    private async Task<EventsController.EventModel[]> GetEvents(Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.OK) =>
        (await Get<EventsController.EventModel[]>($"/api/games/{gameId ?? _game.Id}/events", expectedResult))!;

    private async Task<EventsController.EventModel[]> GetUndoEvents(Guid? gameId = null, string? query = null, HttpStatusCode expectedResult = HttpStatusCode.OK) =>
        (await Get<EventsController.EventModel[]>($"/api/games/{gameId ?? _game.Id}/events/undo?{query ?? string.Empty}", expectedResult))!;

    private async Task<EventsController.EventModel> AddEvent<TEvent>(TEvent @event, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.Accepted) where TEvent : Event =>
        (await Post<EventsController.EventModel>(
            $"/api/games/{gameId ?? _game.Id}/events",
            new EventsController.CreateEventModel(
                @event.GetType().Name,
                @event.GetBodyObject()?.Map(body => JsonObject.Create(JsonSerializer.SerializeToElement(body)))),
            expectedResult))!;

    private Task DeleteEvent(Guid eventId, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.NoContent) =>
        Delete($"/api/games/{gameId ?? _game.Id}/events/{eventId}", expectedResult);

    private Task ReplaceEvent<TEvent>(Guid eventId, TEvent newEvent, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.OK) where TEvent : Event =>
        Put<EventsController.EventModel>(
            $"/api/games/{gameId ?? _game.Id}/events/{eventId}",
            new EventsController.CreateEventModel(
                newEvent.GetType().Name,
                newEvent.GetBodyObject()?.Map(body => JsonObject.Create(JsonSerializer.SerializeToElement(body)))),
            expectedResult);

    private Task<EventsController.EventTickSetModel> SetEventTick(Guid eventId, Tick newTick, bool offsetFollowing, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.OK) =>
        Put<EventsController.EventTickSetModel>(
            $"/api/games/{gameId ?? _game.Id}/events/{eventId}/tick",
            new EventsController.SetEventTickModel(newTick, offsetFollowing),
            expectedResult)!;

    private async Task<TState> GetState<TState>() =>
        (await Get<TState>($"/api/games/{_game.Id}/state/{typeof(TState).Name}", HttpStatusCode.OK))!;

    private async Task<TState> GetState<TState>(TeamSide side) =>
        (await Get<TState>($"/api/games/{_game.Id}/state/{typeof(TState).Name}_{side}", HttpStatusCode.OK))!;

    protected override void CleanDatabase()
    {
        GameDataStoreFactory?.ReleaseConnections().Wait();
        GC.Collect(); // Force SQLite to release database files

        foreach (var databaseFile in Directory.GetFiles(GameDataStore.GamesFolder, "*.db"))
        {
            File.Delete(databaseFile);
        }
    }
}
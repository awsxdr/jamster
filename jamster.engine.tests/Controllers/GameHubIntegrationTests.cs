﻿using System.Net;
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

using Microsoft.AspNetCore.SignalR.Client;

namespace jamster.engine.tests.Controllers;

public class GameHubIntegrationTests : ControllerIntegrationTest
{
    private GamesController.GameModel _game;

    public override void Setup()
    {
        base.Setup();

        _game = Post<GamesController.GameModel>("/api/games", new GamesController.CreateGameModel("Test game"), HttpStatusCode.Created).Result!;
    }

    protected override void CleanDatabase()
    {
        GameDataStoreFactory?.ReleaseConnections().Wait();
        GC.Collect(); // Force SQLite to release database files

        foreach (var databaseFile in Directory.GetFiles(GameDataStore.GamesFolder, "*.db"))
        {
            File.Delete(databaseFile);
        }
    }

    [Test]
    public async Task StateChange_NotifiesWatchingClients()
    {
        var secondGame = (await Post<GamesController.GameModel>("/api/games", new GamesController.CreateGameModel("Test game 2"), HttpStatusCode.Created))!;

        var gameOneHub = await GetHubConnection($"api/hubs/game/{_game.Id}");
        var gameTwoHub = await GetHubConnection($"api/hubs/game/{secondGame.Id}");

        await gameOneHub.InvokeAsync(nameof(GameStatesHub.WatchState), $"{nameof(TeamScoreState)}_{nameof(TeamSide.Home)}");
        await gameTwoHub.InvokeAsync(nameof(GameStatesHub.WatchState), $"{nameof(TeamScoreState)}_{nameof(TeamSide.Home)}");

        var gameOneCompletionSource = new TaskCompletionSource();
        gameOneHub.On("StateChanged", (string _, TeamScoreState _) =>
        {
            gameOneCompletionSource.SetResult();
        });

        var gameTwoCompletionSource = new TaskCompletionSource();
        gameTwoHub.On("StateChanged", (string _, TeamScoreState _) =>
        {
            gameTwoCompletionSource.SetResult();
        });

        await AddEvent(new ScoreModifiedRelative(0, new ScoreModifiedRelativeBody(TeamSide.Home, 4)));

        await Wait(gameOneCompletionSource.Task);

        var secondWait = () => Wait(gameTwoCompletionSource.Task, TimeSpan.FromMilliseconds(500)).Wait();

        secondWait.Should().Throw<TimeoutException>();
    }

    private async Task<EventsController.EventModel> AddEvent<TEvent>(TEvent @event, Guid? gameId = null, HttpStatusCode expectedResult = HttpStatusCode.Accepted) where TEvent : Event =>
        (await Post<EventsController.EventModel>(
            $"/api/games/{gameId ?? _game.Id}/events",
            new EventsController.CreateEventModel(
                @event.GetType().Name,
                @event.GetBodyObject()?.Map(body => JsonObject.Create(JsonSerializer.SerializeToElement(body)))),
            expectedResult))!;
}
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Configurations;
using amethyst.Controllers;
using amethyst.DataStores;
using amethyst.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SQLite;

namespace amethyst.tests.Controllers;

public class ConfigurationsIntegrationTests : ControllerIntegrationTest
{
    private GamesController.GameModel _game;

    public override void Setup()
    {
        base.Setup();

        _game = Post<GamesController.GameModel>("/api/games", new GamesController.CreateGameModel("Test game"), HttpStatusCode.Created).Result!;
    }

    [Test]
    public async Task GetConfiguration_WhenNotInDatabase_ReturnsDefaultConfiguration()
    {
        var expectedConfiguration = new DisplayConfigurationFactory().GetDefaultValue();

        var configuration = await Get<DisplayConfiguration>("api/Configurations/DisplayConfiguration", HttpStatusCode.OK);

        configuration.Should().Be(expectedConfiguration);
    }

    [Test]
    public async Task GetConfiguration_WhenPreviouslySet_ReturnsSetConfiguration()
    {
        var expectedConfiguration = new DisplayConfiguration(false, true, "test");

        await Put("api/Configurations/DisplayConfiguration", expectedConfiguration, HttpStatusCode.OK);

        var configuration = await Get<DisplayConfiguration>("api/Configurations/DisplayConfiguration", HttpStatusCode.OK);

        configuration.Should().Be(expectedConfiguration);
    }

    [Test]
    public async Task GetConfiguration_WithGameId_WhenNotSetInGame_AndNotInDatabase_ReturnsDefaultConfiguration()
    {
        var expectedConfiguration = new DisplayConfigurationFactory().GetDefaultValue();

        var configuration = await Get<DisplayConfiguration>($"api/Configurations/DisplayConfiguration?gameId={_game.Id}", HttpStatusCode.OK);

        configuration.Should().Be(expectedConfiguration);
    }

    [Test]
    public async Task GetConfiguration_WithGameId_WhenNotSetInGame_AndSetInDatabase_ReturnsDatabaseConfiguration()
    {
        var expectedConfiguration = new DisplayConfiguration(false, true, "test");

        await Put("api/Configurations/DisplayConfiguration", expectedConfiguration, HttpStatusCode.OK);

        var configuration = await Get<DisplayConfiguration>($"api/Configurations/DisplayConfiguration?gameId={_game.Id}", HttpStatusCode.OK);

        configuration.Should().Be(expectedConfiguration);
    }

    [Test]
    public async Task GetConfiguration_WithGameId_WhenSetInGame_ReturnsGameConfiguration()
    {
        var expectedConfiguration = new DisplayConfiguration(false, true, "expected");
        var databaseConfiguration = new DisplayConfiguration(true, false, "test");

        await Put("api/Configurations/DisplayConfiguration", databaseConfiguration, HttpStatusCode.OK);
        await Put($"api/Configurations/DisplayConfiguration?gameId={_game.Id}", expectedConfiguration, HttpStatusCode.OK);

        var configuration = await Get<DisplayConfiguration>($"api/Configurations/DisplayConfiguration?gameId={_game.Id}", HttpStatusCode.OK);

        configuration.Should().Be(expectedConfiguration);
    }

    [Test]
    public async Task SetConfiguration_NotifiesConfigurationWatchers()
    {
        var expectedConfiguration = new DisplayConfiguration(false, true, "test");

        var hub = await GetHubConnection("api/Hubs/Configuration");
        await hub.InvokeAsync(nameof(ConfigurationHub.WatchConfiguration), nameof(DisplayConfiguration));

        var taskCompletionSource = new TaskCompletionSource<DisplayConfiguration>();

        hub.On("ConfigurationChanged", (string key, JsonObject value) =>
        {
            if (key == nameof(DisplayConfiguration))
            {
                var deserializedValue = value.Deserialize<DisplayConfiguration>(Program.JsonSerializerOptions)!;
                taskCompletionSource.SetResult(deserializedValue);
            }
        });

        await Put("api/Configurations/DisplayConfiguration", expectedConfiguration, HttpStatusCode.OK);

        var passedConfiguration = await Wait(taskCompletionSource.Task);

        passedConfiguration.Should().Be(expectedConfiguration);
    }

    protected override void CleanDatabase()
    {
        using var connection = new SQLiteConnection(Path.Combine(RunningEnvironment.RootPath, "db", "configurations.db"));

        connection.Execute("DELETE FROM configurationDataItem");

        GameDataStoreFactory?.ReleaseConnections().Wait();
        GC.Collect(); // Force SQLite to release database files

        foreach (var databaseFile in Directory.GetFiles(GameDataStore.GamesFolder, "*.db"))
        {
            File.Delete(databaseFile);
        }
    }
}
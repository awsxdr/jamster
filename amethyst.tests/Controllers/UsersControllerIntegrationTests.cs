using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using amethyst.Configurations;
using amethyst.Controllers;
using amethyst.DataStores;
using amethyst.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SQLite;

namespace amethyst.tests.Controllers;

public class UsersControllerIntegrationTests : ControllerIntegrationTest
{
    [Test]
    public async Task GetUsers_ReturnsFullUserList()
    {
        string[] expectedUsers = ["user1", "user2", "user3"];

        foreach (var user in expectedUsers)
        {
            await Post("api/users", new UserModel(user), HttpStatusCode.Created);
        }

        var users = await Get<UserModel[]>("api/users", HttpStatusCode.OK);

        users!.Select(u => u.UserName).Should().BeEquivalentTo(expectedUsers);
    }

    [Test]
    public async Task GetUser_ReturnsAllConfiguredSettings()
    {
        var configuration1 = new TestConfiguration1 { Value = "Test" };
        var configuration2 = new TestConfiguration2 { Value = 1234 };

        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        await Put(
            "api/users/testUser/configuration/testConfiguration1",
            configuration1,
            HttpStatusCode.OK);

        await Put(
            "api/users/testUser/configuration/testConfiguration2",
            configuration2,
            HttpStatusCode.OK);

        var user = (await Get<UserConfigurationsModel>("api/users/testUser", HttpStatusCode.OK))!;

        user.UserName.Should().Be("testuser");
        user.Configurations.Should().ContainKey(nameof(TestConfiguration1))
            .WhoseValue.Should().BeOfType<JsonElement>()
            .Which.Deserialize<TestConfiguration1>(Program.JsonSerializerOptions).Should().BeEquivalentTo(configuration1);
        user.Configurations.Should().ContainKey(nameof(TestConfiguration2))
            .WhoseValue.Should().BeOfType<JsonElement>()
            .Which.Deserialize<TestConfiguration2>(Program.JsonSerializerOptions).Should().BeEquivalentTo(configuration2);
    }

    [Test]
    public async Task SetUserConfiguration_OverwritesExistingConfiguration()
    {
        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        await Put(
            "api/users/testUser/configuration/testConfiguration2",
            new TestConfiguration2 { Value = 1 },
            HttpStatusCode.OK);

        var user1 = (await Get<UserConfigurationsModel>("api/users/testUser", HttpStatusCode.OK))!;

        user1.Configurations.Should().ContainKey(nameof(TestConfiguration2))
            .WhoseValue.Should().BeOfType<JsonElement>()
            .Which.Deserialize<TestConfiguration2>(Program.JsonSerializerOptions)!.Value.Should().Be(1);

        await Put(
            "api/users/testUser/configuration/testConfiguration2",
            new TestConfiguration2 { Value = 2 },
            HttpStatusCode.OK);

        var user2 = (await Get<UserConfigurationsModel>("api/users/testUser", HttpStatusCode.OK))!;

        user2.Configurations.Should().ContainKey(nameof(TestConfiguration2))
            .WhoseValue.Should().BeOfType<JsonElement>()
            .Which.Deserialize<TestConfiguration2>(Program.JsonSerializerOptions)!.Value.Should().Be(2);
    }

    [Test]
    public async Task GetUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await Get<UserConfigurationsModel>("api/users/doesNotExist", HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUser_WhenUserExists_RemovesUserFromList()
    {
        for (var i = 0; i < 3; ++i)
        {
            await Post("api/users", new UserModel($"testUser{i + 1}"), HttpStatusCode.Created);
        }

        var users1 = (await Get<UserModel[]>("api/users", HttpStatusCode.OK))!;

        users1.Length.Should().Be(3);

        await Delete("api/users/testUser2", HttpStatusCode.NoContent);

        var users2 = (await Get<UserModel[]>("api/users", HttpStatusCode.OK))!;

        users2.Length.Should().Be(2);
    }

    [Test]
    public async Task DeleteUser_WhenUserExists_RemovesUserConfigurations()
    {
        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        await Put(
            "api/users/testUser/configuration/testConfiguration1",
            new TestConfiguration1 { Value = "Test" },
            HttpStatusCode.OK);

        await Put(
            "api/users/testUser/configuration/testConfiguration2",
            new TestConfiguration2 { Value = 123 },
            HttpStatusCode.OK);

        using var connection = new SQLiteConnection(Path.Combine(RunningEnvironment.RootPath, "db", "users.db"));

        Column<string, UserConfiguration>[] columns =
        [
            new ("userName", u => u.UserName),
            new ("configurationType", u => u.ConfigurationType),
        ];

        var dataTable = new DataTableFactory().Create<UserConfiguration, string>(c => $"{c.UserName}_{c.ConfigurationType}", connection, columns);

        dataTable.GetByColumn(columns[0], "testuser").Should().HaveCount(2);

        await Delete("api/users/testUser", HttpStatusCode.NoContent);

        dataTable.GetByColumn(columns[0], "testuser").Should().HaveCount(0);
    }

    [Test]
    public async Task DeleteUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await Delete("api/users/doesNotExist", HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUser_NotifiesUserListWatchers()
    {
        for (var i = 0; i < 3; ++i)
        {
            await Post("api/users", new UserModel($"testUser{i + 1}"), HttpStatusCode.Created);
        }

        var hub = await GetHubConnection("api/Hubs/Users");
        await hub.InvokeAsync(nameof(UsersHub.WatchUserList));

        var taskCompletionSource = new TaskCompletionSource<string[]>();

        hub.On(nameof(IUsersHubClient.UserListChanged), (string[] users) =>
        {
            taskCompletionSource.SetResult(users);
        });

        await Delete("api/users/testUser2", HttpStatusCode.NoContent);

        var users = await Wait(taskCompletionSource.Task);

        users.Should().BeEquivalentTo(["testuser1", "testuser3"]);
    }

    [Test]
    public async Task SetUserConfiguration_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await Put(
            "api/users/doesNotExist/configuration/testConfiguration1",
            new TestConfiguration1(),
            HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetUserConfiguration_WhenConfigurationTypeNotKnown_ReturnsBadRequest()
    {
        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        await Put(
            "api/users/testUser/configuration/badConfiguration",
            new object(),
            HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetConfiguration_WhenConfigurationNotSet_ReturnsDefaultConfiguration()
    {
        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        var configuration = (await Get<TestConfiguration1>("api/users/testUser/configuration/testConfiguration1", HttpStatusCode.OK))!;

        configuration.Value.Should().Be("Default");
    }

    [Test]
    public async Task GetConfiguration_WhenConfigurationSet_ReturnsConfiguredValue()
    {
        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        await Put(
            "api/users/testUser/configuration/testConfiguration1",
            new TestConfiguration1 { Value = "Test" },
            HttpStatusCode.OK);

        var configuration = (await Get<TestConfiguration1>("api/users/testUser/configuration/testConfiguration1", HttpStatusCode.OK))!;
        configuration.Value.Should().Be("Test");
    }

    [Test]
    public async Task SetConfiguration_NotifiesConfigurationWatchers()
    {
        var expectedConfiguration = new TestConfiguration1 { Value = "Test" };

        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        var hub = await GetHubConnection("api/Hubs/Users");
        await hub.InvokeAsync(nameof(UsersHub.WatchUserConfiguration), "testUser", nameof(TestConfiguration1));

        var taskCompletionSource = new TaskCompletionSource<TestConfiguration1>();

        hub.On(nameof(IUsersHubClient.UserConfigurationChanged), (string userName, string key, JsonObject value) =>
        {
            if (key == nameof(TestConfiguration1))
            {
                var deserializedValue = value.Deserialize<TestConfiguration1>(Program.JsonSerializerOptions)!;
                taskCompletionSource.SetResult(deserializedValue);
            }
        });

        await Put(
            "api/users/testUser/configuration/testConfiguration1",
            expectedConfiguration,
            HttpStatusCode.OK);

        var passedConfiguration = await Wait(taskCompletionSource.Task);

        passedConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

    [Test]
    public async Task AddUser_NotifiesUserListWatchers()
    {
        var hub = await GetHubConnection("api/Hubs/Users");
        await hub.InvokeAsync(nameof(UsersHub.WatchUserList));

        var taskCompletionSource = new TaskCompletionSource<string[]>();

        hub.On(nameof(IUsersHubClient.UserListChanged), (string[] users) =>
        {
            taskCompletionSource.SetResult(users);
        });

        await Post("api/users", new UserModel("testUser"), HttpStatusCode.Created);

        var users = await Wait(taskCompletionSource.Task);

        users.Should().BeEquivalentTo(["testuser"]);
    }

    protected override void CleanDatabase()
    {
        using var connection = new SQLiteConnection(Path.Combine(RunningEnvironment.RootPath, "db", "users.db"));

        connection.Execute("DELETE FROM user");
        connection.Execute("DELETE FROM userConfiguration");
    }

    private class TestConfiguration1
    {
        public string Value { get; set; }
    }

    private class TestConfiguration2
    {
        public int Value { get; set; }
    }

    private class TestConfiguration1Factory : IConfigurationFactory<TestConfiguration1>
    {
        public TestConfiguration1 GetDefaultValue() => new() { Value = "Default" };
    }

    private class TestConfiguration2Factory : IConfigurationFactory<TestConfiguration2>
    {
        public TestConfiguration2 GetDefaultValue() => new();
    }
}
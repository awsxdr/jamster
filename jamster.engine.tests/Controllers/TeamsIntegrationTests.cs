using System.Net;
using System.Net.Http.Json;
using jamster.Controllers;
using jamster.Domain;
using jamster.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SQLite;

namespace jamster.engine.tests.Controllers;

public class TeamsIntegrationTests : ControllerIntegrationTest
{
    [Test]
    public async Task GetTeamReturnsEmptyArray()
    {
        var response = await Get<TeamModel[]>("api/Teams", HttpStatusCode.OK);

        response.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public async Task CreatedTeamReturnedCorrectly()
    {
        var team = new CreateTeamModel(
            new()
            {
                ["default"] = "Test team",
            },
            new()
            {
                ["White"] = new(Color.White, Color.Black)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;
        createResponse.Id.Should().NotBeEmpty();

        var expectedTeam = createResponse with { Names = team.Names, Colors = team.Colors };

        createResponse.Should().Be(expectedTeam);
    }

    [Test]
    public async Task Update_ChangesTeamCorrectly()
    {
        var team = new CreateTeamModel(
            new() { ["default"] = "Test team", },
            new() { ["White"] = new(Color.White, Color.Black) });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var roster = new RosterModel([
            new("123", "Test Skater 1"),
            new("321", "Test Skater 2"),
        ]);

        await Put($"/api/Teams/{createResponse.Id}/roster", roster, HttpStatusCode.OK);

        var updateRequest = new UpdateTeamModel(
            new() { ["default"] = "Edited team", ["new"] = "New name" },
            new()
            {
                ["White"] = new(Color.White, Color.Black),
                ["Test"] = new(Color.FromRgb(1, 2, 3), Color.FromRgb(3, 2, 1)),
            });

        await Put($"api/Teams/{createResponse.Id}", updateRequest, HttpStatusCode.OK);

        var getResponse = (await Get<TeamModel>($"api/Teams/{createResponse.Id}", HttpStatusCode.OK))!;

        getResponse.Should().Be((TeamModel)(Team)updateRequest with { Id = createResponse.Id, LastUpdateTime = getResponse.LastUpdateTime });

        var getRosterResponse = (await Get<RosterModel>($"api/Teams/{createResponse.Id}/roster", HttpStatusCode.OK))!;

        getRosterResponse.Should().BeEquivalentTo(roster);
    }

    [Test]
    public async Task ArchivedTeamOnlyShowsWhenDesired()
    {
        var team = new CreateTeamModel(
            new()
            {
                ["default"] = "Test team",
            },
            new()
            {
                ["White"] = new(Color.White, Color.Black)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var expectedTeam = createResponse with { Names = team.Names, Colors = team.Colors };

        var getTeamsResponse = (await Get<TeamModel[]>("api/Teams", HttpStatusCode.OK))!;
        getTeamsResponse.Should().NotBeEmpty().And.ContainEquivalentOf(expectedTeam);

        var expectedTeamWithRoster = new TeamWithRosterModel(
            expectedTeam.Id,
            expectedTeam.Names,
            expectedTeam.Colors,
            [],
            expectedTeam.LastUpdateTime);

        var getTeamResponse = (await Get<TeamWithRosterModel>($"api/Teams/{createResponse.Id}", HttpStatusCode.OK))!;
        getTeamResponse.Should().BeEquivalentTo(expectedTeamWithRoster);

        await Delete($"api/Teams/{createResponse.Id}", HttpStatusCode.NoContent);

        await Get<TeamModel>($"api/Teams/{createResponse.Id}", HttpStatusCode.NotFound);

        var getTeamIncludingArchivedResponse = (await Get<TeamWithRosterModel>($"api/Teams/{createResponse.Id}?includeArchived=true", HttpStatusCode.OK))!;
        getTeamIncludingArchivedResponse.Should().BeEquivalentTo(expectedTeamWithRoster);
    }

    [Test]
    public async Task EndpointsReturnNotFoundWhenTeamDoesNotExist()
    {
        (await Get($"/api/Teams/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Get($"/api/Teams/{Guid.NewGuid()}?includeArchived=true")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Get($"/api/Teams/{Guid.NewGuid()}?includeArchived=false")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Delete($"/api/Teams/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Get($"/api/Teams/{Guid.NewGuid()}/roster")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Put($"/api/Teams/{Guid.NewGuid()}/roster", JsonContent.Create(new RosterModel([])))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SettingRosterUpdatesCurrentValue()
    {
        var team = new CreateTeamModel(
            new()
            {
                ["default"] = "Test team",
            },
            new()
            {
                ["White"] = new(Color.White, Color.Black)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var roster = new RosterModel([
            new Skater("1", "One"),
            new Skater("2", "Two"),
            new Skater("3", "Three"),
            new Skater("4", "Four"),
        ]);

        var rosterPath = $"api/Teams/{createResponse.Id}/roster";
        var originalRoster = await Get<RosterModel>(rosterPath, HttpStatusCode.OK);

        originalRoster.Should().NotBeNull().And.BeEquivalentTo(new RosterModel([]));

        await Put(rosterPath, roster, HttpStatusCode.OK);

        var newRoster = await Get<RosterModel>(rosterPath, HttpStatusCode.OK);
        newRoster.Should().BeEquivalentTo(roster);
    }

    [Test]
    public async Task CreateTeam_NotifiesWatchingClients()
    {
        var connection = await GetHubConnection("api/Hubs/Teams");

        await connection.InvokeAsync(nameof(TeamsHub.WatchTeamCreated));

        var taskCompletionSource = new TaskCompletionSource<TeamModel>();

        connection.On(nameof(ITeamsHubClient.TeamCreated), (TeamModel team) =>
        {
            taskCompletionSource.SetResult(team);
        });

        var team = new CreateTeamModel(
            new() { ["default"] = "Test team", },
            new() { ["White"] = new(Color.White, Color.Black) });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var notificationTeam = await Wait(taskCompletionSource.Task);

        notificationTeam.Should().Be(createResponse);
    }

    [Test]
    public async Task UpdateTeam_NotifiesWatchingClients()
    {
        var connection = await GetHubConnection("api/Hubs/Teams");

        await connection.InvokeAsync(nameof(TeamsHub.WatchTeamChanged));

        var taskCompletionSource = new TaskCompletionSource<TeamModel>();

        connection.On(nameof(ITeamsHubClient.TeamChanged), (TeamModel team) =>
        {
            taskCompletionSource.SetResult(team);
        });

        var team = new CreateTeamModel(
            new() { ["default"] = "Test team", },
            new() { ["White"] = new(Color.White, Color.Black) });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var updateRequest = new UpdateTeamModel(
            new() { ["default"] = "Edited team", ["new"] = "New name" },
            new()
            {
                ["White"] = new (Color.FromRgb(1, 2, 3), Color.FromRgb(3, 2, 1)),
                ["Black"] = new(Color.Black, Color.White),
            });

        await Put($"api/Teams/{createResponse.Id}", updateRequest, HttpStatusCode.OK);

        var notificationTeam = await Wait(taskCompletionSource.Task);

        notificationTeam.Should().Be((TeamModel)(Team)updateRequest with { Id = createResponse.Id, LastUpdateTime = notificationTeam.LastUpdateTime });
    }

    [Test]
    public async Task SetRoster_NotifiesWatchingClients()
    {
        var connection = await GetHubConnection("api/Hubs/Teams");

        var taskCompletionSource = new TaskCompletionSource<TeamWithRosterModel>();

        connection.On(nameof(ITeamsHubClient.TeamChanged), (TeamWithRosterModel team) =>
        {
            taskCompletionSource.SetResult(team);
        });

        await connection.InvokeAsync(nameof(TeamsHub.WatchTeamChanged));

        var team = new CreateTeamModel(
            new() { ["default"] = "Test team", },
            new() { ["White"] = new(Color.White, Color.Black) });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var roster = new RosterModel([
            new Skater("1", "One"),
            new Skater("2", "Two"),
            new Skater("3", "Three"),
            new Skater("4", "Four"),
        ]);

        await Put($"api/Teams/{createResponse.Id}/roster", roster, HttpStatusCode.OK);

        var notificationTeam = await Wait(taskCompletionSource.Task);

        notificationTeam.Roster.Should().BeEquivalentTo(roster.Roster);
    }

    [Test]
    public async Task ArchiveTeam_NotifiesWatchingClients()
    {
        var connection = await GetHubConnection("api/Hubs/Teams");

        var taskCompletionSource = new TaskCompletionSource<Guid>();

        connection.On(nameof(ITeamsHubClient.TeamArchived), (Guid teamId) =>
        {
            taskCompletionSource.SetResult(teamId);
        });

        await connection.InvokeAsync(nameof(TeamsHub.WatchTeamArchived));

        var team = new CreateTeamModel(
            new() { ["default"] = "Test team", },
            new() { ["White"] = new(Color.White, Color.Black) });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        await Delete($"api/Teams/{createResponse.Id}", HttpStatusCode.NoContent);

        var notificationTeamId = await Wait(taskCompletionSource.Task);

        notificationTeamId.Should().Be(createResponse.Id);
    }

    protected override void CleanDatabase()
    {
        using var connection = new SQLiteConnection(Path.Combine(RunningEnvironment.RootPath, "db", "teams.db"));

        connection.Execute("DELETE FROM team");
    }
}
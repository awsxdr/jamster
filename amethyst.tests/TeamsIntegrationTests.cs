namespace amethyst.tests;

using System.Net;
using System.Net.Http.Json;
using Controllers;
using DataStores;
using Domain;
using FluentAssertions;
using SQLite;

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
                ["default"] = new DisplayColor(Color.Black, Color.White)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;
        createResponse.Id.Should().NotBeEmpty();

        var expectedTeam = new TeamModel(
            createResponse.Id,
            createResponse.Names,
            createResponse.Colors);

        createResponse.Should().Be(expectedTeam);
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
                ["default"] = new DisplayColor(Color.Black, Color.White)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var expectedTeam = new TeamModel(
            createResponse.Id,
            createResponse.Names,
            createResponse.Colors);

        var getTeamsResponse = (await Get<TeamModel[]>("api/Teams", HttpStatusCode.OK))!;
        getTeamsResponse.Should().NotBeEmpty().And.ContainEquivalentOf(expectedTeam);

        var expectedTeamWithRoster = new TeamWithRosterModel(
            expectedTeam.Id,
            expectedTeam.Names,
            expectedTeam.Colors,
            []);

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
                ["default"] = new DisplayColor(Color.Black, Color.White)
            });

        var createResponse = (await Post<TeamModel>("api/Teams", team, HttpStatusCode.Created))!;

        var roster = new RosterModel([ 
            new Skater("1", "One", "he/him", SkaterRole.Skater),
            new Skater("2", "Two", "she/her", SkaterRole.NotSkating),
            new Skater("3", "Three", "they/them", SkaterRole.BenchStaff),
            new Skater("4", "Four", "", SkaterRole.Captain),
        ]);

        var rosterPath = $"api/Teams/{createResponse.Id}/roster";
        var originalRoster = await Get<RosterModel>(rosterPath, HttpStatusCode.OK);

        originalRoster.Should().NotBeNull().And.BeEquivalentTo(new RosterModel([]));

        await Put(rosterPath, roster, HttpStatusCode.OK);

        var newRoster = await Get<RosterModel>(rosterPath, HttpStatusCode.OK);
        newRoster.Should().BeEquivalentTo(roster);
    }

    protected override void CleanDatabase()
    {
        using var connection = new SQLiteConnection(Path.Combine(RunningEnvironment.RootPath, "db", "teams.db"));

        connection.Execute("DELETE FROM team");
    }
}
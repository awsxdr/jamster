using amethyst.DataStores;
using amethyst.Domain;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/[controller]")]
public class TeamsController(
    ITeamsDataStore teamsStore,
    ILogger<TeamsController> logger
    ) : Controller
{
    [HttpGet]
    public ActionResult<TeamModel[]> GetTeams()
    {
        logger.LogDebug("Getting teams list");

        return Ok(teamsStore.GetTeams().Select(t => (TeamModel) t).ToArray());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<TeamWithRosterModel> GetTeam(Guid id, [FromQuery] bool includeArchived)
    {
        logger.LogDebug("Getting team with ID: {id}", id);

        return (includeArchived ? teamsStore.GetTeamIncludingArchived(id) : teamsStore.GetTeam(id)) switch
        {
            Success<Team> s => Ok((TeamWithRosterModel) s.Value),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpPost]
    public ActionResult<TeamModel> CreateTeam([FromBody] CreateTeamModel team)
    {
        logger.LogDebug("Creating new team");

        var newTeam = teamsStore.CreateTeam((Team) team);

        return Created($"/api/teams/{newTeam.Id}", (TeamModel) newTeam);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteTeam(Guid id)
    {
        logger.LogInformation("Deleting team {id}", id);

        teamsStore.ArchiveTeam(id);

        return NoContent();
    }

    [HttpGet("{teamId:guid}/roster")]
    public ActionResult<RosterModel> GetRoster(Guid teamId)
    {
        logger.LogDebug("Getting team roster with team ID: {teamId}", teamId);

        return teamsStore.GetTeam(teamId) switch
        {
            Success<Team> s => Ok(s.Value.Roster),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpPut("{teamId:guid}/roster")]
    public IActionResult SetRoster(Guid teamId, [FromBody] RosterModel roster)
    {
        logger.LogDebug("Setting roster for team ID: {teamId}", teamId);

        return teamsStore.SetRoster(teamId, roster.Roster) switch
        {
            Success => Ok(),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }
}

public record CreateTeamModel(Dictionary<string, string> Names, Dictionary<string, DisplayColor> Colors)
{
    public static explicit operator Team(CreateTeamModel model) => new(Guid.Empty, model.Names, model.Colors, []);
}

public record TeamModel(Guid Id, Dictionary<string, string> Names, Dictionary<string, DisplayColor> Colors)
{
    public static explicit operator Team(TeamModel model) => new(model.Id, model.Names, model.Colors, []);
    public static explicit operator TeamModel(Team team) => new(team.Id, team.Names, team.Colors);
}

public record TeamWithRosterModel(
    Guid Id,
    Dictionary<string, string> Names,
    Dictionary<string, DisplayColor> Colors,
    List<Skater> Roster)
{
    public static explicit operator Team(TeamWithRosterModel model) =>
        new(model.Id, model.Names, model.Colors, model.Roster);

    public static explicit operator TeamWithRosterModel(Team team) =>
        new(team.Id, team.Names, team.Colors, team.Roster);
}

public record RosterModel(List<Skater> Roster);
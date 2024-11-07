using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;
using DotNext.Collections.Generic;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("api/[controller]")]
public class TeamsController(
    ITeamStore teamsStore,
    ILogger<TeamsController> logger
    ) : Controller
{
    [HttpGet]
    public ActionResult<TeamModel[]> GetTeams()
    {
        logger.LogDebug("Getting teams list");

        return Ok(teamsStore.GetTeams().Select(t => (TeamModel) t).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<TeamModel>> CreateTeam([FromBody] CreateTeamModel team)
    {
        logger.LogDebug("Creating new team");

        var newTeam = await teamsStore.CreateTeam((Team) team);

        return Created($"/api/teams/{newTeam.Id}", (TeamModel) newTeam);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<TeamWithRosterModel> GetTeam(Guid id, [FromQuery] bool includeArchived)
    {
        logger.LogDebug("Getting team with ID: {id}", id);

        return (includeArchived ? teamsStore.GetTeamIncludingArchived(id) : teamsStore.GetTeam(id)) switch
        {
            Success<Team> s => Ok((TeamWithRosterModel)s.Value),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamModel team)
    {
        logger.LogDebug("Updating team with ID: {id}", id);

        return await teamsStore.UpdateTeam(((Team)team) with { Id = id }) switch
        {
            Success => Ok(),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        logger.LogInformation("Deleting team {id}", id);

        return await teamsStore.ArchiveTeam(id) switch
        {
            Success => NoContent(),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpGet("{teamId:guid}/roster")]
    public ActionResult<RosterModel> GetRoster(Guid teamId)
    {
        logger.LogDebug("Getting team roster with team ID: {teamId}", teamId);

        return teamsStore.GetTeam(teamId) switch
        {
            Success<Team> s => Ok((RosterModel) s.Value.Roster),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }

    [HttpPut("{teamId:guid}/roster")]
    public async Task<IActionResult> SetRoster(Guid teamId, [FromBody] RosterModel roster)
    {
        logger.LogDebug("Setting roster for team ID: {teamId}", teamId);

        return await teamsStore.SetRoster(teamId, roster.Roster) switch
        {
            Success => Ok(),
            Failure<TeamNotFoundError> => NotFound(),
            _ => throw new UnexpectedResultException()
        };
    }
}

public record CreateTeamModel(Dictionary<string, string> Names, Dictionary<string, Dictionary<string, DisplayColor>> Colors)
{
    public static explicit operator Team(CreateTeamModel model) => new(Guid.Empty, model.Names, model.Colors, []);
}

public record UpdateTeamModel(Dictionary<string, string> Names, Dictionary<string, Dictionary<string, DisplayColor>> Colors)
{
    public static explicit operator Team(UpdateTeamModel model) => new(Guid.Empty, model.Names, model.Colors, []);
}

public record TeamModel(Guid Id, Dictionary<string, string> Names, Dictionary<string, Dictionary<string, DisplayColor>> Colors)
{
    public static explicit operator Team(TeamModel model) => new(model.Id, model.Names, model.Colors, []);
    public static explicit operator TeamModel(Team team) => new(team.Id, team.Names, team.Colors);

    public virtual bool Equals(TeamModel? other) =>
        other is not null
        && other.Id == Id
        && other.Names.SequenceEqual(Names)
        && other.Colors.Keys.SequenceEqual(Colors.Keys)
        && other.Colors.All(o => o.Value.SequenceEqual(Colors[o.Key]));

    public override int GetHashCode() => HashCode.Combine(Id, Names.SequenceHashCode(), Colors.SequenceHashCode());
}

public record TeamWithRosterModel(
    Guid Id,
    Dictionary<string, string> Names,
    Dictionary<string, Dictionary<string, DisplayColor>> Colors,
    List<Skater> Roster)
{
    public static explicit operator Team(TeamWithRosterModel model) =>
        new(model.Id, model.Names, model.Colors, model.Roster);

    public static explicit operator TeamWithRosterModel(Team team) =>
        new(team.Id, team.Names, team.Colors, team.Roster);
}

public record RosterModel(List<Skater> Roster)
{
    public static explicit operator RosterModel(List<Skater> skaters) => new(skaters);
}
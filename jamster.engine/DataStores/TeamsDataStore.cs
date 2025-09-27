using System.Text.Json.Nodes;

using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.DataStores;

public interface ITeamsDataStore
{
    IEnumerable<Team> GetTeams();
    Team CreateTeam(Team team);
    Result<Team> GetTeam(Guid teamId);
    Result<Team> GetTeamIncludingArchived(Guid teamId);
    Result UpdateTeam(Team team);
    Result ArchiveTeam(Guid teamId);
    Result SetRoster(Guid teamId, IEnumerable<Skater> skaters);
}

public class TeamsDataStore : DataStore, ITeamsDataStore
{
    private readonly ISystemTime _systemTime;
    private readonly IDataTable<Team, Guid> _teamsTable;

    public TeamsDataStore(ConnectionFactory connectionFactory, IDataTableFactory dataTableFactory, ISystemTime systemTime)
        : base("teams", 4, connectionFactory, dataTableFactory)
    {
        _systemTime = systemTime;

        _teamsTable = GetTable<Team, Guid>(t => t.Id);
    }

    public IEnumerable<Team> GetTeams() =>
        _teamsTable.GetAll().ToArray();

    public Result<Team> GetTeam(Guid teamId) =>
        _teamsTable.Get(teamId) switch
        {
            Success<Team> s => s,
            Failure<NotFoundError> => Result<Team>.Fail<TeamNotFoundError>(),
            var r => throw new UnexpectedResultException(r)
        };

    public Result<Team> GetTeamIncludingArchived(Guid teamId) =>
        _teamsTable.GetIncludingArchived(teamId) switch
        {
            Success<Team> s => s,
            Failure<NotFoundError> => Result<Team>.Fail<TeamNotFoundError>(),
            var r => throw new UnexpectedResultException(r)
        };

    public Team CreateTeam(Team team)
    {
        var newTeam = team with {Id = Guid.NewGuid()};

        _teamsTable.Insert(newTeam);

        return newTeam;
    }

    public Result UpdateTeam(Team team) =>
        _teamsTable.Update(team.Id, team);

    public Result ArchiveTeam(Guid teamId) =>
        _teamsTable.Archive(teamId) switch
        {
            Success s => s,
            Failure<NotFoundError> => Result.Fail<TeamNotFoundError>(),
            var r => throw new UnexpectedResultException(r)
        };

    public Result SetRoster(Guid teamId, IEnumerable<Skater> skaters) =>
        _teamsTable.Get(teamId)
                .ThenMap(team => team with {Roster = skaters.ToArray()})
                .Then(_teamsTable.Update, teamId)
            switch
            {
                Success s => s,
                Failure<NotFoundError> => Result.Fail<TeamNotFoundError>(),
                var r => throw new UnexpectedResultException(r)
            };

    protected override void ApplyUpgrade(int version)
    {
    }

    private IEnumerable<JsonObject> GetAllItemsAsJsonObjects() =>
        _teamsTable.GetAllItems()
            .Select(i => JsonNode.Parse(i.Data))
            .Cast<JsonObject>();
}

public record DisplayColor(Color Foreground, Color Background);

public class TeamNotFoundError : NotFoundError;
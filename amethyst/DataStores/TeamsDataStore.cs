using amethyst.Domain;
using Func;

namespace amethyst.DataStores;

public interface ITeamsDataStore
{
    IEnumerable<Team> GetTeams();
    Team CreateTeam(Team team);
    Result<Team> GetTeam(Guid teamId);
    Result<Team> GetTeamIncludingArchived(Guid teamId);
    Result ArchiveTeam(Guid teamId);
    Result SetRoster(Guid teamId, IEnumerable<Skater> skaters);
}

public class TeamsDataStore(ConnectionFactory connectionFactory) 
    : DataStore<Team>("teams", t => t.Id, connectionFactory)
    , ITeamsDataStore
{
    public IEnumerable<Team> GetTeams() =>
        GetAll().ToArray();

    public Result<Team> GetTeam(Guid teamId) => 
        Get(teamId) switch
        {
            Success<Team> s => s,
            Failure<NotFoundError> => Result<Team>.Fail<TeamNotFoundError>(),
            _ => throw new UnexpectedResultException()
        };

    public Result<Team> GetTeamIncludingArchived(Guid teamId) =>
        GetIncludingArchived(teamId) switch
        {
            Success<Team> s => s,
            Failure<NotFoundError> => Result<Team>.Fail<TeamNotFoundError>(),
            _ => throw new UnexpectedResultException()
        };

    public Team CreateTeam(Team team)
    {
        var newTeam = team with {Id = Guid.NewGuid()};

        Insert(newTeam);

        return newTeam;
    }

    public Result ArchiveTeam(Guid teamId) =>
        Archive(teamId) switch
        {
            Success s => s,
            Failure<NotFoundError> => Result.Fail<TeamNotFoundError>(),
            _ => throw new UnexpectedResultException()
        };

    public Result SetRoster(Guid teamId, IEnumerable<Skater> skaters) =>
        Get(teamId)
                .ThenMap(team => team with {Roster = skaters.ToList()})
                .Then(team =>
                {
                    Update(teamId, team);
                    return Result.Succeed(team);
                })
            switch
            {
                Success<Team> s => s,
                Failure<NotFoundError> => Result.Fail<TeamNotFoundError>(),
                _ => throw new UnexpectedResultException()
            };
}

public record Team(Guid Id, Dictionary<string, string> Names, Dictionary<string, DisplayColor> Colors, List<Skater> Roster)
{
    public Team() : this(Guid.NewGuid(), [], [], [])
    {
    }
}

public record Skater(string Number, string Name, string Pronouns, SkaterRole Role);

public enum SkaterRole
{
    Skater,
    Captain,
    NotSkating,
    BenchStaff,
}

public record DisplayColor(Color Foreground, Color Background);

public class TeamNotFoundError : NotFoundError;
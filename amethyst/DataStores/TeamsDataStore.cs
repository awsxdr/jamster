using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Domain;
using amethyst.Services;
using Func;

namespace amethyst.DataStores;

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
                .ThenMap(team => team with {Roster = skaters.ToList()})
                .Then(_teamsTable.Update, teamId)
            switch
            {
                Success s => s,
                Failure<NotFoundError> => Result.Fail<TeamNotFoundError>(),
                var r => throw new UnexpectedResultException(r)
            };

    protected override void ApplyUpgrade(int version)
    {
        #region Database upgrades
        switch (version)
        {
            case 1:
                break;

            case 2:
            case 3:
            {
                var items = 
                    GetAllItemsAsJsonObjects()
                        .Select(i => new 
                        {
                            Id = i["Id"]!.GetValue<Guid>(),
                            Names = i["Names"]!.AsObject().ToDictionary(k => k.Key, v => v.Value!.GetValue<string>()),
                            Colors = new Dictionary<string, Dictionary<string, DisplayColor>>
                            {
                                ["Legacy"] = i["Colors"]!.AsObject().ToDictionary(k => k.Key, v => v.Value!.Deserialize<DisplayColor>()!)
                            },
                            Roster = i["Roster"]!.AsArray().Select(n => n!.Deserialize<Skater>()!).ToList(),
                            LastUpdateTime = _systemTime.UtcNow()
                        })
                        .ToArray();

                foreach (var item in items)
                {
                    _teamsTable.Update(item.Id, JsonSerializer.SerializeToNode(item)!.AsObject());
                }

                break;
            }

            case 4:
            {
                    var items =
                        GetAllItemsAsJsonObjects()
                            .Select(i => new
                            {
                                Id = i["Id"]!.GetValue<Guid>(),
                                Names = i["Names"]!.AsObject().ToDictionary(k => k.Key, v => v.Value!.GetValue<string>()),
                                Colors = i["Colors"]!.AsObject().ToDictionary(
                                    k => k.Key,
                                    v => v.Value!.AsObject()[v.Key]!.AsObject().Map(c => new
                                    {
                                        ShirtColor = c["Background"]!.GetValue<string>(),
                                        ComplementaryColor = c["Foreground"]!.GetValue<string>(),
                                    })
                                ),
                                Roster = i["Roster"]!.AsArray().Select(n => n!.Deserialize<Skater>()!).ToList(),
                                LastUpdateTime = _systemTime.UtcNow()
                            })
                            .ToArray();

                    foreach (var item in items)
                    {
                        _teamsTable.Update(item.Id, JsonSerializer.SerializeToNode(item)!.AsObject());
                    }

                    break;
            }
        }
        #endregion
    }

    private IEnumerable<JsonObject> GetAllItemsAsJsonObjects() =>
        _teamsTable.GetAllItems()
            .Select(i => JsonNode.Parse(i.Data))
            .Cast<JsonObject>();
}

public record DisplayColor(Color Foreground, Color Background);

public class TeamNotFoundError : NotFoundError;
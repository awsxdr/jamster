using amethyst.DataStores;
using amethyst.Extensions;
using Func;

namespace amethyst.Services;

public interface ITeamStore
{
    event AsyncEventHandler<TeamStore.TeamCreatedEventArgs>? TeamCreated;
    event AsyncEventHandler<TeamStore.TeamArchivedEventArgs>? TeamArchived;
    event AsyncEventHandler<TeamStore.TeamChangedEventArgs>? TeamChanged;
    IEnumerable<Team> GetTeams();
    Result<Team> GetTeam(Guid teamId);
    Result<Team> GetTeamIncludingArchived(Guid teamId);
    Task<Team> CreateTeam(Team team);
    Task<Result> UpdateTeam(Team team);
    Task<Result> ArchiveTeam(Guid teamId);
    Task<Result> SetRoster(Guid teamId, IEnumerable<Skater> skaters);
}

public class TeamStore(ITeamsDataStore teamDataStore) : ITeamStore
{
    public event AsyncEventHandler<TeamCreatedEventArgs>? TeamCreated;
    public event AsyncEventHandler<TeamArchivedEventArgs>? TeamArchived;
    public event AsyncEventHandler<TeamChangedEventArgs>? TeamChanged; 

    public IEnumerable<Team> GetTeams() =>
        teamDataStore.GetTeams();

    public Result<Team> GetTeam(Guid teamId) =>
        teamDataStore.GetTeam(teamId);

    public Result<Team> GetTeamIncludingArchived(Guid teamId) =>
        teamDataStore.GetTeamIncludingArchived(teamId);

    public async Task<Team> CreateTeam(Team team)
    {
        var newTeam = teamDataStore.CreateTeam(team);

        await TeamCreated.InvokeHandlersAsync(this, new(newTeam));

        return newTeam;
    }

    public Task<Result> UpdateTeam(Team team) =>
        teamDataStore.UpdateTeam(team)
            .Then(async () =>
            {
                await TeamChanged.InvokeHandlersAsync(this, new(team));
                return Result.Succeed();
            });

    public Task<Result> ArchiveTeam(Guid teamId) =>
        teamDataStore.ArchiveTeam(teamId)
            .Then(async () =>
            {
                await TeamArchived.InvokeHandlersAsync(this, new(teamId));
                return Result.Succeed();
            });

    public Task<Result> SetRoster(Guid teamId, IEnumerable<Skater> skaters) =>
        teamDataStore.SetRoster(teamId, skaters)
            .Then(() => teamDataStore.GetTeam(teamId))
            .Then(async team =>
                {
                    await TeamChanged.InvokeHandlersAsync(this, new(team));
                    return Result.Succeed();
                });

    public sealed class TeamCreatedEventArgs(Team team) : EventArgs
    {
        public Team Team { get; } = team;
    }

    public sealed class TeamArchivedEventArgs(Guid teamId) : EventArgs
    {
        public Guid TeamId { get; } = teamId;
    }

    public sealed class TeamChangedEventArgs(Team team) : EventArgs
    {
        public Team Team { get; } = team;
    }
}
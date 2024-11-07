using amethyst.Controllers;
using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class TeamsNotifier
{
    public TeamsNotifier(
        ITeamStore teamStore,
        IHubContext<TeamsHub, ITeamsHubClient> hubContext,
        ILogger<TeamsNotifier> logger
    )
    {
        teamStore.TeamChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team change");

            await hubContext.Clients.Group("TeamChanged").TeamChanged((TeamWithRosterModel)e.Team);
        };

        teamStore.TeamCreated += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team creation");

            await hubContext.Clients.Group("TeamCreated").TeamCreated((TeamWithRosterModel) e.Team);
        };

        teamStore.TeamArchived += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team archiving");
            await hubContext.Clients.Group("TeamArchived").TeamArchived(e.TeamId);
        };
    }
}

public interface ITeamsHubClient
{
    Task TeamChanged(TeamWithRosterModel team);
    Task TeamCreated(TeamWithRosterModel team);
    Task TeamArchived(Guid teamId);
}

public class TeamsHub : Hub<ITeamsHubClient>
{
    public Task WatchTeamCreated() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamCreated");

    public Task WatchTeamChanged() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamChanged");

    public Task WatchTeamArchived() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamArchived");
}
using jamster.engine.Controllers;
using jamster.engine.Services;

using Microsoft.AspNetCore.SignalR;

namespace jamster.engine.Hubs;

public class TeamsNotifier : Notifier<TeamsHub, ITeamsHubClient>
{
    public override string HubAddress => "api/hubs/teams";

    public TeamsNotifier(
        ITeamStore teamStore,
        IHubContext<TeamsHub, ITeamsHubClient> hubContext,
        ILogger<TeamsNotifier> logger
    ) : base(hubContext)
    {
        teamStore.TeamChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team change");

            await HubContext.Clients.Group("TeamChanged").TeamChanged((TeamWithRosterModel)e.Team);
        };

        teamStore.TeamCreated += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team creation");

            await HubContext.Clients.Group("TeamCreated").TeamCreated((TeamWithRosterModel) e.Team);
        };

        teamStore.TeamArchived += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team archiving");
            await HubContext.Clients.Group("TeamArchived").TeamArchived(e.TeamId);
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
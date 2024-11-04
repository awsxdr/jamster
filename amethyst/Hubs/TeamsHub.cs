using amethyst.Controllers;
using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class TeamsNotifier
{
    public TeamsNotifier(
        ITeamStore teamStore,
        IHubContext<TeamsHub> hubContext,
        ILogger<TeamsNotifier> logger
    )
    {
        teamStore.TeamChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team change");

            await hubContext.Clients.Group("TeamChanged").SendAsync("TeamChanged", (TeamWithRosterModel)e.Team);
        };

        teamStore.TeamCreated += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team creation");

            await hubContext.Clients.Group("TeamCreated").SendAsync("TeamCreated", (TeamWithRosterModel)e.Team);
        };

        teamStore.TeamArchived += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of team archiving");

            await hubContext.Clients.Group("TeamArchived").SendAsync("TeamArchived", e.TeamId);
        };
    }
}

public class TeamsHub : Hub
{
    public Task WatchTeamCreated() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamCreated");

    public Task WatchTeamChanged() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamChanged");

    public Task WatchTeamArchived() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "TeamArchived");
}
using jamster.Services;
using Microsoft.AspNetCore.SignalR;

namespace jamster.Hubs;

public class UsersNotifier : Notifier<UsersHub, IUsersHubClient>
{
    public override string HubAddress => "api/hubs/users";

    public UsersNotifier(
        IUserService userService,
        IHubContext<UsersHub, IUsersHubClient> hubContext,
        ILogger<UsersNotifier> logger
    ) : base(hubContext)
    {
        userService.UserListChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of user list change");

            await HubContext.Clients.Group(nameof(IUsersHubClient.UserListChanged)).UserListChanged(e.Users);
        };

        userService.UserConfigurationChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of user configuration change");

            await HubContext.Clients.Group(UsersHub.GetUserConfigurationChangedGroupName(e.UserName, e.ConfigurationType.Name)).UserConfigurationChanged(e.UserName, e.ConfigurationType.Name, e.Value);
        };
    }
}

public interface IUsersHubClient
{
    Task UserConfigurationChanged(string userName, string configurationType, object value);
    Task UserListChanged(string[] users);
}

public class UsersHub : Hub<IUsersHubClient>
{
    public Task WatchUserList() =>
        Groups.AddToGroupAsync(Context.ConnectionId, nameof(IUsersHubClient.UserListChanged));

    public Task WatchUserConfiguration(string userName, string configurationType) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GetUserConfigurationChangedGroupName(userName, configurationType));

    public static string GetUserConfigurationChangedGroupName(string userName, string configurationType) =>
        $"{nameof(IUsersHubClient.UserConfigurationChanged)}_{userName.ToLowerInvariant()}_{configurationType.ToLowerInvariant()}";
}
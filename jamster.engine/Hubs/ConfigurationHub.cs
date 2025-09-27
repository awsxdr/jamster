using jamster.engine.Services;

using Microsoft.AspNetCore.SignalR;

namespace jamster.engine.Hubs;

public class ConfigurationNotifier : Notifier<ConfigurationHub>
{
    public override string HubAddress => "api/hubs/configuration";

    public ConfigurationNotifier(
        IConfigurationService configurationService,
        IHubContext<ConfigurationHub> hubContext,
        ILogger<ConfigurationNotifier> logger) 
        : base(hubContext)
    {
        configurationService.ConfigurationChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying clients of {configurationType} configuration change", e.Key);

            await hubContext.Clients.Group(e.Key).SendAsync("ConfigurationChanged", e.Key, e.Value);
        };
    }
}

public class ConfigurationHub : Hub
{
    public async Task WatchConfiguration(string configurationKey)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, configurationKey);
    }

    public async Task UnwatchConfiguration(string configurationKey)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, configurationKey);
    }
}
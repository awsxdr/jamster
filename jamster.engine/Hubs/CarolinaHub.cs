using System.Collections.Concurrent;

using jamster.engine.Carolina;

using Microsoft.AspNetCore.SignalR;

namespace jamster.engine.Hubs;


[RegisterOnOption(nameof(CommandLineOptions.EnableCarolinaCompatibility))]
public class CarolinaNotifier(IStateTracker stateTracker, IHubContext<CarolinaHub> hubContext)
    : Notifier<CarolinaHub>(hubContext)
{
    private readonly ConcurrentDictionary<string, ConnectionWatch> _watches = new();

    public override string HubAddress => "api/hubs/carolina";

    public static bool ShouldRegister(CommandLineOptions options) => options.EnableCarolinaCompatibility;

    public Task Register(string connectionId, string[] paths)
    {
        var watch = _watches.GetOrAdd(connectionId, _ => new ConnectionWatch());

        lock (watch)
        {
            foreach (var path in paths)
                watch.Paths.Add(path);

            if (watch.WatchId.HasValue)
                stateTracker.UnwatchStates(watch.WatchId.Value);

            watch.WatchId = stateTracker.WatchStates(
                watch.Paths.ToArray(),
                async changes => await HubContext.Clients.Client(connectionId).SendAsync("StateUpdated", changes));
        }

        return Task.CompletedTask;
    }

    public void Disconnect(string connectionId)
    {
        if (_watches.TryRemove(connectionId, out var watch) && watch.WatchId.HasValue)
            stateTracker.UnwatchStates(watch.WatchId.Value);
    }

    private sealed class ConnectionWatch
    {
        public HashSet<string> Paths { get; } = [];
        public Guid? WatchId { get; set; }
    }
}

public class CarolinaHub(CarolinaNotifier notifier) : Hub
{
    public async Task Register(string[] paths) =>
        await notifier.Register(Context.ConnectionId, paths);

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        notifier.Disconnect(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
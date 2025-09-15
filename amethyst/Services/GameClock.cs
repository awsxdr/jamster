using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;

namespace amethyst.Services;

public interface IGameClock : IDisposable
{
    void Run();
    void Stop();
}

public class GameClock(GameInfo game, IEnumerable<ITickReceiverAsync> receivers, IEventBus eventBus, ISystemTime systemTime, ILogger<GameClock> logger) : IGameClock
{
    public delegate IGameClock Factory(GameInfo game, IEnumerable<ITickReceiverAsync> receivers);

    public static readonly Guid TickEventId = Guid.Parse("00000000-0000-0000-0000-00000000c10c");

    public int TicksBetweenFrames { get; init; } = 10;

    private volatile bool _isRunning;

    public void Run()
    {
        _isRunning = true;

        new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None).StartNew(async() =>
        {
            while (_isRunning)
            {
                var tick = systemTime.GetTick();

                foreach (var receiver in receivers)
                {
                    try
                    {
                        var implicitEvents = await receiver.TickAsync(tick);
                        foreach (var @event in implicitEvents)
                        {
                            if (@event is IAlwaysPersisted)
                                await eventBus.AddEvent(game, @event);
                            else
                                await eventBus.AddEventWithoutPersisting(game, @event, TickEventId);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while processing tick with receiver {receiverName}", receiver.GetType().Name);
                    }
                }

                Thread.Sleep(TicksBetweenFrames);
            }
        });
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Dispose()
    {
        _isRunning = false;
    }
}

public interface ITickReceiver : ITickReceiverAsync
{
    IEnumerable<Event> Tick(Tick tick);

    Task<IEnumerable<Event>> ITickReceiverAsync.TickAsync(Tick tick) =>
        Task.FromResult(Tick(tick));
}

public interface ITickReceiverAsync
{
    Task<IEnumerable<Event>> TickAsync(Tick tick);
}
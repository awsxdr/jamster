namespace amethyst.Services;

public interface IGameClock : IDisposable
{
    void Run();
}

public class GameClock(IEnumerable<ITickReceiver> receivers, ILogger<GameClock> logger) : IGameClock
{
    public int MillisecondsBetweenFrames { get; init; } = 100;

    private volatile bool _isRunning;

    public void Run()
    {
        _isRunning = true;

        new Thread(() =>
        {
            var lastTick = GetTick();

            while (_isRunning)
            {
                var tick = GetTick();
                var tickDelta = tick - lastTick;

                foreach (var receiver in receivers)
                {
                    try
                    {
                        receiver.Tick(tick, tickDelta);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while processing tick with receiver {receiverName}", receiver.GetType().Name);
                    }
                }

                lastTick = tick;

                Thread.Sleep(MillisecondsBetweenFrames);
            }
        }).Start();

        return;

        static long GetTick() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void Dispose()
    {
        _isRunning = false;
    }
}

public interface ITickReceiver
{
    void Tick(long tick, long tickDelta);
}
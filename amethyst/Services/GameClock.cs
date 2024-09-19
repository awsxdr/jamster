namespace amethyst.Services;

public interface IGameClock : IDisposable
{
}

public class GameClock(IEnumerable<ITickReceiver> receivers) : IGameClock
{
    public int MillisecondsBetweenFrames { get; init; } = 100;

    private volatile bool _isRunning = false;

    public void Run()
    {
        _isRunning = true;

        new Thread(() =>
        {
            while (_isRunning)
            {

            }
        }).Start();
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
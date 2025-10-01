using System.Diagnostics;

using jamster.engine.Domain;

namespace jamster.ui.tests;

public interface IReminderSetter
{
    Task WaitForTick(Tick tick);
}

public class GameClock : IReminderSetter, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    public Tick CurrentTick { get; private set; } = 0;

    public void Start(long startTick = 0)
    {
        _cancellationTokenSource?.Cancel();

        _cancellationTokenSource = new();
        CurrentTick = startTick;

        _ = RunClock(_cancellationTokenSource.Token);
    }

    public Task WaitForTick(Tick tick) => Task.Run(async () =>
    {
        while (CurrentTick < tick)
            await Task.Delay(10);
    });

    private async Task RunClock(CancellationToken cancellationToken)
    {
        var startTick = CurrentTick;
        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);

            CurrentTick = Tick.FromSeconds(TimeSpan.FromMilliseconds(startTick + stopwatch.ElapsedMilliseconds).TotalSeconds);
        }
    }

    public void Dispose() => _cancellationTokenSource?.Dispose();
}
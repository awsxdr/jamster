using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class IntermissionClock(GameContext context, IEventBus eventBus, ILogger<IntermissionClock> logger) 
    : Reducer<IntermissionClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEventAsync<IntermissionStarted>
    , IHandlesEvent<IntermissionLengthSet>
    , IHandlesEvent<IntermissionEnded>
    , ITickReceiver
{
    protected override IntermissionClockState DefaultState => new(false, true, 0, 0);

    public void Handle(JamStarted @event)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        logger.LogDebug("Stopping intermission clock due to jam start");

        SetState(state with { IsRunning = false });
    }

    public async Task HandleAsync(IntermissionStarted @event)
    {
        logger.LogInformation("Intermission started");

        await eventBus.AddEvent(Context.GameInfo, new IntermissionLengthSet(@event.Tick, new(@event.Body.DurationInSeconds)));

        SetState(GetState() with { IsRunning = true });
    }

    public void Handle(IntermissionLengthSet @event)
    {
        logger.LogDebug("Intermission length set to {length} seconds", @event.Body.DurationInSeconds);
        SetState(GetState() with
        {
            HasExpired = @event.Body.DurationInSeconds <= 0,
            SecondsRemaining = @event.Body.DurationInSeconds,
            TargetTick = @event.Tick + @event.Body.DurationInSeconds * 1000,
        });
    }

    public void Handle(IntermissionEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        SetState(state with { IsRunning = false });
    }

    public Task Tick(Tick tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return Task.CompletedTask;

        var ticksRemaining = Math.Max(0, state.TargetTick - tick);

        SetState(state with
        {
            HasExpired = ticksRemaining == 0,
            SecondsRemaining = (int)(ticksRemaining / 1000),
        });

        return Task.CompletedTask;
    }
}

public sealed record IntermissionClockState(
    bool IsRunning,
    bool HasExpired,
    Tick TargetTick,
    int SecondsRemaining);
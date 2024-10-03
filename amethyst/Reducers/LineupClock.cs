using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public sealed class LineupClock(GameContext gameContext, ILogger<LineupClock> logger)
    : Reducer<LineupClockState>(gameContext), IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , ITickReceiver
{
    public const long LineupDurationInTicks = 30 * 1000;

    protected override LineupClockState DefaultState => new(false, 0, 0, 0);

    public void Handle(JamStarted @event)
    {
        if (!GetState().IsRunning) return;

        logger.LogDebug("Jam started, stopping lineup clock");
        SetState(GetState() with { IsRunning = false });
    }

    public void Handle(JamEnded @event)
    {
        if (GetState().IsRunning || GetState<TimeoutClockState>().IsRunning) return;

        var periodClock = GetState<PeriodClockState>();
        var ticksRemainingInPeriod =
            PeriodClock.PeriodLengthInTicks - periodClock.TicksPassed;

        if (ticksRemainingInPeriod <= 0)
        {
            logger.LogDebug("Not starting lineup at jam end due to period clock expiry");
            return;
        }

        logger.LogDebug("Starting lineup clock due to jam end");
        SetState(new(true, @event.Tick, 0, 0));
    }

    public void Handle(TimeoutStarted @event)
    {
        if (!GetState().IsRunning) return;

        logger.LogDebug("Timeout started, stopping lineup clock");
        SetState(GetState() with { IsRunning = false });
    }

    public void Tick(Tick tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        var newState = state with
        {
            TicksPassed = tick - state.StartTick,
            SecondsPassed = (int)((tick - state.StartTick) / 1000L),
        };
        SetState(newState);
    }
}

public record LineupClockState(bool IsRunning, long StartTick, [property: IgnoreChange] long TicksPassed, int SecondsPassed);

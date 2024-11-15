using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TripScore(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<TripScoreState>(context)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<ScoreSet>
    , ITickReceiver
{
    protected override TripScoreState DefaultState => new(0, 0);

    public static readonly Tick TripScoreResetTimeInTicks = 3000;

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => HandleIfTeam(@event, () =>
    {
        var state = GetState();

        SetState(new(
            Math.Min(4, Math.Max(0,
                @event.Tick - state.LastChangeTick < TripScoreResetTimeInTicks
                ? state.Score + @event.Body.Value
                : @event.Body.Value)),
            @event.Tick
        ));

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSet @event) => HandleIfTeam(@event, () =>
    {
        logger.LogDebug("Resetting pass score due to absolute score being set");

        SetState(new(0, @event.Tick));

        return [];
    });

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (state.Score == 0) return [];

        if (tick - state.LastChangeTick < TripScoreResetTimeInTicks) return [];

        logger.LogDebug("Resetting pass score due to pass time expiring");

        SetState(state with { Score = 0, LastChangeTick = state.LastChangeTick + TripScoreResetTimeInTicks });

        return [];
    }

    private IEnumerable<Event> HandleIfTeam<TEvent>(TEvent @event, Func<IEnumerable<Event>> handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return [];

        return handler();
    }
}

public sealed record TripScoreState(int Score, Tick LastChangeTick);

public sealed class HomeTripScore(ReducerGameContext context, ILogger<HomeTripScore> logger) : TripScore(TeamSide.Home, context, logger);
public sealed class AwayTripScore(ReducerGameContext context, ILogger<AwayTripScore> logger) : TripScore(TeamSide.Away, context, logger);

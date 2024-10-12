using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class PassScore(TeamSide teamSide, GameContext context, ILogger logger)
    : Reducer<PassScoreState>(context)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<ScoreSet>
    , ITickReceiver
{
    protected override PassScoreState DefaultState => new(0, 0);

    public static readonly Tick PassScoreResetTimeInTicks = 3000;

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => HandleIfTeam(@event, () =>
    {
        var state = GetState();

        SetState(new(
            Math.Min(4, Math.Max(0,
                @event.Tick - state.LastChangeTick < PassScoreResetTimeInTicks
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

        if (tick - state.LastChangeTick < PassScoreResetTimeInTicks) return [];

        logger.LogDebug("Resetting pass score due to pass time expiring");

        SetState(state with { Score = 0, LastChangeTick = state.LastChangeTick + PassScoreResetTimeInTicks });

        return [];
    }

    private IEnumerable<Event> HandleIfTeam<TEvent>(TEvent @event, Func<IEnumerable<Event>> handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return [];

        return handler();
    }
}

public sealed record PassScoreState(int Score, Tick LastChangeTick);

public sealed class HomePassScore(GameContext context, ILogger<HomePassScore> logger) : PassScore(TeamSide.Home, context, logger);
public sealed class AwayPassScore(GameContext context, ILogger<AwayPassScore> logger) : PassScore(TeamSide.Away, context, logger);

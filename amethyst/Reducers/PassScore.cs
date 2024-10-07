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

    public void Handle(ScoreModifiedRelative @event) => HandleIfTeam(@event, () =>
    {
        var state = GetState();

        SetState(new(
            @event.Tick - state.LastChangeTick < PassScoreResetTimeInTicks
            ? Math.Min(4, Math.Max(0, state.Score + @event.Body.Value))
            : Math.Min(4, Math.Max(0, @event.Body.Value)),
            @event.Tick
        ));
    });

    public void Handle(ScoreSet @event)
    {
    }

    public Task Tick(Tick tick)
    {
        return Task.CompletedTask;
    }

    private void HandleIfTeam<TEvent>(TEvent @event, Action handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return;

        handler();
    }
}

public sealed record PassScoreState(int Score, Tick LastChangeTick);

public sealed class HomePassScore(GameContext context, ILogger<HomePassScore> logger) : PassScore(TeamSide.Home, context, logger);
public sealed class AwayPassScore(GameContext context, ILogger<AwayPassScore> logger) : PassScore(TeamSide.Home, context, logger);

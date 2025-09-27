using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class PenaltyBox(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<PenaltyBoxState>(context)
    , IHandlesEvent<PenaltyAssessed>
    , IHandlesEvent<SkaterSatInBox>
    , IHandlesEvent<SkaterReleasedFromBox>
    , IHandlesEvent<SkaterSubstitutedInBox>
{
    protected override PenaltyBoxState DefaultState => new([], []);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(PenaltyAssessed @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Penalty assessed to {number} on {team} team", @event.Body.SkaterNumber, teamSide);

        var state = GetState();

        SetState(state with
        {
            QueuedSkaters = state.QueuedSkaters.Append(@event.Body.SkaterNumber).Except(state.Skaters).ToArray(),
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Skaters.Any(s => s == @event.Body.SkaterNumber))
            return [];

        SetState(state with
        {
            Skaters = state.Skaters.Append(@event.Body.SkaterNumber).ToArray(),
            QueuedSkaters = state.QueuedSkaters.Except([@event.Body.SkaterNumber]).ToArray(),
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterReleasedFromBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Skaters.All(s => s != @event.Body.SkaterNumber))
            return [];

        SetState(state with
        {
            Skaters = state.Skaters.Except([@event.Body.SkaterNumber]).ToArray(),
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSubstitutedInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Skater {number} substituted by {substituteNumber} for {team} team", @event.Body.OriginalSkaterNumber, @event.Body.NewSkaterNumber, teamSide);

        var state = GetState();

        SetState(state with
        {
            Skaters = state.Skaters.Select(s => s == @event.Body.OriginalSkaterNumber ? @event.Body.NewSkaterNumber : s).ToArray()
        });

        return [];
    });
}

public sealed class HomePenaltyBox(ReducerGameContext context, ILogger<HomePenaltyBox> logger) : PenaltyBox(TeamSide.Home, context, logger);
public sealed class AwayPenaltyBox(ReducerGameContext context, ILogger<AwayPenaltyBox> logger) : PenaltyBox(TeamSide.Away, context, logger);

public sealed record PenaltyBoxState(string[] Skaters, string[] QueuedSkaters)
{
    public bool Equals(PenaltyBoxState? other) =>
        other is not null
        && other.Skaters.OrderBy(s => s).SequenceEqual(Skaters.OrderBy(s => s))
        && other.QueuedSkaters.OrderBy(s => s).SequenceEqual(QueuedSkaters.OrderBy(s => s));

    public override int GetHashCode() => HashCode.Combine(Skaters, QueuedSkaters);
}
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public abstract class PenaltyBox(TeamSide teamSide, ReducerGameContext context)
    : Reducer<PenaltyBoxState>(context)
    , IHandlesEvent<SkaterSatInBox>
    , IHandlesEvent<SkaterReleasedFromBox>
    , IHandlesEvent<SkaterSubstitutedInBox>
{
    protected override PenaltyBoxState DefaultState => new([]);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Skaters.Any(s => s == @event.Body.SkaterNumber))
            return [];

        SetState(new(state.Skaters.Append(@event.Body.SkaterNumber).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterReleasedFromBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Skaters.All(s => s != @event.Body.SkaterNumber))
            return [];

        SetState(new(state.Skaters.Except([@event.Body.SkaterNumber]).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSubstitutedInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(
            state.Skaters.Select(s => s == @event.Body.OriginalSkaterNumber ? @event.Body.NewSkaterNumber : s).ToArray()
        ));

        return [];
    });
}

public sealed class HomePenaltyBox(ReducerGameContext context) : PenaltyBox(TeamSide.Home, context);
public sealed class AwayPenaltyBox(ReducerGameContext context) : PenaltyBox(TeamSide.Away, context);

public sealed record PenaltyBoxState(string[] Skaters)
{
    public bool Equals(PenaltyBoxState? other) =>
        other is not null
        && other.Skaters.SequenceEqual(Skaters);

    public override int GetHashCode() => Skaters.GetHashCode();
}
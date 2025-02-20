using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;

namespace amethyst.Reducers;

public abstract class PenaltySheet(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<PenaltySheetState>(context)
    , IHandlesEvent<TeamSet>
    , IHandlesEvent<PenaltyAssessed>
    , IHandlesEvent<PenaltyRescinded>
    , IHandlesEvent<SkaterSatInBox>
    , IDependsOnState<GameStageState>
{
    protected override PenaltySheetState DefaultState => new([]);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(TeamSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(state.Lines.Where(l => @event.Body.Team.Roster.Any(s => s.Number == l.SkaterNumber))
            .Concat(@event.Body.Team.Roster
                .Where(s => state.Lines.All(l => l.SkaterNumber != s.Number))
                .Select(s => new PenaltySheetLine(s.Number, [])))
            .OrderBy(l => l.SkaterNumber)
            .ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(PenaltyAssessed @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber);

        if (skaterPenalties == null)
            return [];

        var newPenalties = skaterPenalties.Penalties.Append(new(@event.Body.PenaltyCode, gameStage.PeriodNumber, gameStage.JamNumber, false)).ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with { Penalties =  newPenalties } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(PenaltyRescinded @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber);

        if (skaterPenalties == null)
            return [];

        var newPenalties = skaterPenalties.Penalties.Where(
                p => p.Period != @event.Body.Period
                     || p.Jam != @event.Body.Jam
                     || p.Code != @event.Body.PenaltyCode)
            .ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with { Penalties = newPenalties } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber);

        if (skaterPenalties == null)
            return [];

        var newPenalties = skaterPenalties.Penalties.Select(p => p with { Served = true }).ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with { Penalties = newPenalties } : l).ToArray()));

        return [];
    });
}

public sealed record PenaltySheetState(PenaltySheetLine[] Lines)
{
    public bool Equals(PenaltySheetState? other) =>
        other is not null
        && other.Lines.SequenceEqual(Lines);

    public override int GetHashCode() => Lines.GetHashCode();
}

public sealed record PenaltySheetLine(string SkaterNumber, Penalty[] Penalties)
{
    public bool Equals(PenaltySheetLine? other) =>
        other is not null
        && other.SkaterNumber.Equals(SkaterNumber)
        && other.Penalties.SequenceEqual(Penalties);

    public override int GetHashCode() => HashCode.Combine(SkaterNumber, Penalties);
}
public sealed record Penalty(string Code, int Period, int Jam, bool Served);

public sealed class HomePenaltySheet(ReducerGameContext context, ILogger<HomePenaltySheet> logger) : PenaltySheet(TeamSide.Home, context, logger);
public sealed class AwayPenaltySheet(ReducerGameContext context, ILogger<HomePenaltySheet> logger) : PenaltySheet(TeamSide.Away, context, logger);

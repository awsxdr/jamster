using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class LineupSheet(TeamSide teamSide, ReducerGameContext context) 
    : Reducer<LineupSheetState>(context)
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<SkaterAddedToJam>
    , IHandlesEvent<SkaterRemovedFromJam>
    , IDependsOnState<GameStageState>
{
    protected override LineupSheetState DefaultState => new([new(1, 1, null, null, [null, null, null])]);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        SetState(new(state.Jams.Append(new(gameStage.PeriodNumber, gameStage.JamNumber + 1, null, null, [null, null, null])).ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(SkaterAddedToJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.Period, @event.Body.Jam, jam => jam with
        {
            JammerNumber = @event.Body.Position == SkaterPosition.Jammer ? @event.Body.SkaterNumber : jam.JammerNumber == @event.Body.SkaterNumber ? null : jam.JammerNumber,
            PivotNumber = @event.Body.Position == SkaterPosition.Pivot ? @event.Body.SkaterNumber : jam.PivotNumber == @event.Body.SkaterNumber ? null : jam.PivotNumber,
            BlockerNumbers = jam.BlockerNumbers
                    .Where(n => n != null)
                    .Except([@event.Body.SkaterNumber])
                    .Map(x => @event.Body.Position == SkaterPosition.Blocker ? x.Append(@event.Body.SkaterNumber) : x)
                    .TakeLast(3)
                    .Pad(3, null)
                    .ToArray()
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterRemovedFromJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.Period, @event.Body.Jam, jam => jam with
        {
            JammerNumber = jam.JammerNumber == @event.Body.SkaterNumber ? null : jam.JammerNumber,
            PivotNumber = jam.PivotNumber == @event.Body.SkaterNumber ? null : jam.PivotNumber,
            BlockerNumbers = jam.BlockerNumbers
                .Where(n => n != null)
                .Except([@event.Body.SkaterNumber])
                .Pad(3, null)
                .ToArray()
        });

        return [];
    });

    private void ModifyJam(int period, int jam, Func<LineupSheetJam, LineupSheetJam> mapper)
    {
        var state = GetState();

        var totalJamNumber = state.Jams.Select((lineupJam, totalJamNumber) => (Jam: lineupJam, TotalJamNumber: totalJamNumber)).Where(j => j.Jam.Jam == jam && j.Jam.Period == period).Select(j => j.TotalJamNumber).ToArray();

        if (!totalJamNumber.Any())
            return;

        ModifyJam(totalJamNumber[0], mapper);
    }

    private void ModifyJam(int totalJamNumber, Func<LineupSheetJam, LineupSheetJam> mapper)
    {
        var state = GetState();

        if (totalJamNumber >= state.Jams.Length || totalJamNumber < 0)
            return;

        SetState(new(
            state.Jams.Take(totalJamNumber)
                .Append(mapper(state.Jams[totalJamNumber]))
                .Concat(state.Jams.Skip(totalJamNumber + 1))
                .ToArray()));
    }
}

public sealed class HomeLineupSheet(ReducerGameContext context) : LineupSheet(TeamSide.Home, context);
public sealed class AwayLineupSheet(ReducerGameContext context) : LineupSheet(TeamSide.Away, context);

public sealed record LineupSheetState(LineupSheetJam[] Jams)
{
    public bool Equals(LineupSheetState? other) =>
        other != null
        && other.Jams.SequenceEqual(Jams);

    public override int GetHashCode() => Jams.GetHashCode();
}

public sealed record LineupSheetJam(
    int Period,
    int Jam,
    string? JammerNumber,
    string? PivotNumber,
    string?[] BlockerNumbers)
{
    public bool Equals(LineupSheetJam? other) =>
        other != null
        && other.Period.Equals(Period)
        && other.Jam.Equals(Jam)
        && (other.JammerNumber?.Equals(JammerNumber) ?? JammerNumber == null)
        && (other.PivotNumber?.Equals(PivotNumber) ?? PivotNumber == null)
        && other.BlockerNumbers.SequenceEqual(BlockerNumbers);

    public override int GetHashCode() => HashCode.Combine(Period, Jam, JammerNumber, PivotNumber, BlockerNumbers);
}
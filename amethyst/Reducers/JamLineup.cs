using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class JamLineup(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<JamLineupState>(context)
    , IHandlesEvent<SkaterOnTrack>
    , IHandlesEvent<SkaterAddedToJam>
    , IHandlesEvent<SkaterOffTrack>
    , IHandlesEvent<SkaterRemovedFromJam>
    , IHandlesEvent<JamEnded>
    , IDependsOnState<GameStageState>
{
    protected override JamLineupState DefaultState => new(null, null, [null, null, null]);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterOnTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        return [new SkaterAddedToJam(@event.Tick, new(teamSide, gameStage.PeriodNumber, gameStage.JamNumber + (gameStage.Stage == Stage.Jam ? 0 : 1), @event.Body.SkaterNumber, @event.Body.Position))];
    });

    public IEnumerable<Event> Handle(SkaterAddedToJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        if (
            @event.Body.Period != gameStage.PeriodNumber
            || (gameStage.Stage == Stage.Jam && @event.Body.Jam != gameStage.JamNumber)
            || (gameStage.Stage != Stage.Jam && @event.Body.Jam != gameStage.JamNumber + 1)
        )
        {
            return [];
        }

        logger.LogDebug("Setting {position} for {team} team to {skaterId}", @event.Body.Position, teamSide, @event.Body.SkaterNumber);

        var state = GetState();

        var stateWithSkaterRemoved = new JamLineupState(
            state.JammerNumber == @event.Body.SkaterNumber ? null : state.JammerNumber,
            state.PivotNumber == @event.Body.SkaterNumber ? null : state.PivotNumber,
            state.BlockerNumbers.Except([@event.Body.SkaterNumber]).Pad(3, null).ToArray());

        SetState(@event.Body.Position switch
        {
            SkaterPosition.Jammer => stateWithSkaterRemoved with { JammerNumber = @event.Body.SkaterNumber },
            SkaterPosition.Pivot => stateWithSkaterRemoved with { PivotNumber = @event.Body.SkaterNumber },
            SkaterPosition.Blocker => stateWithSkaterRemoved with
            {
                BlockerNumbers = stateWithSkaterRemoved.BlockerNumbers
                    .Where(s => s != null)
                    .Append(@event.Body.SkaterNumber)
                    .TakeLast(3)
                    .Pad(3, null)
                    .ToArray()
            },
            _ => state
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterOffTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        return [new SkaterRemovedFromJam(@event.Tick, new(teamSide, gameStage.PeriodNumber, gameStage.JamNumber + (gameStage.Stage == Stage.Jam ? 0 : 1), @event.Body.SkaterNumber))];
    });

    public IEnumerable<Event> Handle(SkaterRemovedFromJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        if (
            @event.Body.Period != gameStage.PeriodNumber
            || (gameStage.Stage == Stage.Jam && @event.Body.Jam != gameStage.JamNumber)
            || (gameStage.Stage != Stage.Jam && @event.Body.Jam != gameStage.JamNumber + 1)
        )
        {
            return [];
        }

        logger.LogDebug("Removing skater {skaterNumber} from track for {team} team", @event.Body.SkaterNumber, teamSide);

        var state = GetState();

        SetState(new(
            state.JammerNumber == @event.Body.SkaterNumber ? null : state.JammerNumber,
            state.PivotNumber == @event.Body.SkaterNumber ? null : state.PivotNumber,
            state.BlockerNumbers.Except([@event.Body.SkaterNumber]).Pad(3, null).ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Clearing jam lineup for {team} team due to jam end", teamSide);

        SetState(DefaultState);

        return [];
    }
}

public sealed record JamLineupState(string? JammerNumber, string? PivotNumber, string?[] BlockerNumbers)
{
    public bool Equals(JamLineupState? other) =>
        other is not null
        && (other.JammerNumber?.Equals(JammerNumber) ?? JammerNumber is null)
        && (other.PivotNumber?.Equals(PivotNumber) ?? PivotNumber is null)
        && other.BlockerNumbers.SequenceEqual(BlockerNumbers);

    public override int GetHashCode() => 
        HashCode.Combine(JammerNumber, PivotNumber, BlockerNumbers);
}

public sealed class HomeTeamJamLineup(ReducerGameContext context, ILogger<HomeTeamJamLineup> logger) : JamLineup(TeamSide.Home, context, logger);
public sealed class AwayTeamJamLineup(ReducerGameContext context, ILogger<AwayTeamJamLineup> logger) : JamLineup(TeamSide.Away, context, logger);

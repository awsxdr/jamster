using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public abstract class JamLineup(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<JamLineupState>(context)
    , IHandlesEvent<SkaterOnTrack>
    , IHandlesEvent<SkaterAddedToJam>
    , IHandlesEvent<SkaterOffTrack>
    , IHandlesEvent<SkaterRemovedFromJam>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<SkaterSubstitutedInBox>
    , IHandlesEvent<PenaltyAssessed>
    , IDependsOnState<GameStageState>
    , IDependsOnState<PenaltyBoxState>
    , IDependsOnState<TeamJamStatsState>
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
            SkaterPosition.Pivot => stateWithSkaterRemoved with
            {
                PivotNumber = @event.Body.SkaterNumber,
                BlockerNumbers = stateWithSkaterRemoved.BlockerNumbers
                    .Where(s => s != null)
                    .TakeLast(stateWithSkaterRemoved.PivotNumber is not null || @event.Body.Position == SkaterPosition.Pivot ? 3 : 4)
                    .Pad(3, null)
                    .ToArray(),
            },
            SkaterPosition.Blocker => stateWithSkaterRemoved with
            {
                BlockerNumbers = stateWithSkaterRemoved.BlockerNumbers
                    .Where(s => s != null)
                    .Append(@event.Body.SkaterNumber)
                    .TakeLast(stateWithSkaterRemoved.PivotNumber is not null || @event.Body.Position == SkaterPosition.Pivot ? 3 : 4)
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

        var state = GetState();
        var penaltyBox = GetKeyedState<PenaltyBoxState>(teamSide.ToString());

        SetState(DefaultState);

        return penaltyBox.Skaters.Select(s => new SkaterOnTrack(@event.Tick, new(
            teamSide, 
            s,
            state.JammerNumber == s ? SkaterPosition.Jammer
                : state.PivotNumber == s ? SkaterPosition.Pivot
                : SkaterPosition.Blocker)));
    }

    public IEnumerable<Event> Handle(SkaterSubstitutedInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var position =
            state.JammerNumber == @event.Body.OriginalSkaterNumber ? SkaterPosition.Jammer
            : state.PivotNumber == @event.Body.OriginalSkaterNumber ? SkaterPosition.Pivot
            : state.BlockerNumbers.Contains(@event.Body.OriginalSkaterNumber) ? SkaterPosition.Blocker
            : (SkaterPosition?)null;

        if (position is null)
            return [];

        logger.LogDebug("Skater {oldNumber} substituted in box by {newNumber}", @event.Body.OriginalSkaterNumber, @event.Body.NewSkaterNumber);

        return
        [
            new SkaterOffTrack(@event.Tick, new(teamSide, @event.Body.OriginalSkaterNumber)),
            new SkaterOnTrack(@event.Tick, new(teamSide, @event.Body.NewSkaterNumber, (SkaterPosition)position)),
        ];
    });

    public IEnumerable<Event> Handle(PenaltyAssessed @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (@event.Body.SkaterNumber != state.JammerNumber)
            return [];

        var opponentStats = GetKeyedState<TeamJamStatsState>(teamSide == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home));

        if (opponentStats.Lead)
            return [];

        return [new LostMarked(@event.Id, new(teamSide, true))];
    });
}

public sealed record JamLineupState(string? JammerNumber, string? PivotNumber, string?[] BlockerNumbers)
{
    public bool Contains(string number) =>
        JammerNumber == number
        || PivotNumber == number
        || BlockerNumbers.Contains(number);

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

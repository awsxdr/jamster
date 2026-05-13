using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class JamLineup(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<JamLineupState>(context)
    , IHandlesEvent<SkaterOnTrack>
    , IHandlesEvent<SkaterAddedToJam>
    , IHandlesEvent<SkaterOffTrack>
    , IHandlesEvent<SkaterRemovedFromJam>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<SkaterSubstitutedInBox>
    , IHandlesEvent<PenaltyAssessed>
    , IHandlesEvent<SkaterSatInBox>
    , IDependsOnState<GameStageState>
    , IDependsOnState<PenaltyBoxState>
    , IDependsOnState<PenaltySheetState>
    , IDependsOnState<TeamJamStatsState>
    , IDependsOnState<OvertimeState>
{
    protected override JamLineupState DefaultState => new(null, null, [null, null, null]);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterOnTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        return [new SkaterAddedToJam(@event.Tick, new(teamSide, gameStage.PeriodNumber, gameStage.JamNumber + (gameStage.Stage == Stage.Jam ? 0 : 1), @event.Body.SkaterId, @event.Body.Position))];
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

        logger.LogDebug("Setting {position} for {team} team to {skaterId}", @event.Body.Position, teamSide, @event.Body.SkaterId);

        var state = GetState();

        var stateWithSkaterRemoved = new JamLineupState(
            state.JammerId == @event.Body.SkaterId ? null : state.JammerId,
            state.PivotId == @event.Body.SkaterId ? null : state.PivotId,
            state.BlockerIds.Except([@event.Body.SkaterId]).Pad(3, null).ToArray());

        SetState(@event.Body.Position switch
        {
            SkaterPosition.Jammer => stateWithSkaterRemoved with { JammerId = @event.Body.SkaterId },
            SkaterPosition.Pivot => stateWithSkaterRemoved with
            {
                PivotId = @event.Body.SkaterId,
                BlockerIds = stateWithSkaterRemoved.BlockerIds
                    .Where(s => s != null)
                    .TakeLast(stateWithSkaterRemoved.PivotId is not null || @event.Body.Position == SkaterPosition.Pivot ? 3 : 4)
                    .OrderBy(x => x)
                    .Pad(3, null)
                    .ToArray(),
            },
            SkaterPosition.Blocker => stateWithSkaterRemoved with
            {
                BlockerIds = stateWithSkaterRemoved.BlockerIds
                    .Where(s => s != null)
                    .Append(@event.Body.SkaterId)
                    .TakeLast(stateWithSkaterRemoved.PivotId is not null || @event.Body.Position == SkaterPosition.Pivot ? 3 : 4)
                    .OrderBy(x => x)
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

        return [new SkaterRemovedFromJam(@event.Tick, new(teamSide, gameStage.PeriodNumber, gameStage.JamNumber + (gameStage.Stage == Stage.Jam ? 0 : 1), @event.Body.SkaterId))];
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

        logger.LogDebug("Removing skater {SkaterId} from track for {team} team", @event.Body.SkaterId, teamSide);

        var state = GetState();

        SetState(new(
            state.JammerId == @event.Body.SkaterId ? null : state.JammerId,
            state.PivotId == @event.Body.SkaterId ? null : state.PivotId,
            state.BlockerIds.Except([@event.Body.SkaterId]).Pad(3, null).ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Clearing jam lineup for {team} team due to jam end", teamSide);

        var state = GetState();
        var penaltyBox = GetKeyedState<PenaltyBoxState>(teamSide.ToString());

        SetState(DefaultState);

        return penaltyBox.QueuedSkaters.Concat(penaltyBox.Skaters).Select(s => new SkaterOnTrack(@event.Tick, new(
            teamSide, 
            s,
            state.JammerId == s ? SkaterPosition.Jammer
                : state.PivotId == s ? SkaterPosition.Pivot
                : SkaterPosition.Blocker)))
            .ToArray();
    }

    public IEnumerable<Event> Handle(SkaterSubstitutedInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var position =
            state.JammerId == @event.Body.OriginalSkaterId ? SkaterPosition.Jammer
            : state.PivotId == @event.Body.OriginalSkaterId ? SkaterPosition.Pivot
            : state.BlockerIds.Contains(@event.Body.OriginalSkaterId) ? SkaterPosition.Blocker
            : (SkaterPosition?)null;

        if (position is null)
            return [];

        logger.LogDebug("Skater {oldNumber} substituted in box by {newNumber}", @event.Body.OriginalSkaterId, @event.Body.NewSkaterId);

        return
        [
            new SkaterOffTrack(@event.Tick, new(teamSide, @event.Body.OriginalSkaterId)),
            new SkaterOnTrack(@event.Tick, new(teamSide, @event.Body.NewSkaterId, (SkaterPosition)position)),
        ];
    });

    public IEnumerable<Event> Handle(PenaltyAssessed @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (!state.SkaterIds.Contains(@event.Body.SkaterId))
        {
            logger.LogDebug("Penalty assessed when skater {number} on {team} team not in current lineup. Adding to lineup.", @event.Body.SkaterId, teamSide);
            return [new PreviousJamSkaterOnTrack(@event.Tick, new(teamSide, @event.Body.SkaterId))];
        }

        var overtime = GetState<OvertimeState>();
        if (overtime.IsInOvertime)
            return [];

        if (@event.Body.SkaterId != state.JammerId)
            return [];

        var opponentStats = GetKeyedState<TeamJamStatsState>(teamSide == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home));

        if (opponentStats.Lead)
            return [];

        logger.LogDebug("Marking lost for jammer on {team} team due to penalty", teamSide);
        return [new LostMarked(@event.Tick, new(teamSide, true))];
    });

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (!state.SkaterIds.Contains(@event.Body.SkaterId))
        {
            logger.LogDebug("Box entry when skater {number} on {team} team not in current lineup. Adding to lineup.", @event.Body.SkaterId, teamSide);
            return [new PreviousJamSkaterOnTrack(@event.Tick, new(teamSide, @event.Body.SkaterId))];
        }

        var overtime = GetState<OvertimeState>();
        if (overtime.IsInOvertime)
            return [];

        if (@event.Body.SkaterId != state.JammerId)
            return [];

        var opponentStats = GetKeyedState<TeamJamStatsState>(teamSide == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home));

        if (opponentStats.Lead)
            return [];

        logger.LogDebug("Marking lost for jammer on {team} team due to sat in box", teamSide);
        return [new LostMarked(@event.Tick, new(teamSide, true))];
    });
}

public sealed record JamLineupState(Guid? JammerId, Guid? PivotId, Guid?[] BlockerIds)
{
    public bool Contains(Guid id) =>
        JammerId == id
        || PivotId == id
        || BlockerIds.Contains(id);

    public Guid?[] SkaterIds => [JammerId, PivotId, .. BlockerIds];

    public bool Equals(JamLineupState? other) =>
        other is not null
        && (other.JammerId?.Equals(JammerId) ?? JammerId is null)
        && (other.PivotId?.Equals(PivotId) ?? PivotId is null)
        && other.BlockerIds.OrderBy(n => n).SequenceEqual(BlockerIds.OrderBy(n => n));

    public override int GetHashCode() => 
        HashCode.Combine(JammerId, PivotId, BlockerIds);
}

public sealed class HomeTeamJamLineup(ReducerGameContext context, ILogger<HomeTeamJamLineup> logger) : JamLineup(TeamSide.Home, context, logger);
public sealed class AwayTeamJamLineup(ReducerGameContext context, ILogger<AwayTeamJamLineup> logger) : JamLineup(TeamSide.Away, context, logger);

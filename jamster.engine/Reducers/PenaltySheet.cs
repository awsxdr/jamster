using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class PenaltySheet(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<PenaltySheetState>(context)
    , IHandlesEvent<TeamSet>
    , IHandlesEvent<PenaltyAssessed>
    , IHandlesEvent<PenaltyRescinded>
    , IHandlesEvent<PenaltyUpdated>
    , IHandlesEvent<SkaterExpelled>
    , IHandlesEvent<ExpulsionCleared>
    , IHandlesEvent<SkaterSatInBox>
    , IHandlesEvent<PenaltyServedSet>
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
                .Select(s => new PenaltySheetLine(s.Number, null, [])))
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
        {
            logger.LogWarning("Attempting to assess penalty but skater {skater} on {team} team was not found", @event.Body.SkaterNumber, teamSide);
            return [];
        }

        var newPenalties = skaterPenalties.Penalties.Append(new(@event.Body.PenaltyCode, gameStage.PeriodNumber, Math.Max(1, gameStage.JamNumber), false)).ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with { Penalties =  newPenalties } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(PenaltyRescinded @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)?.Penalties;

        if (skaterPenalties == null)
        {
            logger.LogWarning("Attempting to rescind penalty but skater {skater} on {team} team was not found", @event.Body.SkaterNumber, teamSide);
            return [];
        }

        var newPenalties = skaterPenalties
            .GroupBy(p => (p.Period, p.Jam, p.Code))
            .SelectMany(g =>
                g.Key == (@event.Body.Period, @event.Body.Jam, @event.Body.PenaltyCode)
                    ? g.Take(g.Count() - 1)
                    : g
            )
            .OrderBy(p => p.Period)
            .ThenBy(p => p.Jam)
            .ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with
        {
            Penalties = newPenalties,
            ExpulsionPenalty = 
                newPenalties.Any(p => p.Period == @event.Body.Period && p.Jam == @event.Body.Jam && p.Code == @event.Body.PenaltyCode)
                ? l.ExpulsionPenalty
                : null,
        } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(PenaltyUpdated @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)?.Penalties;

        if (skaterPenalties == null)
        {
            logger.LogWarning("Attempting to update penalty but skater {skater} on {team} team was not found", @event.Body.SkaterNumber, teamSide);
            return [];
        }

        var targetPenalty = skaterPenalties.FirstOrDefault(p =>
            p.Period == @event.Body.OriginalPeriod
            && p.Jam == @event.Body.OriginalJam
            && p.Code == @event.Body.OriginalPenaltyCode);

        if (targetPenalty == null)
        {
            logger.LogWarning("Attempting to update penalty but penalty with code {code} in period {period} jam {jam} for skater {skater} on {team} team was not found", @event.Body.OriginalPenaltyCode, @event.Body.OriginalPeriod, @event.Body.OriginalJam, @event.Body.SkaterNumber, teamSide);
            return [];
        }

        var modifiedPenalty = targetPenalty with
        {
            Code = @event.Body.NewPenaltyCode,
            Period = @event.Body.NewPeriod,
            Jam = @event.Body.NewJam,
        };

        logger.LogDebug("Changing penalty for skater {skaterNumber} from {originalCode} in period {originalPeriod} jam {originalJam} to {newCode} in period {newPeriod} jam {newJam}", @event.Body.SkaterNumber, @event.Body.OriginalPenaltyCode, @event.Body.OriginalPeriod, @event.Body.OriginalJam, @event.Body.NewPenaltyCode, @event.Body.NewPeriod, @event.Body.NewJam);

        var newPenalties = skaterPenalties
            .GroupBy(p => (p.Period, p.Jam, p.Code))
            .SelectMany(g =>
                g.Key == (targetPenalty.Period, targetPenalty.Jam, targetPenalty.Code)
                    ? g.Take(g.Count() - 1)
                    : g
            )
            .Append(modifiedPenalty)
            .OrderBy(p => p.Period)
            .ThenBy(p => p.Jam)
            .ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with
        {
            Penalties = newPenalties,
            ExpulsionPenalty =
                l.ExpulsionPenalty is null || newPenalties.Any(p => p.Period == @event.Body.OriginalPeriod && p.Jam == @event.Body.OriginalJam && p.Code == @event.Body.OriginalPenaltyCode)
                    ? l.ExpulsionPenalty
                    : modifiedPenalty,
        } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterExpelled @event) => @event.HandleIfTeam(teamSide, () => 
    {
        var state = GetState();

        var existingPenalty = state.Lines
            .SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)
            ?.Penalties
            .FirstOrDefault(p => p.Period == @event.Body.Period && p.Jam == @event.Body.Jam && p.Code == @event.Body.PenaltyCode);

        if (existingPenalty is null)
            return [];

        logger.LogDebug("Skater {skaterNumber} from {team} team expelled", @event.Body.SkaterNumber, teamSide);

        SetState(new(state.Lines
            .Select(l =>
                l.SkaterNumber == @event.Body.SkaterNumber
                ? l with { ExpulsionPenalty = existingPenalty }
                : l
            )
            .ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(ExpulsionCleared @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Lines.All(l => l.SkaterNumber != @event.Body.SkaterNumber))
            return [];

        logger.LogDebug("Expulsion removed for skater {skater} on {team} team", @event.Body.SkaterNumber, teamSide);

        SetState(new(state.Lines
            .Select(l =>
                l.SkaterNumber == @event.Body.SkaterNumber
                ? l with { ExpulsionPenalty = null }
                : l
            )
            .ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber);

        if (skaterPenalties == null)
            return [];

        var newPenalties = skaterPenalties.Penalties.Select(p => p with { Served = true }).ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with
        {
            Penalties = newPenalties,
            ExpulsionPenalty = l.ExpulsionPenalty is null ? null : l.ExpulsionPenalty with { Served = true },
        } : l).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(PenaltyServedSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var skaterPenalties = state.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)?.Penalties;

        if (skaterPenalties == null)
        {
            logger.LogWarning("Attempting to update penalty served status but skater {skater} on {team} team was not found", @event.Body.SkaterNumber, teamSide);
            return [];
        }

        var targetPenalty = skaterPenalties.FirstOrDefault(p =>
            p.Period == @event.Body.Period
            && p.Jam == @event.Body.Jam
            && p.Code == @event.Body.PenaltyCode);

        if (targetPenalty == null)
        {
            logger.LogWarning("Attempting to update penalty served status but penalty with code {code} in period {period} jam {jam} for skater {skater} on {team} team was not found", @event.Body.PenaltyCode, @event.Body.Period, @event.Body.Jam, @event.Body.SkaterNumber, teamSide);
            return [];
        }

        logger.LogDebug("Setting penalty served status for skater {skaterNumber} on {team} team for penalty {code} in period {period} jam {jam} to {served}", @event.Body.SkaterNumber, teamSide, @event.Body.PenaltyCode, @event.Body.Period, @event.Body.Jam, @event.Body.Served);

        var modifiedPenalty = targetPenalty with { Served = @event.Body.Served };

        var newPenalties = skaterPenalties
            .GroupBy(p => (p.Period, p.Jam, p.Code))
            .SelectMany(g =>
                g.Key == (targetPenalty.Period, targetPenalty.Jam, targetPenalty.Code)
                    ? g.Take(g.Count() - 1)
                    : g
            )
            .Append(modifiedPenalty)
            .OrderBy(p => p.Period)
            .ThenBy(p => p.Jam)
            .ToArray();

        SetState(new(state.Lines.Select(l => l.SkaterNumber == @event.Body.SkaterNumber ? l with
        {
            Penalties = newPenalties,
            ExpulsionPenalty =
            l.ExpulsionPenalty is null || newPenalties.Any(p => p.Period == @event.Body.Period && p.Jam == @event.Body.Jam && p.Code == @event.Body.PenaltyCode)
                ? l.ExpulsionPenalty
                : modifiedPenalty,
        } : l).ToArray()));

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

public sealed record PenaltySheetLine(string SkaterNumber, Penalty? ExpulsionPenalty, Penalty[] Penalties)
{
    public bool Equals(PenaltySheetLine? other) =>
        other is not null
        && other.SkaterNumber.Equals(SkaterNumber)
        && (other.ExpulsionPenalty?.Equals(ExpulsionPenalty) ?? ExpulsionPenalty is null)
        && other.Penalties.SequenceEqual(Penalties);

    public override int GetHashCode() => HashCode.Combine(SkaterNumber, Penalties);
}
public sealed record Penalty(string Code, int Period, int Jam, bool Served);

public sealed class HomePenaltySheet(ReducerGameContext context, ILogger<HomePenaltySheet> logger) : PenaltySheet(TeamSide.Home, context, logger);
public sealed class AwayPenaltySheet(ReducerGameContext context, ILogger<HomePenaltySheet> logger) : PenaltySheet(TeamSide.Away, context, logger);

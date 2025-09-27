using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class Injuries(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<InjuriesState>(context)
    , IHandlesEvent<SkaterInjuryAdded>
    , IHandlesEvent<SkaterInjuryRemoved>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<PeriodFinalized>
    , IDependsOnState<GameStageState>
    , IDependsOnState<RulesState>
{
    protected override InjuriesState DefaultState => new([]);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterInjuryAdded @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();

        var state = GetState();

        var isDuplicate = state.Injuries.Any(i =>
            i.SkaterNumber == @event.Body.SkaterNumber
            && i.Period == gameStage.PeriodNumber
            && i.Jam == gameStage.JamNumber);

        if (isDuplicate)
            return [];

        logger.LogDebug("Adding injury to skater {skater} on {team} team in period {period} jam {jam}", @event.Body.SkaterNumber, teamSide, gameStage.PeriodNumber, gameStage.JamNumber);

        SetState(new(GetState().Injuries
            .Append(new(@event.Body.SkaterNumber, gameStage.PeriodNumber, gameStage.JamNumber, gameStage.TotalJamNumber, false))
            .ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(SkaterInjuryRemoved @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.Injuries.All(i => i.SkaterNumber != @event.Body.SkaterNumber || i.TotalJamNumberStart != @event.Body.TotalJamNumberStart))
            return [];

        logger.LogDebug("Removing injury for skater {skater} on {team} team in total jam number {jam}", @event.Body.SkaterNumber, teamSide, @event.Body.TotalJamNumberStart);

        SetState(new(state.Injuries.Where(i => i.SkaterNumber != @event.Body.SkaterNumber || i.TotalJamNumberStart != @event.Body.TotalJamNumberStart).ToArray()));

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();
        var rules = GetState<RulesState>().Rules;
        var gameStage = GetState<GameStageState>();

        SetState(new(
            state.Injuries.Select(i =>
            {
                if (i.Expired)
                    return i;

                var injuriesThisPeriod = state.Injuries.Count(i2 => i2.SkaterNumber == i.SkaterNumber && i2.Period == gameStage.PeriodNumber);
                var maximumInjuriesPerPeriodReached = injuriesThisPeriod >= rules.InjuryRules.NumberOfAllowableInjuriesPerPeriod;

                if (maximumInjuriesPerPeriodReached)
                    return i;

                if (gameStage.TotalJamNumber < i.TotalJamNumberStart + rules.InjuryRules.JamsToSitOutFollowingInjury)
                    return i;

                logger.LogDebug("Injury expired for {skater} on {team} team", i.SkaterNumber, teamSide);
                return i with { Expired = true };
            })
            .ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        var state = GetState();
        var rules = GetState<RulesState>().Rules;
        var gameStage = GetState<GameStageState>();

        SetState(new(
            state.Injuries.Select(i =>
            {
                if (i.Expired)
                    return i;

                return i with { Expired = gameStage.TotalJamNumber >= i.TotalJamNumberStart + rules.InjuryRules.JamsToSitOutFollowingInjury };
            })
            .ToArray()
        ));

        return [];
    }
}

public sealed record InjuriesState(Injury[] Injuries)
{
    public bool Equals(InjuriesState? other) =>
        other is not null
        && other.Injuries.SequenceEqual(Injuries);

    public override int GetHashCode() => Injuries.GetHashCode();
}

public sealed record Injury(string SkaterNumber, int Period, int Jam, int TotalJamNumberStart, bool Expired);

public sealed class HomeInjuries(ReducerGameContext context, ILogger<HomeInjuries> logger) : Injuries(TeamSide.Home, context, logger);
public sealed class AwayInjuries(ReducerGameContext context, ILogger<AwayInjuries> logger) : Injuries(TeamSide.Away, context, logger);
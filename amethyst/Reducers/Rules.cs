using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class Rules(ReducerGameContext context) 
    : Reducer<RulesState>(context)
    , IHandlesEvent<RulesetSet>
{
    public static readonly Ruleset DefaultRules = new(
        PeriodRules: new(
            PeriodCount: 2,
            Duration: Tick.FromSeconds(30 * 60),
            PeriodEndBehavior: PeriodEndBehavior.AnytimeOutsideJam),
        JamRules: new(
            ResetJamNumbersBetweenPeriods: true,
            Duration: Tick.FromSeconds(2 * 60)),
        LineupRules: new(
            Duration: Tick.FromSeconds(30),
            OvertimeDuration: Tick.FromSeconds(60)),
        TimeoutRules: new(
            TeamTimeoutDuration: Tick.FromSeconds(60),
            PeriodClockBehavior: TimeoutPeriodClockStopBehavior.All,
            TeamTimeoutAllowance: 3,
            ResetBehavior: TimeoutResetBehavior.Never),
        PenaltyRules: new(
            FoulOutPenaltyCount: 7),
        IntermissionRules: new(
            Duration: Tick.FromSeconds(15 * 60))
    );

    protected override RulesState DefaultState => new(DefaultRules);

    public IEnumerable<Event> Handle(RulesetSet @event)
    {
        SetState(new(@event.Body.Rules));

        return [];
    }
}

public record RulesState(Ruleset Rules);
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class Rules(ReducerGameContext context, ILogger<Rules> logger) 
    : Reducer<RulesState>(context)
    , IHandlesEvent<RulesetSet>
{
    public static readonly Ruleset DefaultRules = new(
        PeriodRules: new(
            PeriodCount: 2,
            DurationInSeconds: 30 * 60,
            PeriodEndBehavior: PeriodEndBehavior.AnytimeOutsideJam),
        JamRules: new(
            ResetJamNumbersBetweenPeriods: true,
            DurationInSeconds: 2 * 60),
        LineupRules: new(
            DurationInSeconds: 30,
            OvertimeDurationInSeconds: 60),
        TimeoutRules: new(
            TeamTimeoutDurationInSeconds: 60,
            PeriodClockBehavior: TimeoutPeriodClockStopBehavior.All,
            TeamTimeoutAllowance: 3,
            ResetBehavior: TimeoutResetBehavior.Never),
        PenaltyRules: new(
            FoulOutPenaltyCount: 7),
        IntermissionRules: new(
            DurationInSeconds: 15 * 60)
    );

    protected override RulesState DefaultState => new(DefaultRules);

    public IEnumerable<Event> Handle(RulesetSet @event)
    {
        var rules = @event.Body.Rules;

        SetState(new(new(
            PeriodRules: new(
                PeriodCount: Constrain(rules.PeriodRules.PeriodCount, 1, 20),
                DurationInSeconds: Constrain(rules.PeriodRules.DurationInSeconds, 1, 24 * 60 * 60),
                PeriodEndBehavior: rules.PeriodRules.PeriodEndBehavior),
            JamRules: new(
                ResetJamNumbersBetweenPeriods: rules.JamRules.ResetJamNumbersBetweenPeriods,
                DurationInSeconds: Constrain(rules.JamRules.DurationInSeconds, 1, 60 * 60)),
            LineupRules: new(
                DurationInSeconds: Constrain(rules.LineupRules.DurationInSeconds, 1, 60 * 60),
                OvertimeDurationInSeconds: Constrain(rules.LineupRules.OvertimeDurationInSeconds, 1, 60 * 60)),
            TimeoutRules: new(
                TeamTimeoutDurationInSeconds: Constrain(rules.TimeoutRules.TeamTimeoutDurationInSeconds, 1, 60 * 60),
                PeriodClockBehavior: rules.TimeoutRules.PeriodClockBehavior,
                TeamTimeoutAllowance: Constrain(rules.TimeoutRules.TeamTimeoutAllowance, 0, 10),
                ResetBehavior: rules.TimeoutRules.ResetBehavior),
            PenaltyRules: new(
                FoulOutPenaltyCount: Constrain(rules.PenaltyRules.FoulOutPenaltyCount, 1, 20)),
            IntermissionRules: new(
                DurationInSeconds: Constrain(rules.IntermissionRules.DurationInSeconds, 1, 24 * 60 * 60))
        )));

        return [];
    }

    private static int Constrain(int value, int min, int max) =>
        Math.Min(max, Math.Max(min, value));
}

public record RulesState(Ruleset Rules);
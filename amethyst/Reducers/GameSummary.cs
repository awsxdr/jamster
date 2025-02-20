using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class GameSummary(ReducerGameContext context)
    : Reducer<GameSummaryState>(context)
    , IHandlesEvent<RulesetSet>
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<PenaltyAssessed>
    , IHandlesEvent<PenaltyRescinded>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<PeriodFinalized>
    , IDependsOnState<GameStageState>
    , IDependsOnState<PenaltySheetState>
{
    protected override GameSummaryState DefaultState => new(GameProgress.Upcoming, new([], 0), new([], 0), new([], 0), new([], 0), Enumerable.Repeat(0, Rules.DefaultRules.PeriodRules.PeriodCount).ToArray());

    public IEnumerable<Event> Handle(RulesetSet @event)
    {
        var state = GetState();

        SetState(state with
        {
            HomeScore = state.HomeScore with { PeriodTotals = GetNewPeriodArray(state.HomeScore.PeriodTotals) },
            HomePenalties = state.HomePenalties with { PeriodTotals = GetNewPeriodArray(state.HomePenalties.PeriodTotals) },
            AwayScore = state.AwayScore with { PeriodTotals = GetNewPeriodArray(state.AwayScore.PeriodTotals) },
            AwayPenalties = state.AwayPenalties with { PeriodTotals = GetNewPeriodArray(state.AwayPenalties.PeriodTotals) },
            PeriodJamCounts = GetNewPeriodArray(state.PeriodJamCounts),
        });

        return [];

        int[] GetNewPeriodArray(int[] current) =>
            current.Concat(Enumerable.Repeat(0, Math.Max(0, @event.Body.Rules.PeriodRules.PeriodCount - state.HomeScore.PeriodTotals.Length))).ToArray();
    }

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event)
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        SetState(state with
        {
            HomeScore = @event.Body.TeamSide == TeamSide.Home
                ? new(
                    state.HomeScore.PeriodTotals.Select((s ,i) => i == gameStage.PeriodNumber - 1 ? s + @event.Body.Value : s).ToArray(),
                    state.HomeScore.GrandTotal + @event.Body.Value)
                : state.HomeScore,
            AwayScore = @event.Body.TeamSide == TeamSide.Away
                ? new(
                    state.AwayScore.PeriodTotals.Select((s, i) => i == gameStage.PeriodNumber - 1 ? s + @event.Body.Value : s).ToArray(),
                    state.AwayScore.GrandTotal + @event.Body.Value)
                : state.AwayScore,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PenaltyAssessed @event)
    {
        var state = GetState();
        var penaltySheet = GetKeyedState<PenaltySheetState>(@event.Body.TeamSide.ToString());
        var skaterPenalties = penaltySheet.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)?.Penalties;

        if (skaterPenalties == null)
            return [];

        var newPeriodCounts = new PenaltySummary(
            state.HomePenalties.PeriodTotals.Select((_, i) => skaterPenalties.Count(p => p.Period == i + 1)).ToArray(),
            skaterPenalties.Length);

        SetState(state with
        {
            HomePenalties = @event.Body.TeamSide == TeamSide.Home ? newPeriodCounts : state.HomePenalties,
            AwayPenalties = @event.Body.TeamSide == TeamSide.Away ? newPeriodCounts : state.AwayPenalties,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PenaltyRescinded @event)
    {
        var state = GetState();
        var penaltySheet = GetKeyedState<PenaltySheetState>(@event.Body.TeamSide.ToString());
        var skaterPenalties = penaltySheet.Lines.SingleOrDefault(l => l.SkaterNumber == @event.Body.SkaterNumber)?.Penalties;

        if (skaterPenalties == null)
            return [];

        var newPeriodCounts = new PenaltySummary(
            state.HomePenalties.PeriodTotals.Select((_, i) => skaterPenalties.Count(p => p.Period == i + 1)).ToArray(),
            skaterPenalties.Length);

        SetState(state with
        {
            HomePenalties = @event.Body.TeamSide == TeamSide.Home ? newPeriodCounts : state.HomePenalties,
            AwayPenalties = @event.Body.TeamSide == TeamSide.Away ? newPeriodCounts : state.AwayPenalties,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        var jamCounts = state.PeriodJamCounts;
        jamCounts[gameStage.PeriodNumber - 1] = gameStage.JamNumber;

        SetState(state with
        {
            GameProgress = GameProgress.InProgress,
            PeriodJamCounts = jamCounts,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        var gameStage = GetState<GameStageState>();

        if (gameStage.Stage != Stage.AfterGame)
            return [];

        SetState(GetState() with { GameProgress = GameProgress.Finished });

        return [];
    }
}

public sealed record GameSummaryState(
    GameProgress GameProgress,
    ScoreSummary HomeScore,
    ScoreSummary AwayScore,
    PenaltySummary HomePenalties,
    PenaltySummary AwayPenalties,
    int[] PeriodJamCounts);

public enum GameProgress
{
    Upcoming,
    InProgress,
    Finished,
}

public sealed record ScoreSummary(int[] PeriodTotals, int GrandTotal);
public sealed record PenaltySummary(int[] PeriodTotals, int GrandTotal);
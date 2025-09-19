using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using FluentAssertions;
using static jamster.engine.tests.DataGenerator;
using GameSummary = jamster.Reducers.GameSummary;

namespace jamster.engine.tests.Reducers;

public class GameSummaryUnitTests : ReducerUnitTest<GameSummary, GameSummaryState>
{
    [TestCase(0, 2)]
    [TestCase(1, 2)]
    [TestCase(3, 2)]
    [TestCase(5, 2)]
    [TestCase(5, 1)]
    public async Task RulesetSet_CreatesExtractPeriodsWhereNeeded(int initialPeriodCount, int newPeriodCount)
    {
        State = new(
            GameProgress.InProgress,
            new(GetRandomIntArray(initialPeriodCount, min: 0, max: 200), 0),
            new(GetRandomIntArray(initialPeriodCount, min: 0, max: 200), 0),
            new(GetRandomIntArray(initialPeriodCount, min: 0, max: 30), 0),
            new(GetRandomIntArray(initialPeriodCount, min: 0, max: 30), 0),
            GetRandomIntArray(initialPeriodCount, min: 5, max: 25));

        var originalState = State;

        await Subject.Handle(new RulesetSet(0,
            new(Rules.DefaultRules with
            {
                PeriodRules = Rules.DefaultRules.PeriodRules with { PeriodCount = newPeriodCount }
            })));

        var expectedPeriodCount = Math.Max(initialPeriodCount, newPeriodCount);
        State.HomeScore.PeriodTotals.Should().HaveCount(expectedPeriodCount).And.StartWith(originalState.HomeScore.PeriodTotals);
        State.AwayScore.PeriodTotals.Should().HaveCount(expectedPeriodCount).And.StartWith(originalState.AwayScore.PeriodTotals);
        State.HomePenalties.PeriodTotals.Should().HaveCount(expectedPeriodCount).And.StartWith(originalState.HomePenalties.PeriodTotals);
        State.AwayPenalties.PeriodTotals.Should().HaveCount(expectedPeriodCount).And.StartWith(originalState.AwayPenalties.PeriodTotals);
        State.PeriodJamCounts.Should().HaveCount(expectedPeriodCount).And.StartWith(originalState.PeriodJamCounts);
    }

    [Test]
    public async Task ScoreModifiedRelative_AddsPointsToCurrentPeriodForCorrectTeam([Values] TeamSide team, [Values(1, 5, -1, -5, 0)] int scoreChange, [Values(1, 2)] int period)
    {
        var testScoreSummary = new ScoreSummary([50, 50], 100);
        State = new(
            GameProgress.InProgress,
            team == TeamSide.Home ? testScoreSummary : new([0, 0], 0),
            team == TeamSide.Away ? testScoreSummary : new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            [10, 10]
        );
        MockState<GameStageState>(new(Stage.Jam, period, 10, 10, false));

        await Subject.Handle(new ScoreModifiedRelative(0, new(team, scoreChange)));

        var scoreSummary = team == TeamSide.Home ? State.HomeScore : State.AwayScore;

        scoreSummary.PeriodTotals[period - 1].Should().Be(50 + scoreChange);
        scoreSummary.GrandTotal.Should().Be(100 + scoreChange);
    }

    [Test]
    public async Task PenaltyAssessed_UpdatesPenaltyCounts([Values] TeamSide team, [Values(1, 2)] int period)
    {
        State = new(
            GameProgress.InProgress,
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            [10, 10]
        );
        MockState<GameStageState>(new(Stage.Jam, period, 10, 10, false));
        MockKeyedState<PenaltySheetState>(team.ToString(), new([
            new("123", null, [new("X", 1, 3, true), new("X", 1, 6, true), new("X", 1, 9, true), new("X", 2, 3, true), new("X", 2, 6, true), new("X", 2, 9, true)])
        ]));
        MockKeyedState<PenaltySheetState>(team == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home), new([]));

        await Subject.Handle(new PenaltyAssessed(0, new(team, "123", "X")));

        var penaltySummary = team == TeamSide.Home ? State.HomePenalties : State.AwayPenalties;

        penaltySummary.PeriodTotals[period - 1].Should().Be(3);
        penaltySummary.GrandTotal.Should().Be(6);
    }

    [Test]
    public async Task PenaltyRescinded_UpdatesPenaltyCounts([Values] TeamSide team, [Values(1, 2)] int period)
    {
        State = new(
            GameProgress.InProgress,
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            [10, 10]
        );
        MockState<GameStageState>(new(Stage.Jam, period, 10, 10, false));
        MockKeyedState<PenaltySheetState>(team.ToString(), new([
            new("123", null, [new("X", 1, 3, true), new("X", 1, 6, true), new("X", 1, 9, true), new("X", 2, 3, true), new("X", 2, 6, true), new("X", 2, 9, true)])
        ]));
        MockKeyedState<PenaltySheetState>(team == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home), new([]));

        await Subject.Handle(new PenaltyRescinded(0, new(team, "123", "X", 2, 6)));

        var penaltySummary = team == TeamSide.Home ? State.HomePenalties : State.AwayPenalties;

        penaltySummary.PeriodTotals[period - 1].Should().Be(3);
        penaltySummary.GrandTotal.Should().Be(6);
    }

    [Test]
    public async Task PenaltyUpdated_UpdatesPenaltyCounts()
    {
        State = new(
            GameProgress.InProgress,
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            new([0, 0], 0),
            [10, 10]
        );
        MockKeyedState<PenaltySheetState>(nameof(TeamSide.Home), new([
            new("123", null, [new("X", 1, 3, true), new("X", 1, 6, true), new("X", 1, 9, true), new("X", 2, 3, true), new("X", 2, 6, true), new("X", 2, 9, true)])
        ]));
        MockKeyedState<PenaltySheetState>(nameof(TeamSide.Away), new([]));

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, "123", "?", 1, 1, "?", 1, 1)));

        State.HomePenalties.Should().Be(new PenaltySummary([3, 3], 6));
    }

    [Test]
    public async Task JamStarted_WhenGameIsUpcoming_SetsGameToInProgress()
    {
        State = new(GameProgress.Upcoming, new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), [0, 0]);
        MockState<GameStageState>(new(Stage.Jam, 1, 10, 20, false));

        await Subject.Handle(new JamStarted(1000));

        State.GameProgress.Should().Be(GameProgress.InProgress);
    }

    [Test]
    public async Task JamStarted_UpdatesJamCounts([Values(1, 2)] int period)
    {
        State = new(GameProgress.InProgress, new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), [0, 0]);
        MockState<GameStageState>(new(Stage.Jam, period, 10, 20, false));

        await Subject.Handle(new JamStarted(1000));

        State.PeriodJamCounts[period - 1].Should().Be(10);
    }

    [Test]
    public async Task PeriodFinalized_WhenAfterGame_SetsGameToFinished()
    {
        State = new(GameProgress.InProgress, new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), [0, 0]);
        MockState<GameStageState>(new(Stage.AfterGame, 2, 20, 40, true));

        await Subject.Handle(new PeriodFinalized(1000));

        State.GameProgress.Should().Be(GameProgress.Finished);
    }
}
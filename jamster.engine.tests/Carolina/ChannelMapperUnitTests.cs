using FluentAssertions;

using jamster.engine.Carolina;
using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests.Carolina;

public class ChannelMapperUnitTests : UnitTest<ChannelMapper>
{
    private readonly GameInfo _game = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Test Game");

    private string Key(string suffix) => $"ScoreBoard.Game({_game.Id}).{suffix}";

    private static StateSnapshot BuildSnapshot(
        GameStageState? gameStageState = null,
        OvertimeState? overtimeState = null,
        JamClockState? jamClockState = null,
        PeriodClockState? periodClockState = null,
        LineupClockState? lineupClockState = null,
        TimeoutClockState? timeoutClockState = null,
        IntermissionClockState? intermissionClockState = null,
        PostGameClockState? postGameClockState = null,
        CurrentTimeoutTypeState? currentTimeoutTypeState = null,
        TimeoutListState? timeoutListState = null,
        RulesState? rulesState = null,
        TeamStateSnapshot<TeamScoreState>? teamScoreState = null,
        TeamStateSnapshot<TripScoreState>? tripScoreState = null,
        TeamStateSnapshot<TeamJamStatsState>? teamJamStatsState = null,
        TeamStateSnapshot<TeamTimeoutsState>? teamTimeoutsState = null,
        TeamStateSnapshot<JamLineupState>? jamLineupState = null,
        TeamStateSnapshot<TeamDetailsState>? teamDetailsState = null,
        TeamStateSnapshot<ScoreSheetState>? scoreSheetState = null,
        TeamStateSnapshot<PenaltySheetState>? penaltySheetState = null,
        TeamStateSnapshot<LineupSheetState>? lineupSheetState = null,
        TeamStateSnapshot<PenaltyBoxState>? penaltyBoxState = null,
        TeamStateSnapshot<BoxTripsState>? boxTripsState = null,
        TeamStateSnapshot<InjuriesState>? injuriesState = null)
    =>
        new(
            gameStageState ?? GameStageState.Default,
            overtimeState ?? new OvertimeState(false),
            jamClockState ?? new JamClockState(false, 0, 0, true, false),
            periodClockState ?? new PeriodClockState(false, false, false, 0, 0, 0),
            lineupClockState ?? new LineupClockState(false, 0, 0),
            timeoutClockState ?? TimeoutClockState.Default,
            intermissionClockState ?? new IntermissionClockState(false, false, 0, 0, 0),
            postGameClockState ?? PostGameClockState.Default,
            currentTimeoutTypeState ?? new CurrentTimeoutTypeState(TimeoutType.Untyped, null),
            timeoutListState ?? new TimeoutListState([]),
            rulesState ?? new RulesState(Rules.DefaultRules),
            teamScoreState ?? BothTeams(new TeamScoreState(0, 0)),
            tripScoreState ?? BothTeams(new TripScoreState(null, 0, 0)),
            teamJamStatsState ?? BothTeams(new TeamJamStatsState(false, false, false, false, false)),
            teamTimeoutsState ?? BothTeams(new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            jamLineupState ?? BothTeams(new JamLineupState(null, null, [null, null, null])),
            teamDetailsState ?? BothTeams(TeamDetailsState.Default),
            scoreSheetState ?? BothTeams(ScoreSheetState.Default),
            penaltySheetState ?? BothTeams(new PenaltySheetState([])),
            lineupSheetState ?? BothTeams(new LineupSheetState([])),
            penaltyBoxState ?? BothTeams(new PenaltyBoxState([], [])),
            boxTripsState ?? BothTeams(new BoxTripsState([], false)),
            injuriesState ?? BothTeams(new InjuriesState([]))
        );

    private static TeamStateSnapshot<T> BothTeams<T>(T state) => new(state, state);

    private static StateSnapshot BuildSnapshotForStage(Stage stage)
    {
        var timeoutList = stage is Stage.Timeout or Stage.AfterTimeout
            ? new TimeoutListState([new TimeoutListItem(Guid7.Empty, TimeoutType.Untyped, 1, 1, null, null, false)])
            : new TimeoutListState([]);

        return BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = stage },
            timeoutListState: timeoutList);
    }

    [TestCase(Stage.Jam, true)]
    [TestCase(Stage.Lineup, false)]
    [TestCase(Stage.BeforeGame, false)]
    [TestCase(Stage.Intermission, false)]
    [TestCase(Stage.AfterGame, false)]
    public void InJam_ReflectsCurrentStage(Stage stage, bool expected)
    {
        var snapshot = BuildSnapshot(gameStageState: GameStageState.Default with { Stage = stage });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("InJam")].Should().Be(expected);
    }

    [Test]
    public void InJam_FalseWhenInTimeout()
    {
        var snapshot = BuildSnapshotForStage(Stage.Timeout);

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("InJam")].Should().Be(false);
    }

    [TestCase(Stage.Jam, true)]
    [TestCase(Stage.Lineup, true)]
    [TestCase(Stage.BeforeGame, true)]
    [TestCase(Stage.Intermission, false)]
    [TestCase(Stage.AfterGame, false)]
    public void InPeriod_FalseOnlyDuringIntermissionAndAfterGame(Stage stage, bool expected)
    {
        var snapshot = BuildSnapshot(gameStageState: GameStageState.Default with { Stage = stage });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("InPeriod")].Should().Be(expected);
    }

    [TestCase(Stage.Timeout, true)]
    [TestCase(Stage.AfterTimeout, true)]
    public void InPeriod_TrueDuringTimeout(Stage stage, bool expected)
    {
        var snapshot = BuildSnapshotForStage(stage);

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("InPeriod")].Should().Be(expected);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void InOvertime_ReflectsOvertimeState(bool isInOvertime)
    {
        var snapshot = BuildSnapshot(overtimeState: new OvertimeState(isInOvertime));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("InOvertime")].Should().Be(isInOvertime);
    }

    [TestCase(Stage.AfterGame, true, true)]
    [TestCase(Stage.AfterGame, false, false)]
    [TestCase(Stage.Jam, true, false)]
    [TestCase(Stage.Lineup, true, false)]
    public void OfficialScore_TrueOnlyWhenAfterGameAndPeriodFinalized(Stage stage, bool periodFinalized, bool expected)
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = stage, PeriodIsFinalized = periodFinalized });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("OfficialScore")].Should().Be(expected);
    }

    [TestCase(Stage.BeforeGame, false, "Prepared")]
    [TestCase(Stage.Jam, false, "In progress")]
    [TestCase(Stage.Lineup, false, "In progress")]
    [TestCase(Stage.Intermission, false, "In progress")]
    [TestCase(Stage.AfterGame, false, "In progress")]
    [TestCase(Stage.AfterGame, true, "Finished")]
    public void State_MapsCorrectly(Stage stage, bool periodFinalized, string expected)
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = stage, PeriodIsFinalized = periodFinalized });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("State")].Should().Be(expected);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void NoMoreJam_IsInverseOfNextJamShouldStart(bool nextJamShouldStart, bool expected)
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { NextJamShouldStart = nextJamShouldStart });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("NoMoreJam")].Should().Be(expected);
    }

    [Test]
    public void OfficialReview_TrueWhenTimeoutClockRunningAndTypeIsReview()
    {
        var snapshot = BuildSnapshot(
            timeoutClockState: TimeoutClockState.Default with { IsRunning = true },
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Review, TeamSide.Home));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("OfficialReview")].Should().Be(true);
    }

    [Test]
    public void OfficialReview_FalseWhenTimeoutClockNotRunning()
    {
        var snapshot = BuildSnapshot(
            timeoutClockState: TimeoutClockState.Default with { IsRunning = false },
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Review, TeamSide.Home));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("OfficialReview")].Should().Be(false);
    }

    [Test]
    public void OfficialReview_FalseWhenTimeoutTypeIsNotReview()
    {
        var snapshot = BuildSnapshot(
            timeoutClockState: TimeoutClockState.Default with { IsRunning = true },
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Official, null));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("OfficialReview")].Should().Be(false);
    }

    [Test]
    public void CurrentTimeout_IsNoTimeoutWhenNotInTimeout()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Lineup },
            timeoutListState: new TimeoutListState([]));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("CurrentTimeout")].Should().Be("noTimeout");
    }

    [Test]
    public void CurrentTimeout_IsLastTimeoutIdWhenInTimeout()
    {
        var timeoutId = Guid7.Empty;
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Timeout },
            timeoutListState: new TimeoutListState(
            [
                new TimeoutListItem(timeoutId, TimeoutType.Untyped, 1, 1, null, null, false)
            ]));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("CurrentTimeout")].Should().Be(timeoutId);
    }

    [Test]
    public void TimeoutOwner_IsEmptyWhenNoTimeout()
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Untyped, null));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("TimeoutOwner")].Should().Be("");
    }

    [Test]
    public void TimeoutOwner_IsOForOfficialTimeout()
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Official, null));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("TimeoutOwner")].Should().Be("O");
    }

    [Test]
    public void TimeoutOwner_IsGameId1ForHomeTeamTimeout()
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Team, TeamSide.Home));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("TimeoutOwner")].Should().Be($"{_game.Id}_1");
    }

    [Test]
    public void TimeoutOwner_IsGameId2ForAwayTeamTimeout()
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Team, TeamSide.Away));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("TimeoutOwner")].Should().Be($"{_game.Id}_2");
    }

    [Test]
    public void TimeoutOwner_IsGameId2ForAwayReview()
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Review, TeamSide.Away));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("TimeoutOwner")].Should().Be($"{_game.Id}_2");
    }

    [Test]
    public void JamClock_DirectionIsTrue()
    {
        var snapshot = BuildSnapshot();

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Jam).Direction")].Should().Be(true);
    }

    [Test]
    public void JamClock_RunningReflectsClockState()
    {
        var snapshot = BuildSnapshot(
            jamClockState: new JamClockState(true, 0, 0, true, false));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Jam).Running")].Should().Be(true);
    }

    [Test]
    public void JamClock_TimeReflectsTicksPassed()
    {
        var ticks = Tick.FromSeconds(45);
        var snapshot = BuildSnapshot(
            jamClockState: new JamClockState(true, 0, ticks, true, false));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Jam).Time")].Should().Be(ticks.Millseconds);
    }

    [Test]
    public void JamClock_MaximumTimeIsRulesDuration()
    {
        var rules = Rules.DefaultRules;
        var snapshot = BuildSnapshot(rulesState: new RulesState(rules));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Jam).MaximumTime")].Should().Be(rules.JamRules.DurationInSeconds * 1000);
    }

    [Test]
    public void PeriodClock_DirectionIsFalse()
    {
        var snapshot = BuildSnapshot();

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Period).Direction")].Should().Be(false);
    }

    [Test]
    public void PeriodClock_RunningReflectsClockState()
    {
        var snapshot = BuildSnapshot(
            periodClockState: new PeriodClockState(true, false, true, 0, 0, 0));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Period).Running")].Should().Be(true);
    }

    [Test]
    public void LineupClock_DirectionIsFalse()
    {
        var snapshot = BuildSnapshot();

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Lineup).Direction")].Should().Be(false);
    }

    [Test]
    public void TimeoutClock_DirectionIsFalse()
    {
        var snapshot = BuildSnapshot();

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Timeout).Direction")].Should().Be(false);
    }

    [Test]
    public void TimeoutClock_MaximumTimeIsRulesDurationForTeamTimeout()
    {
        var rules = Rules.DefaultRules;
        var snapshot = BuildSnapshot(
            rulesState: new RulesState(rules),
            currentTimeoutTypeState: new CurrentTimeoutTypeState(TimeoutType.Team, TeamSide.Home));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Timeout).MaximumTime")].Should().Be(rules.TimeoutRules.TeamTimeoutDurationInSeconds * 1000);
    }

    [TestCase(TimeoutType.Official)]
    [TestCase(TimeoutType.Review)]
    [TestCase(TimeoutType.Untyped)]
    public void TimeoutClock_MaximumTimeIs24HoursForNonTeamTimeout(TimeoutType type)
    {
        var snapshot = BuildSnapshot(
            currentTimeoutTypeState: new CurrentTimeoutTypeState(type, null));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Timeout).MaximumTime")].Should().Be(24 * 60 * 60 * 1000);
    }

    [TestCase(Stage.BeforeGame, true)]
    [TestCase(Stage.Lineup, true)]
    [TestCase(Stage.Jam, true)]
    [TestCase(Stage.Intermission, true)]
    [TestCase(Stage.AfterGame, false)]
    public void IntermissionClock_DirectionIsFalseOnlyAfterGame(Stage stage, bool expected)
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = stage });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Intermission).Direction")].Should().Be(expected);
    }

    [Test]
    public void IntermissionClock_RunningWhenEitherIntermissionOrPostGameClockIsRunning()
    {
        var snapshot = BuildSnapshot(
            postGameClockState: PostGameClockState.Default with { IsRunning = true });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Clock(Intermission).Running")].Should().Be(true);
    }

    [Test]
    public void HomeTeamScore_ReflectsTeamScoreState()
    {
        var snapshot = BuildSnapshot(
            teamScoreState: new TeamStateSnapshot<TeamScoreState>(
                new TeamScoreState(125, 5),
                new TeamScoreState(100, 3)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).Score")].Should().Be(125);
    }

    [Test]
    public void AwayTeamScore_ReflectsTeamScoreState()
    {
        var snapshot = BuildSnapshot(
            teamScoreState: new TeamStateSnapshot<TeamScoreState>(
                new TeamScoreState(125, 5),
                new TeamScoreState(100, 3)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(2).Score")].Should().Be(100);
    }

    [Test]
    public void HomeTeamTimeouts_IsAllowanceMinusNumberTaken()
    {
        var rules = Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with { TeamTimeoutAllowance = 3 }
        };
        var snapshot = BuildSnapshot(
            rulesState: new RulesState(rules),
            teamTimeoutsState: new TeamStateSnapshot<TeamTimeoutsState>(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.None),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).Timeouts")].Should().Be(2);
    }

    [Test]
    public void HomeTeamOfficialReviews_IsZeroWhenReviewUsed()
    {
        var snapshot = BuildSnapshot(
            teamTimeoutsState: new TeamStateSnapshot<TeamTimeoutsState>(
                new TeamTimeoutsState(0, ReviewStatus.Used, TimeoutInUse.None),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).OfficialReviews")].Should().Be(0);
    }

    [Test]
    public void HomeTeamOfficialReviews_IsOneWhenReviewUnused()
    {
        var snapshot = BuildSnapshot(
            teamTimeoutsState: new TeamStateSnapshot<TeamTimeoutsState>(
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).OfficialReviews")].Should().Be(1);
    }

    [Test]
    public void HomeTeamOfficialReviews_IsOneWhenReviewRetained()
    {
        var snapshot = BuildSnapshot(
            teamTimeoutsState: new TeamStateSnapshot<TeamTimeoutsState>(
                new TeamTimeoutsState(0, ReviewStatus.Retained, TimeoutInUse.None),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).OfficialReviews")].Should().Be(1);
    }

    [Test]
    public void HomeTeamInitials_AreFirstLettersOfDisplayName()
    {
        var team = new GameTeam(
            new Dictionary<string, string> { ["league"] = "Gotham City Rollers" },
            TeamDetailsState.Default.Team.Color,
            []);

        var snapshot = BuildSnapshot(
            teamDetailsState: new TeamStateSnapshot<TeamDetailsState>(
                new TeamDetailsState(team),
                TeamDetailsState.Default));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).Initials")].Should().Be("GCR");
    }

    [Test]
    public void RetainedOfficialReview_TrueWhenReviewStatusIsRetained()
    {
        var snapshot = BuildSnapshot(
            teamTimeoutsState: new TeamStateSnapshot<TeamTimeoutsState>(
                new TeamTimeoutsState(0, ReviewStatus.Retained, TimeoutInUse.None),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Team(1).RetainedOfficialReview")].Should().Be(true);
    }

    [Test]
    public void CurrentPeriodNumber_ReflectsGameStageState()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { PeriodNumber = 2 });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("CurrentPeriodNumber")].Should().Be(2);
    }

    [Test]
    public void Period_NotRunningForDifferentPeriodNumber()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Jam, PeriodNumber = 1 });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Period(0).Running")].Should().Be(false);
    }

    [Test]
    public void Period_NotRunningAfterGame()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.AfterGame, PeriodNumber = 1 });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Period(1).Running")].Should().Be(false);
    }

    [Test]
    public void Period_RunningWhenInJam()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Jam, PeriodNumber = 1 });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("Period(1).Running")].Should().Be(true);
    }

    [Test]
    public void UpcomingJamNumber_IsLastScoreSheetJamPlusOne()
    {
        var jam = new ScoreSheetJam(1, 3, "1234", "5678", false, false, false, false, true, [], null, 0, 0, false);
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Lineup, JamNumber = 5, TotalJamNumber = 5 },
            scoreSheetState: new TeamStateSnapshot<ScoreSheetState>(
                new ScoreSheetState([jam]),
                new ScoreSheetState([jam])));

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("UpcomingJamNumber")].Should().Be(4);
    }

    [Test]
    public void CurrentJamNumber_IsJamNumberPlusOneWhenNotInJam()
    {
        var snapshot = BuildSnapshot(
            gameStageState: GameStageState.Default with { Stage = Stage.Lineup, JamNumber = 3 });

        var result = Subject.MapGameStates(snapshot, _game);

        result[Key("CurrentPeriodNumber")].Should().Be(1);
    }
}

using System.Timers;

using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Carolina;

public record TeamStateSnapshot<TState>(TState HomeState, TState AwayState)
{
    public TState this[TeamSide side] => side == TeamSide.Home ? HomeState : AwayState;
}

public record StateSnapshot(
    GameStageState GameStageState,
    OvertimeState OvertimeState,
    JamClockState JamClockState,
    PeriodClockState PeriodClockState,
    LineupClockState LineupClockState,
    TimeoutClockState TimeoutClockState,
    IntermissionClockState IntermissionClockState,
    PostGameClockState PostGameClockState,
    CurrentTimeoutTypeState CurrentTimeoutTypeState,
    TimeoutListState TimeoutListState,
    RulesState RulesState,
    TeamStateSnapshot<TeamScoreState> TeamScoreState,
    TeamStateSnapshot<TripScoreState> TripScoreState,
    TeamStateSnapshot<TeamJamStatsState> TeamJamStatsState,
    TeamStateSnapshot<TeamTimeoutsState> TeamTimeoutsState,
    TeamStateSnapshot<JamLineupState> JamLineupState,
    TeamStateSnapshot<TeamDetailsState> TeamDetailsState,
    TeamStateSnapshot<ScoreSheetState> ScoreSheetState,
    TeamStateSnapshot<PenaltySheetState> PenaltySheetState,
    TeamStateSnapshot<LineupSheetState> LineupSheetState,
    TeamStateSnapshot<PenaltyBoxState> PenaltyBoxState,
    TeamStateSnapshot<BoxTripsState> BoxTripsState,
    TeamStateSnapshot<InjuriesState> InjuriesState
);

public interface IChannelMapper
{
    Dictionary<string, object> MapGameStates(StateSnapshot snapshot, GameInfo game);
}

public class ChannelMapper : IChannelMapper
{
    public Dictionary<string, object> MapGameStates(StateSnapshot snapshot, GameInfo game)
    {
        var result = new Dictionary<string, object>();

        result[$"ScoreBoard.Game({game.Id}).AbortReason"] = "";

        MapIntermissionClock();
        MapJamClock();
        MapLineupClock();
        MapPeriodClock();
        MapTimeoutClock();

        result[$"ScoreBoard.Game({game.Id}).ClockDuringFinalScore"] = false;
        result[$"ScoreBoard.Game({game.Id}).CurrentPeriod"] = GetPeriodId(snapshot.GameStageState.PeriodNumber);
        result[$"ScoreBoard.Game({game.Id}).CurrentPeriodNumber"] = snapshot.GameStageState.PeriodNumber;
        result[$"ScoreBoard.Game({game.Id}).CurrentTimeout"] = 
            snapshot.GameStageState.Stage is Stage.Timeout or Stage.AfterTimeout
            ? snapshot.TimeoutListState.Timeouts[^1].EventId
            : "noTimeout";
        // Skipped: EventInfo(Date)
        // Skipped: EventInfo(StartTime)
        result[$"ScoreBoard.Game({game.Id}).ExportBlockedBy"] = "";
        // Skipped: Filename
        result[$"ScoreBoard.Game({game.Id}).Id"] = game.Id;
        result[$"ScoreBoard.Game({game.Id}).InJam"] = snapshot.GameStageState.Stage == Stage.Jam;
        result[$"ScoreBoard.Game({game.Id}).InOvertime"] = snapshot.OvertimeState.IsInOvertime;
        result[$"ScoreBoard.Game({game.Id}).InPeriod"] = snapshot.GameStageState.Stage is not (Stage.Intermission or Stage.AfterGame);
        result[$"ScoreBoard.Game({game.Id}).InSuddenScoring"] = false;
        result[$"ScoreBoard.Game({game.Id}).InhibitFinalScore"] = false;
        result[$"ScoreBoard.Game({game.Id}).InjuryContinuationUpcoming"] = false;

        var (currentJamNumber, currentTotalJamNumber) = snapshot.GameStageState.Stage == Stage.Jam
            ? (snapshot.GameStageState.JamNumber, snapshot.GameStageState.TotalJamNumber)
            : (snapshot.GameStageState.JamNumber + 1, snapshot.GameStageState.TotalJamNumber + 1);
        MapJam($"ScoreBoard.Game({game.Id}).Jam({currentJamNumber})", currentTotalJamNumber);

        result[$"ScoreBoard.Game({game.Id}).JsonExists"] = false;
        // Skipped: Label(...)
        result[$"ScoreBoard.Game({game.Id}).LastFileUpdate"] = "Never";
        result[$"ScoreBoard.Game({game.Id}).Name"] = game.Name;
        // Skipped: NameFormat
        result[$"ScoreBoard.Game({game.Id}).NoMoreJam"] = !snapshot.GameStageState.NextJamShouldStart;
        result[$"ScoreBoard.Game({game.Id}).OfficialReview"] = 
            snapshot.TimeoutClockState.IsRunning &&
            snapshot.CurrentTimeoutTypeState.Type == TimeoutType.Review;
        result[$"ScoreBoard.Game({game.Id}).OfficialScore"] = 
            snapshot.GameStageState.Stage == Stage.AfterGame && snapshot.GameStageState.PeriodIsFinalized;
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(?)"] = "Unknown";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(A)"] = "High Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(B)"] = "Back Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(C)"] = "Illegal Contact,Illegal Assist,OOP Block,Early/Late Hit";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(D)"] = "Direction,Stop Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(E)"] = "Leg Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(F)"] = "Forearm";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(G)"] = "Misconduct,Insubordination";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(H)"] = "Head Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(I)"] = "Illegal Procedure,Star Pass Violation,Pass Interference";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(L)"] = "Low Block";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(M)"] = "Multiplayer";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(N)"] = "Interference,Delay Of Game";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(P)"] = "Illegal Position,Destruction,Skating OOB,Failure to...";
        result[$"ScoreBoard.Game({game.Id}).PenaltyCode(X)"] = "Cut,Illegal Re-Entry";

        for (var i = 0; i <= snapshot.GameStageState.PeriodNumber; ++i)
        {
            MapPeriod(i);
        }

        result[$"ScoreBoard.Game({game.Id}).Readonly"] = false;
        result[$"ScoreBoard.Game({game.Id}).ReviewIsTo"] = false;

        MapRules();

        result[$"ScoreBoard.Game({game.Id}).State"] = snapshot.GameStageState switch
        {
            { Stage: Stage.BeforeGame } => "Prepared",
            { Stage: Stage.AfterGame, PeriodIsFinalized: true } => "Finished",
            _ => "In progress"
        };
        result[$"ScoreBoard.Game({game.Id}).StatsbookExists"] = false;
        result[$"ScoreBoard.Game({game.Id}).SuspensionsServed"] = "";

        MapTeam(TeamSide.Home);
        MapTeam(TeamSide.Away);

        result[$"ScoreBoard.Game({game.Id}).TimeoutOwner"] = snapshot.CurrentTimeoutTypeState switch
        {
            { Type: TimeoutType.Official } => "O",
            { Type: TimeoutType.Review or TimeoutType.Team, TeamSide: TeamSide.Home } => $"{game.Id}_1",
            { Type: TimeoutType.Review or TimeoutType.Team, TeamSide: TeamSide.Away } => $"{game.Id}_2",
            _ => ""
        };
        result[$"ScoreBoard.Game({game.Id}).UpcomingJam"] = GetJamId(currentTotalJamNumber + 1);
        result[$"ScoreBoard.Game({game.Id}).UpcomingJamNumber"] =
            (snapshot.ScoreSheetState.HomeState.Jams.LastOrDefault()?.Jam ?? 0) + 1;
        result[$"ScoreBoard.Game({game.Id}).UpdateInProgress"] = false;

        result["ScoreBoard.Settings.Setting(ScoreBoard.View_CurrentView)"] = "scoreboard";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_BoxStyle)"] = "";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_SidePadding)"] = "";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_SwapTeams)"] = "false";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_HideLogos)"] = "false";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_HidePenaltyClocks)"] = "false";
        result["ScoreBoard.Settings.Setting(ScoreBoard.HideLineups)"] = "false";
        result["ScoreBoard.Settings.Setting(ScoreBoard.View_ClockAfterTimeout)"] = "";

        result["ScoreBoard.Version(release)"] = "v2025.10";
        result["ScoreBoard.Version(release.commit)"] = "b9a33ac56d7274f93c2889d22214f28d56017605";
        result["ScoreBoard.Version(release.host)"] = "localhost";
        result["ScoreBoard.Version(release.time)"] = "20260420190905";
        result["ScoreBoard.Version(release.user)"] = "frank";

        return result;

        void MapIntermissionClock()
        {
            var maximumTime = snapshot.GameStageState.Stage switch
            {
                Stage.Intermission => snapshot.RulesState.Rules.IntermissionRules.DurationInSeconds * 1000,
                _ => 24 * 60 * 60 * 1000
            };
            
            var time = snapshot.GameStageState.Stage switch
            {
                Stage.AfterGame => snapshot.PostGameClockState.TicksPassed.Millseconds,
                _ => maximumTime - snapshot.IntermissionClockState.SecondsRemaining * 1000
            };

            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Direction"] = snapshot.GameStageState.Stage != Stage.AfterGame;
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Id"] = $"{game.Id}_Intermission";
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).InvertedTime"] = maximumTime - time;
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).MaximumTime"] = maximumTime;
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Name"] = "Intermission";
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Number"] = snapshot.GameStageState.Stage switch
            {
                Stage.AfterGame => snapshot.GameStageState.PeriodNumber + 1,
                _ => snapshot.GameStageState.PeriodNumber
            };
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Running"] =
                snapshot.IntermissionClockState.IsRunning || snapshot.PostGameClockState.IsRunning;
            result[$"ScoreBoard.Game({game.Id}).Clock(Intermission).Time"] = time;
        }

        void MapJamClock()
        {
            var maximumTime = snapshot.RulesState.Rules.JamRules.DurationInSeconds * 1000;
            var time = snapshot.JamClockState.TicksPassed.Millseconds;

            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Direction"] = true;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Id"] = $"{game.Id}_Jam";
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).InvertedTime"] = maximumTime - time;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).MaximumTime"] = maximumTime;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Name"] = "Jam";
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Number"] = snapshot.GameStageState.TotalJamNumber;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Running"] = snapshot.JamClockState.IsRunning;
            result[$"ScoreBoard.Game({game.Id}).Clock(Jam).Time"] = time;
        }

        void MapLineupClock()
        {
            var maximumTime = 24 * 60 * 60 * 1000;
            var time = snapshot.LineupClockState.TicksPassed.Millseconds;

            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Direction"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Id"] = $"{game.Id}_Lineup";
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).InvertedTime"] = maximumTime - time;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).MaximumTime"] = maximumTime;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Name"] = "Lineup";
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Number"] = snapshot.GameStageState.TotalJamNumber;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Running"] = snapshot.LineupClockState.IsRunning;
            result[$"ScoreBoard.Game({game.Id}).Clock(Lineup).Time"] = time;
        }

        void MapPeriodClock()
        {
            var maximumTime = snapshot.RulesState.Rules.PeriodRules.DurationInSeconds * 1000;
            var time = snapshot.PeriodClockState.TicksPassed.Millseconds;

            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Direction"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Id"] = $"{game.Id}_Period";
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).InvertedTime"] = maximumTime - time;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).MaximumTime"] = maximumTime;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Name"] = "Period";
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Number"] = snapshot.GameStageState.PeriodNumber;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Running"] = snapshot.PeriodClockState.IsRunning;
            result[$"ScoreBoard.Game({game.Id}).Clock(Period).Time"] = time;
        }

        void MapTimeoutClock()
        {
            var maximumTime =
                snapshot.CurrentTimeoutTypeState.Type switch
                {
                    TimeoutType.Team => snapshot.RulesState.Rules.TimeoutRules.TeamTimeoutDurationInSeconds * 1000,
                    _ => 24 * 60 * 60 * 1000
                };
            var time = snapshot.TimeoutClockState.TicksPassed.Millseconds;

            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Direction"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Id"] = $"{game.Id}_Timeout";
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).InvertedTime"] = maximumTime - time;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).MaximumTime"] = maximumTime;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Name"] = "Timeout";
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Number"] = snapshot.TimeoutListState.Timeouts.Length;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Running"] = snapshot.TimeoutClockState.IsRunning;
            result[$"ScoreBoard.Game({game.Id}).Clock(Timeout).Time"] = time;
        }

        void MapPeriod(int periodNumber)
        {
            var jamOffset = (periodNumber, snapshot.GameStageState) switch
            {
                (0, _) => 0,
                (_, { Stage: Stage.Jam }) => 0,
                _ => 1
            };

            var currentJamTotalNumber =
                periodNumber == 0 ? 1
                : snapshot.ScoreSheetState.HomeState.Jams.All(x => x.Period != periodNumber) ? 1
                : snapshot.ScoreSheetState.HomeState.Jams.Select((jam, index) => (jam, index)).Where(x => x.jam.Period == periodNumber).Max(x => x.index) + jamOffset;

            var currentJamNumber = 
                periodNumber == 0 ? 0
                : snapshot.ScoreSheetState.HomeState.Jams.Length == 0 ? 0
                : currentJamTotalNumber >= snapshot.ScoreSheetState.HomeState.Jams.Length ? snapshot.ScoreSheetState.HomeState.Jams[^1].Jam + 1
                : snapshot.ScoreSheetState.HomeState.Jams[currentJamTotalNumber].Jam;

            var periodJamCount = snapshot.ScoreSheetState.HomeState.Jams.Count(j => j.Period == periodNumber);
            var totalJamNumberPeriodStart = currentJamTotalNumber - periodJamCount;
            
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).CurrentJam"] = GetJamId(currentJamTotalNumber);
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).CurrentJamNumber"] = currentJamNumber;
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Duration"] = 0; // TODO: Calculate this
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Id"] = GetPeriodId(periodNumber);
            if (periodNumber == 0)
            {
                MapJam($"ScoreBoard.Game({game.Id}).Period(0).Jam(0)", -1);
            }
            for (var jamNumber = 0; jamNumber < periodJamCount; ++jamNumber)
            {
                MapJam($"ScoreBoard.Game({game.Id}).Period({periodNumber}).Jam({jamNumber + 1})", jamNumber + totalJamNumberPeriodStart);
            }

            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).LocalTimeStart"] = "";
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Next"] = GetPeriodId(periodNumber + 1);
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Number"] = periodNumber;
            if (periodNumber > 0)
                result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Previous"] = GetJamId(periodNumber - 1);
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Running"] =
                snapshot.GameStageState.PeriodNumber == periodNumber &&
                snapshot.GameStageState.Stage is not (Stage.AfterGame or Stage.Timeout or Stage.AfterTimeout);
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).SuddenScoring"] = false;
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Team1PenaltyCount"] =
                snapshot.PenaltySheetState.HomeState.Lines.Sum(l => l.Penalties.Count(p => p.Period == periodNumber));
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Team1Points"] =
                snapshot.ScoreSheetState.HomeState.Jams.Sum(j => j.JamTotal);
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Team2PenaltyCount"] =
                snapshot.PenaltySheetState.AwayState.Lines.Sum(l => l.Penalties.Count(p => p.Period == periodNumber));
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).Team2Points"] =
                snapshot.ScoreSheetState.AwayState.Jams.Sum(j => j.JamTotal);
            var periodTimeouts = snapshot.TimeoutListState.Timeouts.Where(t => t.Period == periodNumber).ToArray();

            if (periodTimeouts.Any())
            {
                foreach (var timeout in periodTimeouts)
                {
                    MapTimeout($"ScoreBoard.Game({game.Id}).Period({periodNumber})", timeout.EventId.ToString(), timeout);
                }
            }
            else
            {
                MapTimeout(
                    $"ScoreBoard.Game({game.Id}).Period({periodNumber})",
                    "noTimeout",
                    new(Guid7.Empty, TimeoutType.Untyped, 0, 0, null, 0, false));
            }

            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).WalltimeEnd"] = 0; // TODO: Calculate this
            result[$"ScoreBoard.Game({game.Id}).Period({periodNumber}).WalltimeStart"] = 0; // TODO: Calculate this
        }

        void MapTimeout(string keyPrefix, string id, TimeoutListItem timeout)
        {
            result[$"{keyPrefix}.Timeout({id}).Duration"] = (timeout.DurationInSeconds ?? 0) * 1000;
            result[$"{keyPrefix}.Timeout({id}).Id"] = id;
            result[$"{keyPrefix}.Timeout({id}).OrRequest"] = "";
            result[$"{keyPrefix}.Timeout({id}).OrResult"] = "";
            result[$"{keyPrefix}.Timeout({id}).Owner"] = timeout switch
            {
                { Type: TimeoutType.Official } => "O",
                { Type: TimeoutType.Review or TimeoutType.Team, Side: TeamSide.Home } => $"{game.Id}_1",
                { Type: TimeoutType.Review or TimeoutType.Team, Side: TeamSide.Away } => $"{game.Id}_2",
                _ => ""
            };
            // Skipped: PeriodClockElapsedEnd
            // Skipped: PeriodClockElapsedStart
            // Skipped: PeriodClockEnd
            result[$"{keyPrefix}.Timeout({id}).PrecedingJam"] =
                GetJamId(snapshot.ScoreSheetState.HomeState.Jams.IndexOf(j => j.Period == timeout.Period && j.Jam == timeout.Jam));
            result[$"{keyPrefix}.Timeout({id}).PrecedingJamNumber"] = timeout.Jam;
            result[$"{keyPrefix}.Timeout({id}).Readonly"] = false;
            result[$"{keyPrefix}.Timeout({id}).RetainedReview"] = timeout.Retained;
            result[$"{keyPrefix}.Timeout({id}).Review"] = timeout.Type == TimeoutType.Review;
            result[$"{keyPrefix}.Timeout({id}).Running"] = timeout.DurationInSeconds == null;
            result[$"{keyPrefix}.Timeout({id}).WalltimeEnd"] =
                new Tick(timeout.EventId.Tick).Millseconds + (timeout.DurationInSeconds ?? 0) * 1000;
            result[$"{keyPrefix}.Timeout({id}).WalltimeStart"] = new Tick(timeout.EventId.Tick).Millseconds;
        }

        void MapJam(string keyPrefix, int totalJamNumber)
        {
            var defaultJam = new ScoreSheetJam(snapshot.GameStageState.PeriodNumber, snapshot.GameStageState.JamNumber + 1, "?", "?", false, false, false, false, false, [], null, 0, 0, snapshot.OvertimeState.IsInOvertime);
            var (homeJam, awayJam) = totalJamNumber >= 0 && totalJamNumber < snapshot.ScoreSheetState.HomeState.Jams.Length
                ? (snapshot.ScoreSheetState.HomeState.Jams[totalJamNumber], snapshot.ScoreSheetState.AwayState.Jams[totalJamNumber])
                : (defaultJam, defaultJam);

            result[$"{keyPrefix}.Duration"] = 0; // TODO: Calculate this
            result[$"{keyPrefix}.Id"] = GetJamId(totalJamNumber);
            result[$"{keyPrefix}.InjuryContinuation"] = false;
            result[$"{keyPrefix}.Number"] = homeJam.Jam;
            result[$"{keyPrefix}.Overtime"] = homeJam.IsOvertimeJam;
            // Skipped: PeriodClockDisplayEnd
            // Skipped: PeriodClockElapsedEnd
            // Skipped: PeriodClockElapsedStart
            result[$"{keyPrefix}.PeriodNumber"] = homeJam.Period;
            result[$"{keyPrefix}.Previous"] = GetJamId(totalJamNumber - 1);
            result[$"{keyPrefix}.Readonly"] = false;
            result[$"{keyPrefix}.StarPass"] = homeJam.StarPassTrip != null;

            MapTeamJam($"{keyPrefix}.TeamJam(1)", homeJam, TeamSide.Home, totalJamNumber);
            MapTeamJam($"{keyPrefix}.TeamJam(2)", awayJam, TeamSide.Away, totalJamNumber);

            foreach (var timeout in snapshot.TimeoutListState.Timeouts.Where(t => t.Period == homeJam.Period && t.Jam == homeJam.Jam + 1))
            {
                result[$"{keyPrefix}.TimeoutsAfter({timeout.EventId})"] = timeout.EventId;
            }

            result[$"{keyPrefix}.WalltimeEnd"] = 0; // TODO: Calculate this
            result[$"{keyPrefix}.WalltimeStart"] = 0; // TODO: Calculate this
        }

        void MapTeamJam(string keyPrefix, ScoreSheetJam jam, TeamSide side, int totalJamNumber)
        {
            var lineup = 
                snapshot.LineupSheetState[side].Jams.FirstOrDefault(j => j.Period == jam.Period && j.Jam == jam.Jam)
                ?? new LineupSheetJam(jam.Period, jam.Jam, false, null, null, [null, null, null]);

            result[$"{keyPrefix}.AfterSPScore"] = jam.StarPassTrip == null ? 0 : jam.Trips.Skip((int)jam.StarPassTrip).Sum(j => j.Score ?? 0);
            result[$"{keyPrefix}.Calloff"] = jam.Called;
            result[$"{keyPrefix}.CurrentTrip"] = GetTripId(jam.Period, jam.Jam, jam.Trips.Length - 1);
            result[$"{keyPrefix}.CurrentTripNumber"] = jam.Trips.Length - 1;
            result[$"{keyPrefix}.DisplayLead"] = jam is { Lead: true, Lost: false };
            MapJamLineup(keyPrefix, jam, lineup, side, totalJamNumber);
            result[$"{keyPrefix}.Id"] = $"{GetJamId(totalJamNumber)}_{TeamNumber(side)}";
            result[$"{keyPrefix}.Injury"] = jam.Injury;
            result[$"{keyPrefix}.JamScore"] = jam.JamTotal;
            result[$"{keyPrefix}.LastScore"] = jam.GameTotal - jam.JamTotal;
            result[$"{keyPrefix}.Lead"] = jam.Lead;
            result[$"{keyPrefix}.Lost"] = jam.Lost;
            result[$"{keyPrefix}.Next"] = $"{GetJamId(totalJamNumber + 1)}_{TeamNumber(side)}";
            result[$"{keyPrefix}.NoInitial"] = jam.NoInitial;
            result[$"{keyPrefix}.NoPivot"] = lineup.PivotId == null;
            result[$"{keyPrefix}.Number"] = jam.Jam;
            result[$"{keyPrefix}.OsOffset"] = 0;
            result[$"{keyPrefix}.OsOffsetReason"] = "";
            result[$"{keyPrefix}.Previous"] = $"{GetJamId(totalJamNumber - 1)}_{TeamNumber(side)}";
            result[$"{keyPrefix}.Readonly"] = false;

            MapScoringTrips(jam, keyPrefix);

            result[$"{keyPrefix}.StarPass"] = jam.StarPassTrip != null;
            result[$"{keyPrefix}.TotalScore"] = jam.GameTotal;
        }

        void MapJamLineup(string keyPrefix, ScoreSheetJam jam, LineupSheetJam lineup, TeamSide side, int totalJamNumber)
        {
            MapFielding(keyPrefix, "Blocker1", jam, lineup.BlockerIds[0], side, totalJamNumber);
            MapFielding(keyPrefix, "Blocker2", jam, lineup.BlockerIds[1], side, totalJamNumber);
            MapFielding(keyPrefix, "Blocker3", jam, lineup.BlockerIds[2], side, totalJamNumber);
            MapFielding(keyPrefix, "Jammer", jam, lineup.JammerId, side, totalJamNumber);
            MapFielding(keyPrefix, "Pivot", jam, lineup.PivotId ?? (lineup.BlockerIds.Length > 3 ? lineup.BlockerIds[3] : null), side, totalJamNumber);
        }

        void MapFielding(string keyPrefix, string positionName, ScoreSheetJam jam, Guid? skaterId, TeamSide side, int totalJamNumber)
        {
            var boxTrips = snapshot.BoxTripsState[side].BoxTrips
                .Where(b => b.SkaterId == skaterId)
                .Where(b => b.TotalJamStart <= totalJamNumber &&
                            (b.DurationInJams == null || b.TotalJamStart + b.DurationInJams >= totalJamNumber))
                .OrderBy(b => b.TotalJamStart)
                .ToArray();

            var beforeStarPassBoxTripSymbols = boxTrips
                .Where(b => BoxTripIsRelevant(b, totalJamNumber, jam.StarPassTrip != null, false))
                .Select(b => SymbolForBoxTrip(b, totalJamNumber, false))
                .Map(string.Join, " ");

            var afterStarPassBoxTripSymbols = boxTrips
                .Where(b => BoxTripIsRelevant(b, totalJamNumber, jam.StarPassTrip != null, true))
                .Select(b => SymbolForBoxTrip(b, totalJamNumber, false))
                .Map(string.Join, " ");

            var currentBoxTrip = boxTrips
                .Select((b, i) => (b, i))
                .Where(b => b.b.DurationInJams == null)
                .Select(x => GetBoxTripId(x.b, x.i).ToString())
                .LastOrDefault();

            result[$"{keyPrefix}.Fielding({positionName}).Annotation"] = "";
            foreach (var (trip, tripNumber) in boxTrips.Select((b, i) => (b, i)))
            {
                var boxTripId = GetBoxTripId(trip, tripNumber);
                result[$"{keyPrefix}.Fielding({positionName}).BoxTrip({boxTripId})"] = boxTripId;
            }
            result[$"{keyPrefix}.Fielding({positionName}).BoxTripSymbols"] = afterStarPassBoxTripSymbols;
            result[$"{keyPrefix}.Fielding({positionName}).BoxTripSymbolsAfterSP"] = afterStarPassBoxTripSymbols;
            result[$"{keyPrefix}.Fielding({positionName}).BoxTripSymbolsBeforeSP"] = beforeStarPassBoxTripSymbols;
            result[$"{keyPrefix}.Fielding({positionName}).CurrentBoxTrip"] = currentBoxTrip ?? "";
            result[$"{keyPrefix}.Fielding({positionName}).Id"] = $"{GetJamId(totalJamNumber)}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Fielding({positionName}).Next"] = $"{GetJamId(totalJamNumber + 1)}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Fielding({positionName}).NotFielded"] = skaterId == null;
            result[$"{keyPrefix}.Fielding({positionName}).Number"] = jam.Jam;
            result[$"{keyPrefix}.Fielding({positionName}).PenaltyBox"] = currentBoxTrip != null;
            result[$"{keyPrefix}.Fielding({positionName}).Position"] = $"{game.Id}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Fielding({positionName}).Previous"] = $"{GetJamId(totalJamNumber - 1)}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Fielding({positionName}).Readonly"] = false;
            result[$"{keyPrefix}.Fielding({positionName}).SitFor3"] = snapshot.InjuriesState[side].Injuries.Any(i => !i.Expired);
            result[$"{keyPrefix}.Fielding({positionName}).Skater"] = skaterId?.ToString() ?? "";
            result[$"{keyPrefix}.Fielding({positionName}).SkaterNumber"] = 
                snapshot.TeamDetailsState[side].Team.Roster.FirstOrDefault(s => s.Id == skaterId)?.Number ?? "";
        }

        void MapScoringTrips(ScoreSheetJam jam, string keyPrefix)
        {
            var trips = jam.Trips;
            if (jam is { IsOvertimeJam: false, NoInitial: false })
                trips = trips.Prepend(new(0)).ToArray();

            for (var tripIndex = 0; tripIndex < trips.Length; ++tripIndex)
            {
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).AfterSP"] = jam.StarPassTrip != null && tripIndex >= jam.StarPassTrip;
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Annotation"] = "";
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Current"] =
                    snapshot.GameStageState.Stage == Stage.Jam
                    && snapshot.GameStageState.PeriodNumber == jam.Period
                    && snapshot.GameStageState.JamNumber == jam.Jam
                    && tripIndex == jam.Trips.Length - 1;
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Duration"] = 0; // TODO: Calculate this
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Id"] = GetTripId(jam.Period, jam.Jam, tripIndex);
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).JamClockEnd"] = 0; // TODO: Calculate this
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).JamClockStart"] = 0; // TODO: Calculate this
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Next"] = GetTripId(jam.Period, jam.Jam + 1, tripIndex);
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Number"] = tripIndex + 1;
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Readonly"] = false;
                result[$"{keyPrefix}.ScoringTrip({tripIndex + 1}).Score"] = trips[tripIndex].Score ?? 0;
            }
        }

        string FormatTime(int timeInSeconds) =>
            $"{timeInSeconds / 60}:{(timeInSeconds % 60).ToString().PadLeft(2, '0')}";

        void MapRules()
        {
            var rules = snapshot.RulesState.Rules;

            result[$"ScoreBoard.Game({game.Id}).Rule(Intermission.ClockDirection)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Intermission.Durations)"] = 
                Enumerable.Repeat(FormatTime(rules.IntermissionRules.DurationInSeconds), rules.PeriodRules.PeriodCount - 1)
                .Append("60:00")
                .Map(string.Join, ",");
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.ClockDirection)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.Duration)"] = FormatTime(rules.JamRules.DurationInSeconds);
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.InjuryContinuation)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.ResetNumberEachPeriod)"] = rules.JamRules.ResetJamNumbersBetweenPeriods;
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.SuddenScoring)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.SuddenScoringDuration)"] = "1:00";
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.SuddenScoringMaxTrailingPoints)"] = "25";
            result[$"ScoreBoard.Game({game.Id}).Rule(Jam.SuddenScoringMinPointsDifference)"] = "150";
            result[$"ScoreBoard.Game({game.Id}).Rule(Lineup.ClockDirection)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Lineup.Duration)"] = FormatTime(rules.LineupRules.DurationInSeconds);
            result[$"ScoreBoard.Game({game.Id}).Rule(Lineup.OvertimeDuration)"] = FormatTime(rules.LineupRules.OvertimeDurationInSeconds);
            result[$"ScoreBoard.Game({game.Id}).Rule(Lineup.StopsPeriodClock)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Penalties.DefinitionFile)"] = "/config/penalties/wftda2018.json";
            result[$"ScoreBoard.Game({game.Id}).Rule(Penalties.Duration)"] = "0:30";
            result[$"ScoreBoard.Game({game.Id}).Rule(Penalties.NumberToFoulout)"] = rules.PenaltyRules.FoulOutPenaltyCount;
            result[$"ScoreBoard.Game({game.Id}).Rule(Penalties.PointsDeduction)"] = 0;
            result[$"ScoreBoard.Game({game.Id}).Rule(Period.ClockDirection)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Period.Duration)"] = FormatTime(rules.PeriodRules.DurationInSeconds);
            result[$"ScoreBoard.Game({game.Id}).Rule(Period.EndBetweenJams)"] = rules.PeriodRules.PeriodEndBehavior == PeriodEndBehavior.AnytimeOutsideJam;
            result[$"ScoreBoard.Game({game.Id}).Rule(Period.JamsPer)"] = "0";
            result[$"ScoreBoard.Game({game.Id}).Rule(Period.Number)"] = rules.PeriodRules.PeriodCount;
            result[$"ScoreBoard.Game({game.Id}).Rule(Score.EnforceTimeToOr)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Score.PointsOnInitial)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Score.WftdaLateChangeRule)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.MaxRetains)"] = 1;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.OfficialReviews)"] = 1;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.OfficialReviewsPer)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.RDCLPerHalfRules)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.Timeouts)"] = rules.TimeoutRules.TeamTimeoutAllowance;
            result[$"ScoreBoard.Game({game.Id}).Rule(Team.TimeoutsPer)"] = rules.TimeoutRules.ResetBehavior == TimeoutResetBehavior.Never;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.ClockDirection)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.ExtraJamAfterOTO)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.JamDuring)"] = false;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.NoClockStop)"] = true;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.StopPeriodClockAfterTODuration)"] = "60:00";
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.StopPeriodClockAlways)"] = rules.TimeoutRules.PeriodClockBehavior == TimeoutPeriodClockStopBehavior.All;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.StopPeriodClockOnOR)"] = (rules.TimeoutRules.PeriodClockBehavior | TimeoutPeriodClockStopBehavior.OfficialReview) == TimeoutPeriodClockStopBehavior.OfficialReview;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.StopPeriodClockOnOTO)"] = (rules.TimeoutRules.PeriodClockBehavior | TimeoutPeriodClockStopBehavior.OfficialTimeout) == TimeoutPeriodClockStopBehavior.OfficialTimeout;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.StopPeriodClockOnTTO)"] = (rules.TimeoutRules.PeriodClockBehavior | TimeoutPeriodClockStopBehavior.TeamTimeout) == TimeoutPeriodClockStopBehavior.TeamTimeout;
            result[$"ScoreBoard.Game({game.Id}).Rule(Timeout.TeamTODuration)"] = FormatTime(rules.TimeoutRules.TeamTimeoutDurationInSeconds);
            result[$"ScoreBoard.Game({game.Id}).RulesetName"] = "";
        }

        void MapTeam(TeamSide side)
        {
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).ActiveScoreAdjustmentAmount"] = 0;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).AllBlockersSet"] = false;

            foreach (var trip in snapshot.BoxTripsState[side].BoxTrips.GroupBy(b => b.TotalJamStart).SelectMany(g => g.Select((b, i) => (b, i))))
            {
                MapBoxTrip($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", trip.b, trip.i, side);
            }

            var currentJam = 
                snapshot.ScoreSheetState[side].Jams.LastOrDefault()
                ?? new ScoreSheetJam(0, 0, "?", "?", false, false, false, false, false, [], null, 0, 0, false);

            var currentLineup =
                snapshot.LineupSheetState[side].Jams.LastOrDefault()
                ?? new LineupSheetJam(0, 0, false, null, null, [null, null, null]);

            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Calloff"] = snapshot.TeamJamStatsState[side].Called;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).CurrentTrip"] =
                GetTripId(currentJam.Period, currentJam.Jam, currentJam.Trips.Length);
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).DisplayLead"] =
                snapshot.TeamJamStatsState[side] is { Lead: true, Lost: false };
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).FieldingAdvancePending"] = false;
            // Skipped: FileName
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).FullName"] = game.Name;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Id"] = $"{game.Id}_{TeamNumber(side)}";
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).InOfficialReview"] =
                snapshot.CurrentTimeoutTypeState.Type == TimeoutType.Review &&
                snapshot.CurrentTimeoutTypeState.TeamSide == side;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).InTimeout"] =
                snapshot.CurrentTimeoutTypeState.Type == TimeoutType.Team &&
                snapshot.CurrentTimeoutTypeState.TeamSide == side;
            var teamNames = snapshot.TeamDetailsState[side].Team.Names;
            var displayName = teamNames.GetValueOrDefault("league") ?? teamNames.GetValueOrDefault("team", "");

            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Initials"] =
                displayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x[0].ToString().ToUpperInvariant())
                .Map(string.Concat);
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Injury"] = currentJam.Injury;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).JamScore"] = currentJam.JamTotal;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).LastScore"] =
                currentJam.GameTotal - currentJam.JamTotal;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Lead"] = currentJam.Lead;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).LeagueName"] =
                teamNames.GetValueOrDefault("league", "");
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Logo"] = "";
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Lost"] = currentJam.Lost;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Name"] = displayName;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).NoInitial"] = currentJam.NoInitial;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).NoPivot"] = currentLineup.PivotId == null;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).OfficialReviews"] =
                snapshot.TeamTimeoutsState[side].ReviewStatus == ReviewStatus.Used ? 0 : 1;
            
            MapPosition($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", "Blocker1", currentJam, currentLineup.BlockerIds[0], currentTotalJamNumber, side);
            MapPosition($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", "Blocker2", currentJam, currentLineup.BlockerIds[1], currentTotalJamNumber, side);
            MapPosition($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", "Blocker3", currentJam, currentLineup.BlockerIds[2], currentTotalJamNumber, side);
            MapPosition($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", "Jammer", currentJam, currentLineup.JammerId, currentTotalJamNumber, side);
            MapPosition($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", "Pivot", currentJam, currentLineup.PivotId, currentTotalJamNumber, side);

            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).PreparedTeamConnected"] = false;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Readonly"] = false;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).RetainedOfficialReview"] =
                snapshot.TeamTimeoutsState[side].ReviewStatus == ReviewStatus.Retained;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).RunningOrEndedTeamJam"] = 
                $"{GetJamId(currentTotalJamNumber)}_{TeamNumber(side)}";
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).RunningOrUpcomingTeamJam"] =
                $"{GetJamId(currentTotalJamNumber + 1)}_{TeamNumber(side)}";
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Score"] = snapshot.TeamScoreState[side].Score;

            foreach (var skater in snapshot.TeamDetailsState[side].Team.Roster)
            {
                MapSkater($"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)})", skater, side, currentTotalJamNumber);
            }

            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).StarPass"] =
                snapshot.TeamJamStatsState[side].StarPass;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).TeamName"] =
                snapshot.TeamDetailsState[side].Team.Names.GetValueOrDefault("team", "");

            foreach (var timeout in snapshot.TimeoutListState.Timeouts.Where(t => t.Side == side))
            {
                result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Timeout({timeout.EventId})"] =
                    timeout.EventId;
            }

            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).Timeouts"] =
                snapshot.RulesState.Rules.TimeoutRules.TeamTimeoutAllowance - snapshot.TeamTimeoutsState[side].NumberTaken;
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).TotalPenalties"] =
                snapshot.PenaltySheetState[side].Lines.Sum(l => l.Penalties.Length);
            result[$"ScoreBoard.Game({game.Id}).Team({TeamNumber(side)}).TripScore"] =
                snapshot.TripScoreState[side].Score ?? 0;
        }

        void MapBoxTrip(string keyPrefix, BoxTrip trip, int indexInJam, TeamSide side)
        {
            var roster = snapshot.TeamDetailsState[side].Team.Roster;
            var id = GetBoxTripId(trip, indexInJam);
            keyPrefix = $"{keyPrefix}.BoxTrip({id})";
            // Skipped: CurrentFielding
            result[$"{keyPrefix}.CurrentSkater"] = trip.Substitutions.Select(s => s.NewId).Prepend(trip.SkaterId).Last();
            result[$"{keyPrefix}.Duration"] = trip.TicksPassed.Millseconds;
            result[$"{keyPrefix}.EndAfterSP"] = trip.EndAfterStarPass;
            result[$"{keyPrefix}.EndBetweenJams"] = false;
            // Skipped: EndFielding
            if (trip.DurationInJams != null)
                result[$"{keyPrefix}.EndJamNumber"] = trip.Jam + trip.DurationInJams;
            // Skipped: Fielding(...)
            result[$"{keyPrefix}.Id"] = id;
            result[$"{keyPrefix}.IsCurrent"] = trip.DurationInJams == null;
            result[$"{keyPrefix}.JamClockEnd"] = 0; // TODO: Calculate this
            result[$"{keyPrefix}.JamClockStart"] = 0; // TODO: Calculate this
            // Skipped: Penalty
            // Skipped: PenaltyCodes
            // Skipped: PenaltyDetails
            result[$"{keyPrefix}.Readonly"] = false;
            result[$"{keyPrefix}.RosterNumber"] = roster.FirstOrDefault(s => s.Id == trip.SkaterId)?.Number ?? "";
            result[$"{keyPrefix}.Shortened"] = 0;
            result[$"{keyPrefix}.StartAfterSP"] = trip.StartAfterStarPass;
            result[$"{keyPrefix}.StartBetweenJams"] = trip.StartBetweenJams;
            // Skipped: StartFielding
            result[$"{keyPrefix}.StartJamNumber"] = trip.Jam;
            result[$"{keyPrefix}.TimingStopped"] = false;
            result[$"{keyPrefix}.TotalPenalties"] = snapshot.PenaltySheetState[side].Lines.FirstOrDefault(l => l.SkaterId == trip.SkaterId)?.Penalties.Length ?? 0;
            // Skipped: WalltimeEnd
            // Skipped: WalltimeStart

        }

        void MapPosition(string keyPrefix, string positionName, ScoreSheetJam jam, Guid? skaterId, int totalJamNumber, TeamSide side)
        {
            var boxTrips = snapshot.BoxTripsState[side].BoxTrips
                .Where(b => b.SkaterId == skaterId)
                .Where(b => b.TotalJamStart <= totalJamNumber &&
                            (b.DurationInJams == null || b.TotalJamStart + b.DurationInJams >= totalJamNumber))
                .OrderBy(b => b.TotalJamStart)
                .ToArray();

            var boxTripSymbols = boxTrips
                .Where(b => BoxTripIsRelevant(b, totalJamNumber, false, false))
                .Select(b => SymbolForBoxTrip(b, totalJamNumber, false))
                .Map(string.Join, " ");

            var skaterPenalties = snapshot.PenaltySheetState[side].Lines.FirstOrDefault(l => l.SkaterId == skaterId)?.Penalties ?? [];

            var currentPenalties =
                skaterPenalties
                    .Where(p => p.Jam == jam.Jam && p.Period == jam.Period)
                    .ToArray();

            var skater = snapshot.TeamDetailsState[side].Team.Roster
                .FirstOrDefault(s => s.Id == skaterId, new(Guid.Empty, "", "", false));

            result[$"{keyPrefix}.Position({positionName}).Annotation"] = "";
            result[$"{keyPrefix}.Position({positionName}).CurrentBoxSymbols"] = boxTripSymbols;
            result[$"{keyPrefix}.Position({positionName}).CurrentFielding"] = $"{GetJamId(totalJamNumber)}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Position({positionName}).CurrentPenalties"] = currentPenalties.Select(p => p.Code).Map(string.Concat);
            result[$"{keyPrefix}.Position({positionName}).ExtraPenaltyTime"] = 0;
            result[$"{keyPrefix}.Position({positionName}).Flags"] = "";
            result[$"{keyPrefix}.Position({positionName}).HasUnserved"] = currentPenalties.Any(p => !p.Served);
            result[$"{keyPrefix}.Position({positionName}).Id"] = $"{game.Id}_{TeamNumber(side)}_{positionName}";
            result[$"{keyPrefix}.Position({positionName}).Name"] = skater.Name;
            result[$"{keyPrefix}.Position({positionName}).PenaltyBox"] =
                snapshot.PenaltyBoxState[side].Skaters.Contains(skaterId ?? Guid.Empty);
            result[$"{keyPrefix}.Position({positionName}).PenaltyCount"] = skaterPenalties.Length;
            result[$"{keyPrefix}.Position({positionName}).PenaltyDetails"] = "";
            result[$"{keyPrefix}.Position({positionName}).Readonly"] = false;
            result[$"{keyPrefix}.Position({positionName}).RosterNumber"] = skater.Number;
        }

        void MapSkater(string keyPrefix, GameSkater skater, TeamSide side, int totalJamNumber)
        {
            keyPrefix = $"{keyPrefix}.Skater({skater.Id})";

            var skaterPenalties =
                snapshot.PenaltySheetState[side].Lines.FirstOrDefault(l => l.SkaterId == skater.Id)?.Penalties ?? [];

            var currentPenalties = skaterPenalties.Where(p => !p.Served).ToArray();

            result[$"{keyPrefix}.BaseRole"] = "Bench";
            result[$"{keyPrefix}.Color"] = "";
            result[$"{keyPrefix}.CurrentBoxSymbols"] =
                snapshot.BoxTripsState[side].BoxTrips.Where(b => b.DurationInJams == null)
                    .Select(b => SymbolForBoxTrip(b, totalJamNumber, snapshot.TeamJamStatsState[side].StarPass))
                    .Map(string.Join, " ");
            result[$"{keyPrefix}.CurrentPenalties"] = currentPenalties.Select(p => p.Code).Map(string.Join, " ");
            result[$"{keyPrefix}.ExtraPenaltyTime"] = 0;
            foreach (var linedUpJam in snapshot.LineupSheetState[side].Jams.Where(j => j.SkaterIds.Contains(skater.Id)))
            {
                var positionName =
                    linedUpJam.JammerId == skater.Id ? "Jammer"
                    : linedUpJam.PivotId == skater.Id ? "Pivot"
                    : $"Blocker{linedUpJam.BlockerIds.IndexOf(i => skater.Id == i) + 1}";

                var fieldingId = $"{GetJamId(totalJamNumber)}_{TeamNumber(side)}_{positionName}";

                result[$"{keyPrefix}.Fielding({fieldingId})"] = fieldingId;
            }

            result[$"{keyPrefix}.Flags"] = "";
            result[$"{keyPrefix}.HasUnserved"] = currentPenalties.Any();
            result[$"{keyPrefix}.Id"] = skater.Id;
            result[$"{keyPrefix}.Name"] = skater.Name;

            foreach (var (penalty, penaltyIndex) in skaterPenalties.Select((p, i) => (p, i)))
            {
                var penaltyPrefix = $"{keyPrefix}.Penalty({penaltyIndex + 1})";
                result[$"{penaltyPrefix}.BoxTrip"] = ""; // TODO: Calculate this
                result[$"{penaltyPrefix}.Code"] = penalty.Code;
                result[$"{penaltyPrefix}.ForceServed"] = false;
                result[$"{penaltyPrefix}.Id"] = GetPenaltyId(penalty, skater.Id, penaltyIndex);
                result[$"{penaltyPrefix}.Jam"] = GetJamId(totalJamNumber);
                result[$"{penaltyPrefix}.JamNumber"] = penalty.Jam;
                if (penaltyIndex < skaterPenalties.Length - 1)
                    result[$"{penaltyPrefix}.Next"] = GetPenaltyId(skaterPenalties[penaltyIndex + 1], skater.Id, penaltyIndex + 1);
                result[$"{penaltyPrefix}.Number"] = penaltyIndex + 1;
                result[$"{penaltyPrefix}.PeriodNumber"] = penalty.Period;
                if (penaltyIndex > 0)
                    result[$"{penaltyPrefix}.Previous"] = GetPenaltyId(skaterPenalties[penaltyIndex - 1], skater.Id, penaltyIndex - 1);
                result[$"{penaltyPrefix}.Readonly"] = false;
                result[$"{penaltyPrefix}.Served"] = penalty.Served;
                result[$"{penaltyPrefix}.Serving"] = false; // TODO: Calculate this
                result[$"{penaltyPrefix}.Time"] = 0; // TODO: Calculate this
            }

            result[$"{keyPrefix}.PenaltyBox"] = false; //TODO: Calculate this
            result[$"{keyPrefix}.PenaltyCount"] = skaterPenalties.Length;
            result[$"{keyPrefix}.PenaltyDetails"] = "";
            result[$"{keyPrefix}.Pronouns"] = "";
            result[$"{keyPrefix}.Readonly"] = false;
            result[$"{keyPrefix}.Role"] = "Bench";
            result[$"{keyPrefix}.RosterNumber"] = skater.Number;
        }

        bool BoxTripIsRelevant(BoxTrip trip, int totalJamNumber, bool hasStarPass, bool afterStarPass)
        {
            var tripStartsThisJam = trip.TotalJamStart == totalJamNumber;
            var tripStartsBeforeThisJam = trip.TotalJamStart < totalJamNumber;
            var tripEndsThisJam = trip.TotalJamStart + (trip.DurationInJams ?? 1000) == totalJamNumber;
            var tripEndsAfterThisJam = trip.TotalJamStart + (trip.DurationInJams ?? 1000) > totalJamNumber;
            var tripStartsBeforeStarPassAndRunsForMultipleJams = !trip.StartAfterStarPass && afterStarPass && (trip.DurationInJams ?? 1000) > 0;
            var tripEndsThisJamAndStarPassMatches = tripEndsThisJam && trip.EndAfterStarPass == afterStarPass;
            var tripStartsBeforeJamAndEndsThisJam = tripStartsBeforeThisJam && tripEndsThisJam;

            return
                tripStartsBeforeThisJam && tripEndsAfterThisJam
                || tripStartsThisJam && (
                    !hasStarPass
                    || tripStartsBeforeStarPassAndRunsForMultipleJams && afterStarPass
                    || trip.StartAfterStarPass == afterStarPass
                )
                || tripStartsBeforeJamAndEndsThisJam && (trip.EndAfterStarPass || !trip.EndAfterStarPass && !afterStarPass)
                || tripEndsThisJamAndStarPassMatches;
        }

        string SymbolForBoxTrip(BoxTrip boxTrip, int totalJamNumber, bool afterStarPass)
        {
            var startInBox =
                boxTrip.TotalJamStart < totalJamNumber
                || boxTrip.StartBetweenJams && boxTrip.TotalJamStart == totalJamNumber
                || boxTrip.TotalJamStart == totalJamNumber && afterStarPass && !boxTrip.StartAfterStarPass;

            var endThisJam =
                boxTrip.DurationInJams != null
                && boxTrip.TotalJamStart + boxTrip.DurationInJams == totalJamNumber
                && boxTrip.EndAfterStarPass == afterStarPass;

            return (startInBox, endThisJam) switch
            {
                (true, true) => "$",
                (true, false) => "S",
                (false, true) => "+",
                (false, false) => "-",
            };
        }

        Guid GetPeriodId(int period)
        {
            var idBytes = game.Id.ToByteArray();
            idBytes[^1] = (byte)period;
            return new Guid(idBytes);
        }

        Guid GetJamId(int totalJamNumber)
        {
            var idBytes = game.Id.ToByteArray();
            idBytes[^1] = (byte)totalJamNumber;
            return new Guid(idBytes);
        }

        Guid GetTripId(int period, int jam, int trip)
        {
            var idBytes = game.Id.ToByteArray();
            idBytes[^1] = (byte)trip;
            idBytes[^2] = (byte)jam;
            idBytes[^3] = (byte)period;
            return new Guid(idBytes);
        }

        Guid GetBoxTripId(BoxTrip boxTrip, int tripNumberInJam)
        {
            var idBytes = game.Id.ToByteArray();
            var skaterIdBytes = boxTrip.SkaterId.ToByteArray();
            skaterIdBytes[..7].CopyTo(idBytes, 7);
            idBytes[^1] = (byte)tripNumberInJam;
            idBytes[^2] = (byte)boxTrip.TotalJamStart;

            return new Guid(idBytes);
        }

        Guid GetPenaltyId(Reducers.Penalty penalty, Guid skaterId, int index)
        {
            var idBytes = game.Id.ToByteArray();
            var skaterIdBytes = skaterId.ToByteArray();
            skaterIdBytes[..7].CopyTo(idBytes, 7);
            idBytes[^1] = (byte)index;
            idBytes[^2] = (byte)penalty.Jam;
            idBytes[^3] = (byte)penalty.Period;

            return new Guid(idBytes);
        }
    }

    private static int TeamNumber(TeamSide side) =>
        side == TeamSide.Home ? 1 : 2;
}

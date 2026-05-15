using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Serialization;

namespace jamster.engine.Carolina;

public interface IChannelMapper
{
    Dictionary<string, object?> Map(GameStageState state, Guid gameId);
    Dictionary<string, object?> Map(OvertimeState state, Guid gameId);
    Dictionary<string, object?> Map(JamClockState state, Ruleset rules, Guid gameId);
    Dictionary<string, object?> Map(PeriodClockState state, Ruleset rules, Guid gameId);
    Dictionary<string, object?> Map(LineupClockState state, Guid gameId);
    Dictionary<string, object?> Map(TimeoutClockState state, Guid gameId);
    Dictionary<string, object?> Map(IntermissionClockState state, Guid gameId);
    Dictionary<string, object?> Map(PostGameClockState state, Ruleset rules, Guid gameId);
    Dictionary<string, object?> Map(TeamScoreState state, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(TripScoreState state, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(TeamJamStatsState state, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(TeamTimeoutsState state, TeamSide side, Ruleset rules, Guid gameId);
    Dictionary<string, object?> Map(JamLineupState state, TeamDetailsState teamDetails, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(CurrentTimeoutTypeState state, Guid gameId);
    Dictionary<string, object?> Map(TeamDetailsState state, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(ScoreSheetState state, ScoreSheetState otherTeamState, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(PenaltySheetState state, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(LineupSheetState state, TeamDetailsState teamDetails, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(PenaltyBoxState state, JamLineupState jamLineup, TeamDetailsState teamDetails, TeamSide side, Guid gameId);
    Dictionary<string, object?> Map(TimeoutListState state, Guid gameId);
}

public class ChannelMapper : IChannelMapper
{
    public Dictionary<string, object?> Map(GameStageState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Jam).Number"] = state.JamNumber,
        [$"ScoreBoard.Game({gameId}).CurrentPeriodNumber"] = state.PeriodNumber,
        [$"ScoreBoard.Game({gameId}).InJam"] = state.Stage is Stage.Jam,
        [$"ScoreBoard.Game({gameId}).State"] = state switch
        {
            { Stage: Stage.BeforeGame } => "Prepared",
            { Stage: Stage.AfterGame, PeriodIsFinalized: true } => "Finished",
            _ => "Running"
        },
        [$"ScoreBoard.Game({gameId}).InPeriod"] = state.Stage is Stage.Jam or Stage.Lineup or Stage.Timeout or Stage.AfterTimeout,
        [$"ScoreBoard.Game({gameId}).NoMoreJam"] = !state.NextJamShouldStart,
    };

    public Dictionary<string, object?> Map(OvertimeState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).InOvertime"] = state.IsInOvertime,
    };

    public Dictionary<string, object?> Map(JamClockState state, Ruleset rules, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Jam).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Jam).Time"] = state.TicksPassed.Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Jam).Direction"] = true,
    };

    public Dictionary<string, object?> Map(PeriodClockState state, Ruleset rules, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Period).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Period).Time"] = (Tick.FromSeconds(rules.PeriodRules.DurationInSeconds) - state.TicksPassed).Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Period).Direction"] = true,
    };

    public Dictionary<string, object?> Map(LineupClockState state, Ruleset rules, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Lineup).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Lineup).Time"] = state.TicksPassed.Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Lineup).Direction"] = false,
    };

    public Dictionary<string, object?> Map(TimeoutClockState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Timeout).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Timeout).Time"] = state.TicksPassed.Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Timeout).Direction"] = false,
    };

    public Dictionary<string, object?> Map(IntermissionClockState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Time"] = Tick.FromSeconds(state.SecondsRemaining).Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Direction"] = true,
    };

    public Dictionary<string, object?> Map(PostGameClockState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Running"] = state.IsRunning,
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Time"] = state.TicksPassed.Millseconds,
        [$"ScoreBoard.Game({gameId}).Clock(Intermission).Direction"] = false,
    };

    public Dictionary<string, object?> Map(TeamScoreState state, TeamSide side, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Score"] = state.Score,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).JamScore"] = state.JamScore,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).LastScore"] = state.Score - state.JamScore,
    };

    public Dictionary<string, object?> Map(TripScoreState state, TeamSide side, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).TripScore"] = state.Score ?? 0,
    };

    public Dictionary<string, object?> Map(TeamJamStatsState state, TeamSide side, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Lead"] = state.Lead,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).DisplayLead"] = state.Lead && !state.Lost,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Lost"] = state.Lost,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Calloff"] = state.Called,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).StarPass"] = state.StarPass,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).NoInitial"] = !state.HasCompletedInitial,
    };

    public Dictionary<string, object?> Map(TeamTimeoutsState state, TeamSide side, Ruleset rules, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Timeouts"] = rules.TimeoutRules.TeamTimeoutAllowance - state.NumberTaken,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).OfficialReviews"] = state.ReviewStatus is ReviewStatus.Used ? 0 : 1,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).RetainedOfficialReview"] = state.ReviewStatus is ReviewStatus.Retained,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).InTimeout"] = state.CurrentTimeout is TimeoutInUse.Timeout,
        [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).InOfficialReview"] = state.CurrentTimeout is TimeoutInUse.Review,
    };

    public Dictionary<string, object?> Map(JamLineupState state, TeamDetailsState teamDetails, TeamSide side, Guid gameId)
    {
        var teamNumber = side == TeamSide.Home ? 1 : 2;
        GameSkater? FindSkater(Guid? id) => teamDetails.Team.Roster.SingleOrDefault(r => r.Id == id);
        return new()
        {
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Jammer).RosterNumber"] = FindSkater(state.JammerId)?.Number,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Jammer).Name"] = FindSkater(state.JammerId)?.Name,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Pivot).RosterNumber"] = FindSkater(state.PivotId)?.Number,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Pivot).Name"] = FindSkater(state.PivotId)?.Name,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker1).RosterNumber"] = FindSkater(state.BlockerIds[0])?.Number,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker1).Name"] = FindSkater(state.BlockerIds[0])?.Name,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker2).RosterNumber"] = FindSkater(state.BlockerIds[1])?.Number,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker2).Name"] = FindSkater(state.BlockerIds[1])?.Name,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker3).RosterNumber"] = FindSkater(state.BlockerIds[2])?.Number,
            [$"ScoreBoard.Game({gameId}).Team({teamNumber}).Position(Blocker3).Name"] = FindSkater(state.BlockerIds[2])?.Name,
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).NoPivot"] = state.PivotId == null,
        };
    }

    public Dictionary<string, object?> Map(CurrentTimeoutTypeState state, Guid gameId) => new()
    {
        [$"ScoreBoard.Game({gameId}).TimeoutOwner"] = state switch
        {
            { Type: TimeoutType.Official } => "O",
            { Type: TimeoutType.Team or TimeoutType.Review, TeamSide: TeamSide.Home } => "1",
            { Type: TimeoutType.Team or TimeoutType.Review, TeamSide: TeamSide.Away } => "2",
            _ => "",
        },
        [$"ScoreBoard.Game({gameId}).OfficialReview"] = state.Type == TimeoutType.Review,
    };

    public Dictionary<string, object?> Map(TeamDetailsState state, TeamSide side, Guid gameId) 
    {
        var result = new Dictionary<string, object?>()
        {
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Name"] = state.Team.Names.GetValueOrDefault("team"),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).FullName"] = (state.Team.Names.GetValueOrDefault("league"), state.Team.Names.GetValueOrDefault("team")) switch
            {
                (not null, not null) s => $"{s.Item1} {s.Item2}",
                (not null, null) s => s.Item1,
                (null, not null) s => s.Item2,
                _ => null
            },
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).LeagueName"] = state.Team.Names.GetValueOrDefault("league"),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).TeamName"] = state.Team.Names.GetValueOrDefault("team"),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).UniformColor"] = state.Team.Names.GetValueOrDefault("color"),
        };

        foreach (var skater in state.Team.Roster)
        {
            result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({skater.Id}).Name"] = skater.Name;
            result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({skater.Id}).RosterNumber"] = skater.Number;
        }

        return result;
    }

    public Dictionary<string, object?> Map(ScoreSheetState state, ScoreSheetState otherTeamState, TeamSide side, Guid gameId)
    {
        var result = new Dictionary<string, object?>
        {
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Injury"] = state.Jams.LastOrDefault()?.Injury ?? false,
        };

        foreach (var (jam, otherTeamJam) in state.Jams.Where(j => !j.Deleted).Zip(otherTeamState.Jams.Where(j => !j.Deleted), (a, b) => (a, b)))
        {
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).TotalScore"] = jam.GameTotal;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).JamScore"] = jam.JamTotal;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).LastScore"] = jam.GameTotal - jam.JamTotal;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Lead"] = jam.Lead;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).DisplayLead"] = jam is { Lead: true, Lost: false };
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Lost"] = jam.Lost;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Calloff"] = jam.Called;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).NoInitial"] = jam.NoInitial;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).StarPass"] = jam.StarPassTrip != null;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Injury"] = jam.Injury;

            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).Overtime"] = jam.IsOvertimeJam;
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).StarPass"] = jam.StarPassTrip != null || otherTeamJam.StarPassTrip != null;

            foreach (var (trip, tripNumber) in jam.Trips.Select((t, i) => (t, i)))
            {
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).AfterSP"] = tripNumber >= (jam.StarPassTrip ?? int.MaxValue);
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Annotation"] = "";
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Current"] = false;
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Duration"] = 0;
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Id"] = GetTripId(jam, trip, tripNumber);
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).JamClockEnd"] = "";
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).JamClockStart"] = "";
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Number"] = tripNumber + 1;
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Readonly"] = false;
                result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).ScoringTrip({tripNumber + 1}).Score"] = trip.Score;
            }
        }

        return result;

        Guid GetTripId(ScoreSheetJam jam, JamLineTrip trip, int tripNumber)
        {
            var tripIdBytes = gameId.ToByteArray();

            tripIdBytes[^1] = (byte)tripNumber;
            tripIdBytes[^2] = (byte)jam.Jam;
            tripIdBytes[^3] = (byte)jam.Period;
            tripIdBytes[^4] = (byte)(side == TeamSide.Home ? 1 : 2);

            return new Guid(tripIdBytes);
        }
    }

    public Dictionary<string, object?> Map(PenaltySheetState state, TeamSide side, Guid gameId)
    {
        var result = new Dictionary<string, object?>()
        {
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).TotalPenalties"] = state.Lines.Sum(l => l.Penalties.Length),
        };

        foreach (var line in state.Lines)
        {
            result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).PenaltyCount"] = line.Penalties.Length;

            if (line.ExpulsionPenalty != null)
            {
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty(0).Code"] = line.ExpulsionPenalty.Code;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty(0).PeriodNumber"] = line.ExpulsionPenalty.Period;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty(0).JamNumber"] = line.ExpulsionPenalty.Jam;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty(0).Served"] = line.ExpulsionPenalty.Served;
            }

            foreach (var (penalty, penaltyNumber) in line.Penalties.Select((p, i) => (p, i)))
            {
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty({penaltyNumber + 1}).Code"] = penalty.Code;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty({penaltyNumber + 1}).PeriodNumber"] = penalty.Period;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty({penaltyNumber + 1}).JamNumber"] = penalty.Jam;
                result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({line.SkaterId}).Penalty({penaltyNumber + 1}).Served"] = penalty.Served;
            }
        }

        return result;
    }

    public Dictionary<string, object?> Map(LineupSheetState state, TeamDetailsState teamDetails, TeamSide side, Guid gameId)
    {
        var result = new Dictionary<string, object?>
        {

        };

        foreach (var jam in state.Jams)
        {
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).NoPivot"] = jam.PivotId == null;

            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Fielding(Jammer).SkaterNumber"] = GetSkaterNumber(jam.JammerId);
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Fielding(Pivot).SkaterNumber"] = GetSkaterNumber(jam.PivotId);
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Fielding(Blocker1).SkaterNumber"] = GetSkaterNumber(jam.BlockerIds[0]);
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Fielding(Blocker2).SkaterNumber"] = GetSkaterNumber(jam.BlockerIds[1]);
            result[$"ScoreBoard.Game({gameId}).Period({jam.Period}).Jam({jam.Jam}).TeamJam({TeamNumber(side)}).Fielding(Blocker3).SkaterNumber"] = GetSkaterNumber(jam.BlockerIds[2]);
        }

        return result;

        string? GetSkaterNumber(Guid? skaterId) =>
            skaterId?.Map(id => teamDetails.Team.Roster.SingleOrDefault(s => s.Id == id))?.Number;
    }

    public Dictionary<string, object?> Map(PenaltyBoxState state, JamLineupState jamLineup, TeamDetailsState teamDetails, TeamSide side, Guid gameId)
    {
        var result = new Dictionary<string, object?>()
        {
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Position(Jammer).PenaltyBox"] = jamLineup.JammerId != null && state.Skaters.Contains((Guid)jamLineup.JammerId!),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Position(Pivot).PenaltyBox"] = jamLineup.PivotId != null && state.Skaters.Contains((Guid)jamLineup.PivotId!),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Position(Blocker1).PenaltyBox"] = jamLineup.BlockerIds[0] != null && state.Skaters.Contains((Guid)jamLineup.BlockerIds[0]!),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Position(Blocker2).PenaltyBox"] = jamLineup.BlockerIds[1] != null && state.Skaters.Contains((Guid)jamLineup.BlockerIds[1]!),
            [$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Position(Blocker3).PenaltyBox"] = jamLineup.BlockerIds[2] != null && state.Skaters.Contains((Guid)jamLineup.BlockerIds[2]!),
        };

        foreach (var skater in teamDetails.Team.Roster)
        {
            result[$"ScoreBoard.Game({gameId}).Team({TeamNumber(side)}).Skater({skater.Id}).PenaltyBox"] = state.Skaters.Contains(skater.Id);
        }

        return result;
    }

    public Dictionary<string, object?> Map(TimeoutListState state, Guid gameId)
    {
        var result = new Dictionary<string, object?> { };

        foreach (var timeout in state.Timeouts)
        {
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).Owner"] = timeout switch
            {
                { Type: TimeoutType.Official } => "O",
                { Type: TimeoutType.Team or TimeoutType.Review, Side: TeamSide.Home } => "1",
                { Type: TimeoutType.Team or TimeoutType.Review, Side: TeamSide.Away } => "2",
                _ => "",
            };
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).Review"] = timeout.Type is TimeoutType.Review;
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).RetainedReview"] = timeout.Retained;
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).Duration"] = timeout.DurationInSeconds * 1000;
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).WalltimeStart"] = new Tick(timeout.EventId.Tick).Millseconds;
            result[$"ScoreBoard.Game({gameId}).Period({timeout.Period}).Timeout({timeout.EventId}).WalltimeEnd"] =
                timeout.DurationInSeconds != null
                    ? new Tick(timeout.EventId.Tick).Millseconds + timeout.DurationInSeconds * 1000
                    : null;

        }

        return result;
    }

    private static int TeamNumber(TeamSide side) =>
        side == TeamSide.Home ? 1 : 2;
}

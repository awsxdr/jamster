//ReSharper disable all

using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;

namespace amethyst.tests.EventHandling;

public static class TestGameEventsSource
{
    public static Event[] JamsWithOverrunningJam => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Wait(15)
        .Event<JamStarted>(10)
        .Event<LeadMarked>(10).WithBody(new LeadMarkedBody(TeamSide.Home, true))
        .Event<JamEnded>(30)
        .Event<JamStarted>(90).GetTick(out var jamStartTick)
        .Event<LeadMarked>(10).WithBody(new LeadMarkedBody(TeamSide.Home, true))
        .Event<JamAutoExpiryDisabled>(10)
        .Wait(30)
        .Validate(tick => [
            new JamClockState(true, jamStartTick, tick - jamStartTick, false, false),
            new GameStageState(Stage.Jam, 1, 2, 2, false)
        ])
        .Event<JamEnded>(10).GetTick(out var jamEndTick)
        .Validate([
            new JamClockState(false, jamStartTick, jamEndTick - jamStartTick, true, false),
            new GameStageState(Stage.Lineup, 1, 2, 2, false)
        ])
        .Build();

    public static Event[] JamsWithLineupsAndPenalties => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Wait(15)
        //Mix of scoreboard and PLT lineup
        .Event<SkaterOnTrack>(0).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterAddedToJam>(1).WithBody(new SkaterAddedToJamBody(TeamSide.Home, 1, 1, HomeTeam.Roster[1].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[2].Number, SkaterPosition.Pivot))
        .Event<SkaterAddedToJam>(0).WithBody(new SkaterAddedToJamBody(TeamSide.Away, 1, 1, AwayTeam.Roster[1].Number, SkaterPosition.Blocker))
        .Event<SkaterAddedToJam>(0).WithBody(new SkaterAddedToJamBody(TeamSide.Home, 1, 1, HomeTeam.Roster[3].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterAddedToJam>(2).WithBody(new SkaterAddedToJamBody(TeamSide.Away, 1, 1, AwayTeam.Roster[2].Number, SkaterPosition.Pivot))
        .Event<SkaterAddedToJam>(1).WithBody(new SkaterAddedToJamBody(TeamSide.Home, 1, 1, HomeTeam.Roster[4].Number, SkaterPosition.Blocker))
        .Event<SkaterAddedToJam>(1).WithBody(new SkaterAddedToJamBody(TeamSide.Away, 1, 1, AwayTeam.Roster[3].Number, SkaterPosition.Blocker))
        .Event<SkaterAddedToJam>(1).WithBody(new SkaterAddedToJamBody(TeamSide.Away, 1, 1, AwayTeam.Roster[4].Number, SkaterPosition.Blocker))
        .Validate([
            ("Home", new JamLineupState(HomeTeam.Roster[0].Number, HomeTeam.Roster[2].Number, [HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, HomeTeam.Roster[4].Number])),
            ("Away", new JamLineupState(AwayTeam.Roster[0].Number, AwayTeam.Roster[2].Number, [AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, AwayTeam.Roster[4].Number])),
        ])
        .Event<JamStarted>(1)
        .Validate([
            ("Home", new JamLineupState(HomeTeam.Roster[0].Number, HomeTeam.Roster[2].Number, [HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, HomeTeam.Roster[4].Number])),
            ("Away", new JamLineupState(AwayTeam.Roster[0].Number, AwayTeam.Roster[2].Number, [AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, AwayTeam.Roster[4].Number])),
        ])
        .Wait(20)
        .Event<LeadMarked>(12).WithBody(new LeadMarkedBody(TeamSide.Home, true))
        .Event<PenaltyAssessed>(1).WithBody(new PenaltyAssessedBody(TeamSide.Away, AwayTeam.Roster[0].Number, "X"))
        .Event<PenaltyAssessed>(1).WithBody(new PenaltyAssessedBody(TeamSide.Home, HomeTeam.Roster[2].Number, "C"))
        .Event<PenaltyAssessed>(1).WithBody(new PenaltyAssessedBody(TeamSide.Away, AwayTeam.Roster[3].Number, "P"))
        .Validate([
            ("Home", GetPenaltySheetState(HomeTeam, [new(HomeTeam.Roster[2].Number, null, [new("C", 1, 1, false)])])),
            ("Away", GetPenaltySheetState(AwayTeam, [
                new(AwayTeam.Roster[0].Number, null, [new("X", 1, 1, false)]),
                new(AwayTeam.Roster[3].Number, null, [new("P", 1, 1, false)]),
            ])),
        ])
        .Event<SkaterSatInBox>(1).GetTick(out var skaterInBoxTick1).WithBody(new SkaterSatInBoxBody(TeamSide.Away, AwayTeam.Roster[0].Number))
        .Event<SkaterSatInBox>(1).GetTick(out var skaterInBoxTick2).WithBody(new SkaterSatInBoxBody(TeamSide.Home, HomeTeam.Roster[2].Number))
        .Validate([
            ("Home", GetPenaltySheetState(HomeTeam, [new(HomeTeam.Roster[2].Number, null, [new("C", 1, 1, true)])])),
            ("Away", GetPenaltySheetState(AwayTeam, [
                new(AwayTeam.Roster[0].Number, null, [new("X", 1, 1, true)]),
                new(AwayTeam.Roster[3].Number, null, [new("P", 1, 1, false)]),
            ])),
        ])
        .Event<SkaterSatInBox>(1).GetTick(out var skaterInBoxTick3).WithBody(new SkaterSatInBoxBody(TeamSide.Away, AwayTeam.Roster[3].Number))
        .Validate(tick => [
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[0].Number, AwayTeam.Roster[3].Number], [])),
            ("Home", new BoxTripsState([new(1, 1, 1, false, false, HomeTeam.Roster[2].Number, SkaterPosition.Pivot, null, false, [], skaterInBoxTick2, 0, tick - skaterInBoxTick2)], false)),
            ("Away", new BoxTripsState([
                new(1, 1, 1, false, false, AwayTeam.Roster[0].Number, SkaterPosition.Jammer, null, false, [], skaterInBoxTick1, 0, tick - skaterInBoxTick1),
                new(1, 1, 1, false, false, AwayTeam.Roster[3].Number, SkaterPosition.Blocker, null, false, [], skaterInBoxTick3, 0, tick - skaterInBoxTick3),
            ], false)),
            ("Home", GetPenaltySheetState(HomeTeam, [new(HomeTeam.Roster[2].Number, null, [new("C", 1, 1, true)])])),
            ("Away", GetPenaltySheetState(AwayTeam, [
                new(AwayTeam.Roster[0].Number, null, [new("X", 1, 1, true)]),
                new(AwayTeam.Roster[3].Number, null, [new("P", 1, 1, true)]),
            ])),
        ])
        .Event<JamEnded>(10).GetTick(out var jamEndTick)
        .Validate([
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[0].Number, AwayTeam.Roster[3].Number], [])),
            ("Home", new BoxTripsState([new(1, 1, 1, false, false, HomeTeam.Roster[2].Number, SkaterPosition.Pivot, null, false, [], skaterInBoxTick2, 0, jamEndTick - skaterInBoxTick2)], false)),
            ("Away", new BoxTripsState([
                new(1, 1, 1, false, false, AwayTeam.Roster[0].Number, SkaterPosition.Jammer, null, false, [], skaterInBoxTick1, 0, jamEndTick - skaterInBoxTick1),
                new(1, 1, 1, false, false, AwayTeam.Roster[3].Number, SkaterPosition.Blocker, null, false, [], skaterInBoxTick3, 0, jamEndTick - skaterInBoxTick3),
            ], false)),
            ("Home", new JamLineupState(null, HomeTeam.Roster[2].Number, [null, null, null])),
            ("Away", new JamLineupState(AwayTeam.Roster[0].Number, null, [AwayTeam.Roster[3].Number, null, null])),
            ("Home", new LineupSheetState([
                new (1, 1, false, HomeTeam.Roster[0].Number, HomeTeam.Roster[2].Number, [HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, HomeTeam.Roster[4].Number]),
                new (1, 2, false, null, HomeTeam.Roster[2].Number, [null, null, null]),
            ])),
            ("Away", new LineupSheetState([
                new (1, 1, false, AwayTeam.Roster[0].Number, AwayTeam.Roster[2].Number, [AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, AwayTeam.Roster[4].Number]),
                new (1, 2, false, AwayTeam.Roster[0].Number, null, [AwayTeam.Roster[3].Number, null, null]),
            ])),
        ])
        .Wait(20)
        .Event<JamStarted>(10).GetTick(out var jamStartTick)
        .Validate(tick => [
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[0].Number, AwayTeam.Roster[3].Number], [])),
            ("Home", new BoxTripsState([
                new(1, 1, 1, false, false, HomeTeam.Roster[2].Number, SkaterPosition.Pivot, null, false, [], jamStartTick, jamEndTick - skaterInBoxTick2, jamEndTick - skaterInBoxTick2 + tick - jamStartTick)
            ], false)),
            ("Away", new BoxTripsState([
                new(1, 1, 1, false, false, AwayTeam.Roster[0].Number, SkaterPosition.Jammer, null, false, [], jamStartTick, jamEndTick - skaterInBoxTick1, jamEndTick - skaterInBoxTick1 + tick - jamStartTick),
                new(1, 1, 1, false, false, AwayTeam.Roster[3].Number, SkaterPosition.Blocker, null, false, [], jamStartTick, jamEndTick - skaterInBoxTick3, jamEndTick - skaterInBoxTick3 + tick - jamStartTick),
            ], false)),
            ("Home", new JamLineupState(null, HomeTeam.Roster[2].Number, [null, null, null])),
            ("Away", new JamLineupState(AwayTeam.Roster[0].Number, null, [AwayTeam.Roster[3].Number, null, null])),
        ])
        .Event<SkaterReleasedFromBox>(1).GetTick(out var skaterReleasedTick1).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Away, AwayTeam.Roster[0].Number))
        .Event<SkaterReleasedFromBox>(1).GetTick(out var skaterReleasedTick2).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Home, HomeTeam.Roster[2].Number))
        .Validate(tick => [
            ("Home", new BoxTripsState([
                new(1, 1, 1, false, false, HomeTeam.Roster[2].Number, SkaterPosition.Pivot, 1, false, [], jamStartTick, jamEndTick - skaterInBoxTick2, jamEndTick - skaterInBoxTick2 + skaterReleasedTick2 - jamStartTick)
            ], false)),
            ("Away", new BoxTripsState([
                new(1, 1, 1, false, false, AwayTeam.Roster[0].Number, SkaterPosition.Jammer, 1, false, [], jamStartTick, jamEndTick - skaterInBoxTick1, jamEndTick - skaterInBoxTick1 + skaterReleasedTick1 - jamStartTick),
                new(1, 1, 1, false, false, AwayTeam.Roster[3].Number, SkaterPosition.Blocker, null, false, [], jamStartTick, jamEndTick - skaterInBoxTick3, jamEndTick - skaterInBoxTick3 + tick - jamStartTick),
            ], false)),
            ("Home", new PenaltyBoxState([], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[3].Number], [])),
        ])
        .Event<JamEnded>(10).GetTick(out var jamEndTick2)
        .Validate([
            ("Home", new JamLineupState(null, null, [null, null, null])),
            ("Away", new JamLineupState(null, null, [AwayTeam.Roster[3].Number, null, null])),
        ])
        .Event<SkaterSubstitutedInBox>(1).WithBody(new SkaterSubstitutedInBoxBody(TeamSide.Away, AwayTeam.Roster[3].Number, AwayTeam.Roster[5].Number))
        .Validate([
            ("Home", new BoxTripsState([
                new(1, 1, 1, false, false, HomeTeam.Roster[2].Number, SkaterPosition.Pivot, 1, false, [], jamStartTick, jamEndTick - skaterInBoxTick2, jamEndTick - skaterInBoxTick2 + skaterReleasedTick2 - jamStartTick)
            ], false)),
            ("Away", new BoxTripsState([
                new(1, 1, 1, false, false, AwayTeam.Roster[0].Number, SkaterPosition.Jammer, 1, false, [], jamStartTick, jamEndTick - skaterInBoxTick1, jamEndTick - skaterInBoxTick1 + skaterReleasedTick1 - jamStartTick),
                new(1, 1, 1, false, false, AwayTeam.Roster[3].Number, SkaterPosition.Blocker, null, false, [new(AwayTeam.Roster[5].Number, 3)], jamStartTick, jamEndTick - skaterInBoxTick3, jamEndTick - skaterInBoxTick3 + jamEndTick2 - jamStartTick),
            ], false)),
            ("Home", new PenaltyBoxState([], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[5].Number], [])),
            ("Home", new JamLineupState(null, null, [null, null, null])),
            ("Away", new JamLineupState(null, null, [AwayTeam.Roster[5].Number, null, null])),
        ])
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[2].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[3].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[4].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[2].Number, SkaterPosition.Blocker))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[3].Number, SkaterPosition.Blocker))
        .Validate([
            ("Home", new PenaltyBoxState([], [])),
            ("Away", new PenaltyBoxState([AwayTeam.Roster[5].Number], [])),
            ("Home", new JamLineupState(HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, [HomeTeam.Roster[2].Number, HomeTeam.Roster[3].Number, HomeTeam.Roster[4].Number])),
            ("Away", new JamLineupState(AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, [AwayTeam.Roster[5].Number, AwayTeam.Roster[2].Number, AwayTeam.Roster[3].Number])),
        ])
        .Event<JamStarted>(30)
        .Event<SkaterReleasedFromBox>(1).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Away, AwayTeam.Roster[5].Number))
        .Event<PenaltyAssessed>(5).WithBody(new PenaltyAssessedBody(TeamSide.Home, HomeTeam.Roster[3].Number, "X"))
        .Validate([
            ("Home", new PenaltyBoxState([], [HomeTeam.Roster[3].Number])),
        ])
        .Event<PenaltyAssessed>(5).WithBody(new PenaltyAssessedBody(TeamSide.Home, HomeTeam.Roster[2].Number, "F"))
        .Validate([
            ("Home", new PenaltyBoxState([], [HomeTeam.Roster[3].Number, HomeTeam.Roster[2].Number])),
        ])
        .Event<SkaterSatInBox>(1).WithBody(new SkaterSatInBoxBody(TeamSide.Home, HomeTeam.Roster[2].Number))
        .Validate([
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number], [HomeTeam.Roster[3].Number])),
        ])
        .Event<JamEnded>(5)
        .Validate([
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number], [HomeTeam.Roster[3].Number])),
            ("Home", new JamLineupState(null, null, [HomeTeam.Roster[3].Number, HomeTeam.Roster[2].Number, null])),
        ])
        .Event<PenaltyAssessed>(1).WithBody(new PenaltyAssessedBody(TeamSide.Away, AwayTeam.Roster[2].Number, "C"))
        .Validate([
            ("Away", new PenaltyBoxState([], [AwayTeam.Roster[2].Number])),
            ("Away", new JamLineupState(null, null, [AwayTeam.Roster[2].Number, null, null])),
        ])
        .Event<SkaterSatInBox>(1).WithBody(new SkaterSatInBoxBody(TeamSide.Away, AwayTeam.Roster[2].Number))
        .Validate([
            ("Away", new PenaltyBoxState([AwayTeam.Roster[2].Number], [])),
            ("Away", new JamLineupState(null, null, [AwayTeam.Roster[2].Number, null, null])),
        ])
        .Event<SkaterSatInBox>(1).WithBody(new SkaterSatInBoxBody(TeamSide.Home, HomeTeam.Roster[3].Number))
        .Validate([
            ("Home", new PenaltyBoxState([HomeTeam.Roster[2].Number, HomeTeam.Roster[3].Number], [])),
            ("Home", new JamLineupState(null, null, [HomeTeam.Roster[3].Number, HomeTeam.Roster[2].Number, null])),
        ])
        .Wait(22)
        .Event<JamStarted>(30)
        .Event<SkaterReleasedFromBox>(1).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Home, HomeTeam.Roster[2].Number))
        .Event<SkaterReleasedFromBox>(1).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Home, HomeTeam.Roster[3].Number))
        .Event<SkaterReleasedFromBox>(1).WithBody(new SkaterReleasedFromBoxBody(TeamSide.Away, AwayTeam.Roster[2].Number))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[0].Number, SkaterPosition.Blocker))
        .Wait(30)
        .Event<PenaltyAssessed>(0).WithBody(new PenaltyAssessedBody(TeamSide.Home, HomeTeam.Roster[0].Number, "X"))
        .Event<JamEnded>(0)
        .Event<SkaterSatInBox>(10).WithBody(new SkaterSatInBoxBody(TeamSide.Home, HomeTeam.Roster[0].Number))
        .Validate([
            ("Home", new PenaltyBoxState([HomeTeam.Roster[0].Number], [])),
            ("Home", new JamLineupState(null, null, [HomeTeam.Roster[0].Number, null, null])),
        ])
        .Build();

    public static Event[] JamsWithScoresAndStats => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Validate(
            new GameStageState(Stage.BeforeGame, 1, 0, 0, false),
            ("Home", new TeamDetailsState(new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster))),
            ("Away", new TeamDetailsState(new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        )
        .Wait(10)
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<IntermissionEnded>(30)
        .Validate(
            new GameStageState(Stage.Lineup, 1, 0, 0, false),
            ("Home", new ScoreSheetState([])),
            ("Away", new ScoreSheetState([]))
        )
        .Event<JamStarted>(12)
        .Validate(
            ("Home", new TeamScoreState(0, 0)),
            ("Away", new TeamScoreState(0, 0)),
            ("Home", new TripScoreState(null, 0, 0)),
            ("Away", new TripScoreState(null, 0, 0)),
            ("Home", new TeamJamStatsState(false, false, false, false, false)),
            ("Away", new TeamJamStatsState(false, false, false, false, false)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, false, false, false, true, [], null, 0, 0)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, true, [], null, 0, 0)]))
        )
        .Event<LeadMarked>(2).WithBody(new LeadMarkedBody(TeamSide.Home, true))
        .Event<InitialTripCompleted>(10).WithBody(new InitialTripCompletedBody(TeamSide.Away, true))
        .Event<ScoreModifiedRelative>(0).GetTick(out var homeScoreTick).WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 4))
        .Validate(
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(0, 0)),
            ("Home", new TripScoreState(4, 1, homeScoreTick)),
            ("Away", new TripScoreState(null, 1, 0)),
            ("Home", new TeamJamStatsState(true, false, false, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, true)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, false, false, false, [new JamLineTrip(4)], null, 4, 4)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(null)], null, 0, 0)]))
        )
        .Wait(2)
        .Event<ScoreModifiedRelative>(0).GetTick(out var awayScoreTick).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4))
        .Validate(
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(4, 1, homeScoreTick)),
            ("Away", new TripScoreState(4, 1, awayScoreTick)),
            ("Home", new TeamJamStatsState(true, false, false, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, true)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, false, false, false, [new JamLineTrip(4)], null, 4, 4)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4)], null, 4, 4)]))
        )
        .Wait(2)
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(null, 2, tick - 1000)),
            ("Away", new TripScoreState(4, 1, awayScoreTick)),
            ("Home", new TeamJamStatsState(true, false, false, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, true)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, false, false, false, [new JamLineTrip(4), new JamLineTrip(null)], null, 4, 4)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4)], null, 4,  4)]))
        ])
        .Wait(2)
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(null, 2, tick - 3000)),
            ("Away", new TripScoreState(null, 2, tick - 1000)),
            ("Home", new TeamJamStatsState(true, false, false, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, true)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, false, false, false, [new JamLineTrip(4), new JamLineTrip(null)], null, 4, 4)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(null)], null, 4,  4)]))
        ])
        .Wait(15)
        .Event<JamEnded>(2)
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 3))
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 1))
        .Validate(
            ("Home", new TeamScoreState(7, 7)),
            ("Away", new TeamScoreState(5, 5)),
            ("Home", new TeamJamStatsState(true, false, true, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, true)),
            ("Home", new ScoreSheetState([new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(3)], null, 7, 7)])),
            ("Away", new ScoreSheetState([new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(1)], null, 5, 5)]))
        )
        .Wait(20)
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[1].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[3].Number, SkaterPosition.Pivot))
        .Wait(8)
        .Event<JamStarted>(2)
        .Validate(
            ("Home", new TeamScoreState(7, 0)),
            ("Away", new TeamScoreState(5, 0)),
            ("Home", new ScoreSheetState([
                new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(3)], null, 7, 7),
                new(1, 2, HomeTeam.Roster[1].Number, "?", false, false, false, false, true, [], null, 0, 7),
            ])),
            ("Away", new ScoreSheetState([
                new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(1)], null, 5, 5),
                new(1, 2, "?", AwayTeam.Roster[3].Number, false, false, false, false, true, [], null, 0, 5),
            ]))
        )
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[3].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[1].Number, SkaterPosition.Jammer))
        .Event<LeadMarked>(1).WithBody(new LeadMarkedBody(TeamSide.Away, true))
        .Validate(
            ("Home", new ScoreSheetState([
                new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(3)], null, 7, 7),
                new(1, 2, HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, false, false, false, false, true, [], null, 0, 7),
            ])),
            ("Away", new ScoreSheetState([
                new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(1)], null, 5, 5),
                new(1, 2, AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, false, true, false, false, false, [new JamLineTrip(null)], null, 0, 5),
            ]))
        )
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4))
        .Validate(
            ("Home", new TeamScoreState(7, 0)),
            ("Away", new TeamScoreState(9, 4)),
            ("Home", new ScoreSheetState([
                new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(3)], null, 7, 7),
                new(1, 2, HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, false, false, false, false, true, [], null, 0, 7),
            ])),
            ("Away", new ScoreSheetState([
                new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(1)], null, 5, 5),
                new(1, 2, AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, false, true, false, false, false, [new JamLineTrip(4)], null, 4, 9),
            ]))
        )
        .Wait(5)
        .Event<CallMarked>(2).WithBody(new CallMarkedBody(TeamSide.Away, true))
        .Validate(
            ("Home", new ScoreSheetState([
                new(1, 1, HomeTeam.Roster[0].Number, HomeTeam.Roster[1].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(3)], null, 7, 7),
                new(1, 2, HomeTeam.Roster[1].Number, HomeTeam.Roster[3].Number, false, false, false, false, true, [], null, 0, 7),
            ])),
            ("Away", new ScoreSheetState([
                new(1, 1, AwayTeam.Roster[0].Number, AwayTeam.Roster[1].Number, false, false, false, false, false, [new JamLineTrip(4), new JamLineTrip(1)], null, 5, 5),
                new(1, 2, AwayTeam.Roster[1].Number, AwayTeam.Roster[3].Number, false, true, true, false, false, [new JamLineTrip(4), new JamLineTrip(0)], null, 4, 9),
            ]))
        )
        .Wait(24)
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[0].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[1].Number, SkaterPosition.Pivot))
        .Event<JamStarted>(30)
        .Event<LeadMarked>(1).WithBody(new LeadMarkedBody(TeamSide.Home, true))
        .Wait(100)
        .Validate([
            ("Home", new TeamJamStatsState(true, false, false, false, true)),
            ("Away", new TeamJamStatsState(false, false, false, false, false)),
        ])
        .Build();

    public static Event[] SingleJamStartedWithoutEndingIntermission => new EventsBuilder(0, [])
        .Validate(new GameStageState(Stage.BeforeGame, 1, 0, 0, false))
        .Event<JamStarted>(30)
        .Validate(new GameStageState(Stage.Jam, 1, 1, 1, false))
        .Build();

    public static Event[] OfficialReviewDuringIntermission => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<IntermissionClockSet>(0).WithBody(new IntermissionClockSetBody(120))
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(90)
        .Event<JamEnded>(30)
        .Event<JamStarted>(120)
        .Event<JamEnded>(30)
        .Event<JamStarted>(150)
        .Wait(0)
        .Validate(
            new GameStageState(Stage.Intermission, 1, 15, 15, false),
            new IntermissionClockState(
                true, 
                false,
                Tick.FromSeconds(120), 
                Tick.FromSeconds(120 * 14 + 150) + Tick.FromSeconds(120),
                90)
        )
        .Event<TimeoutStarted>(1)
        .Event<TimeoutTypeSet>(90).WithBody(new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 1, 15, 15, false),
            new TimeoutClockState(true, tick - Tick.FromSeconds(91), 0, TimeoutClockStopReason.None, Tick.FromSeconds(91)),
            new IntermissionClockState(false, false, Tick.FromSeconds(120), Tick.FromSeconds(120 * 14 + 150) + Tick.FromSeconds(120), 90)
        ])
        .Event<TimeoutEnded>(15)
        .Validate(tick => [
            new GameStageState(Stage.Intermission, 1, 15, 15, false),
            new TimeoutClockState(true, tick - Tick.FromSeconds(106), tick - Tick.FromSeconds(15), TimeoutClockStopReason.Other, Tick.FromSeconds(106)),
            new IntermissionClockState(true, false, Tick.FromSeconds(120), tick + Tick.FromSeconds(105), 120 - 15)
        ])
        .Build();

    private static readonly Ruleset CustomRuleset = new(
        PeriodRules: new(
            PeriodCount: 5,
            DurationInSeconds: 10 * 60,
            PeriodEndBehavior.Immediately),
        JamRules: new(
            ResetJamNumbersBetweenPeriods: false,
            DurationInSeconds: 60),
        LineupRules: new(
            DurationInSeconds: 45,
            OvertimeDurationInSeconds: 2 * 60),
        TimeoutRules: new(
            TeamTimeoutDurationInSeconds: 30,
            PeriodClockBehavior: TimeoutPeriodClockStopBehavior.OfficialTimeout,
            TeamTimeoutAllowance: 5,
            ResetBehavior: TimeoutResetBehavior.Period),
        PenaltyRules: new(
            FoulOutPenaltyCount: 10),
        IntermissionRules: new(
            DurationInSeconds: 5 * 60),
        InjuryRules: new(
            JamsToSitOutFollowingInjury: 3,
            NumberOfAllowableInjuriesPerPeriod: 2)
    );

    public static Event[] CustomRules => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Event<RulesetSet>(0).WithBody(new RulesetSetBody(CustomRuleset))
        .Wait(30)
        .Event<JamStarted>(0).GetTick(out var periodStartTick)
        .Validate(tick => [
            new GameStageState(Stage.Jam, 1, 1, 1, false),
            new JamClockState(true, tick, 0, true, false),
            new PeriodClockState(true, false, tick, 0, 0),
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        ])
        .Wait(61)
        .Validate(tick => [
            new GameStageState(Stage.Lineup, 1, 1, 1, false),
            new LineupClockState(true, tick - 1000, 1000),
        ])
        .Event<TimeoutStarted>(1).GetTick(out var timeoutStartTick)
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 1, 1, 1, false),
            new TimeoutClockState(true, timeoutStartTick, 0, TimeoutClockStopReason.None, tick - timeoutStartTick),
            new PeriodClockState(true, false, periodStartTick, 0, tick - periodStartTick),
        ])
        .Event<TimeoutTypeSet>(1).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 1, 1, 1, false),
            new TimeoutClockState(true, timeoutStartTick, 0, TimeoutClockStopReason.None, tick - timeoutStartTick),
            new PeriodClockState(false, false, periodStartTick, 0, timeoutStartTick - periodStartTick),
        ])
        .Wait(10)
        .Event<TimeoutTypeSet>(1).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 1, 1, 1, false),
            new TimeoutClockState(true, timeoutStartTick, 0, TimeoutClockStopReason.None, tick - timeoutStartTick),
            new PeriodClockState(true, false, periodStartTick, 0, tick - periodStartTick)
        ])
        .Wait(539) // Need to force a couple of Ticks
        .Wait(1)
        .Validate([
            new GameStageState(Stage.Intermission, 1, 1, 1, false),
            new TimeoutClockState(false, timeoutStartTick, periodStartTick + Tick.FromSeconds(60 * 10), TimeoutClockStopReason.PeriodExpired, Tick.FromSeconds(60 * 10) - (timeoutStartTick - periodStartTick)),
            new PeriodClockState(false, true, periodStartTick, 0, Tick.FromSeconds(60 * 10)),
            new IntermissionClockState(true, false, Tick.FromSeconds(5 * 60), periodStartTick + Tick.FromSeconds(10 * 60) + Tick.FromSeconds(5 * 60), 5 * 60 - 14)
        ])
        .Event<TimeoutTypeSet>(1).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 1, 1, 1, false),
            new TimeoutClockState(true, timeoutStartTick, 0, TimeoutClockStopReason.None, tick - timeoutStartTick),
            new PeriodClockState(false, false, periodStartTick, 0, timeoutStartTick - periodStartTick),
        ])
        .Event<TimeoutTypeSet>(1).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate([
            new GameStageState(Stage.Intermission, 1, 1, 1, false),
            new TimeoutClockState(false, timeoutStartTick, periodStartTick + Tick.FromSeconds(60 * 10), TimeoutClockStopReason.PeriodExpired, Tick.FromSeconds(60 * 10) - (timeoutStartTick - periodStartTick)),
            new PeriodClockState(false, true, periodStartTick, 0, Tick.FromSeconds(60 * 10)),
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout))
        ])
        .Wait(120)
        .Event<PeriodFinalized>(1)
        .Event<JamStarted>(30)
        .Validate(
            new GameStageState(Stage.Jam, 2, 2, 2, false),
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<JamEnded>(30)
        .Event<JamStarted>(30)
        .Validate(
            new GameStageState(Stage.Jam, 2, 3, 3, false)
        )
        .Wait(550)
        .Validate(
            new GameStageState(Stage.Intermission, 2, 3, 3, false)
        )
        .Event<PeriodFinalized>(1)
        .Event<JamStarted>(30)
        .Event<JamEnded>(30)
        .Wait(520)
        .Event<JamStarted>(30)
        .Validate(
            new GameStageState(Stage.Intermission, 3, 5, 5, false)
        )
        .Event<RulesetSet>(0).WithBody(new RulesetSetBody(
            CustomRuleset with
            {
                PeriodRules = CustomRuleset.PeriodRules with
                {
                    PeriodEndBehavior = PeriodEndBehavior.Manual
                },
                JamRules = CustomRuleset.JamRules with
                {
                    ResetJamNumbersBetweenPeriods = true,
                }
            }
        ))
        .Event<PeriodFinalized>(1)
        .Event<JamStarted>(30).GetTick(out var periodStartTick2)
        .Event<JamEnded>(30)
        .Validate(
            new GameStageState(Stage.Lineup, 4, 1, 6, false)
        )
        .Wait(550)
        .Validate(tick => [
            new GameStageState(Stage.Lineup, 4, 1, 6, false),
            new PeriodClockState(true, true, periodStartTick2, 0, tick - periodStartTick2),
        ])
        .Event<PeriodEnded>(1).GetTick(out var periodEndTick)
        .Validate(
            new GameStageState(Stage.Intermission, 4, 1, 6, false),
            new PeriodClockState(false, true, periodStartTick2, 0, periodEndTick - periodStartTick2)
        )
        .Event<RulesetSet>(0).WithBody(new RulesetSetBody(
            CustomRuleset with
            {
                PeriodRules = CustomRuleset.PeriodRules with
                {
                    PeriodEndBehavior = PeriodEndBehavior.OnJamEnd,
                    DurationInSeconds = 7 * 60
                },
            }
        ))
        .Event<PeriodFinalized>(1)
        .Event<JamStarted>(30).GetTick(out var periodStartTick3)
        .Event<JamEnded>(30)
        .Validate(
            new GameStageState(Stage.Lineup, 5, 2, 7, false)
        )
        .Wait(7 * 60 - 50)
        .Validate(tick => [
            new GameStageState(Stage.Lineup, 5, 2, 7, false),
            new PeriodClockState(true, true, periodStartTick3, 0, Tick.FromSeconds(7 * 60)),
        ])
        .Event<JamStarted>(30)
        .Validate(
            new GameStageState(Stage.Jam, 5, 3, 8, false)
        )
        .Event<JamEnded>(1)
        .Validate(
            new GameStageState(Stage.AfterGame, 5, 3, 8, false),
            new PeriodClockState(false, true, periodStartTick3, 0, Tick.FromSeconds(7 * 60))
        )
        .Build();

    public static Event[] FullGame => new EventsBuilder(0, [])
        .Validate(new GameStageState(Stage.BeforeGame, 1, 0, 0, false))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        .Validate(
            new GameStageState(Stage.BeforeGame, 1, 0, 0, false),
            ("Home", new TeamDetailsState(new GameTeam(HomeTeam.Names, HomeTeam.Color, HomeTeam.Roster))),
            ("Away", new TeamDetailsState(new GameTeam(AwayTeam.Names, AwayTeam.Color, AwayTeam.Roster)))
        )
        .Event<IntermissionClockSet>(10).WithBody(new IntermissionClockSetBody(10 * 60))
        .Validate(new IntermissionClockState(true, false, Tick.FromSeconds(10 * 60), Tick.FromSeconds(10 * 60), 10 * 60 - 10))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[3].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[6].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[2].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[7].Number, SkaterPosition.Pivot))
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Home", new JamLineupState(HomeTeam.Roster[3].Number, HomeTeam.Roster[6].Number, [null, null, null])),
            ("Away", new JamLineupState(AwayTeam.Roster[2].Number, AwayTeam.Roster[7].Number, [null, null, null]))
        )
        // Jam 1
        .Event<JamStarted>(120 + 30) // Jam that runs for full duration
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Home", new JamLineupState(null, null, [null, null, null])),
            ("Away", new JamLineupState(null, null, [null, null, null]))
        )
        .Wait(0)
        // Jam 2
        .Event<JamStarted>(57) // Jam that is called
        .Validate(new GameStageState(Stage.Jam, 1, 2, 2, false))
        .Event<JamEnded>(30)
        // Jam 3
        .Event<JamStarted>(95)
        .Event<JamEnded>(25)
        .Event<TimeoutStarted>(1) // Team timeout
        .Event<TimeoutTypeSet>(60).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(25)
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        // Jam 4
        .Event<JamStarted>(30)
        .Event<JamEnded>(30)
        // Jam 5
        .Event<JamStarted>(107)
        .Event<JamStarted>(1) // Invalid jam start
        .Event<JamEnded>(30)
        // Jam 6
        .Event<JamStarted>(145).GetTick(out var jamStartTick)
        .Validate([
            new GameStageState(Stage.Lineup, 1, 6, 6, false),
            new JamClockState(false, jamStartTick, Rules.DefaultRules.JamRules.DurationInSeconds * Tick.TicksPerSecond, true, true),
        ])
        .Event<TimeoutStarted>(1) // Official timeout
        .Event<TimeoutTypeSet>(218).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(15)
        // Jam 7
        .Event<JamStarted>(82)
        .Event<JamEnded>(31) // Overrunning lineup
        // Jam 8
        .Event<JamStarted>(58)
        .Event<JamEnded>(30)
        // Jam 9
        .Event<JamStarted>(120 + 15)
        .Wait(1)
        .Validate(new GameStageState(Stage.Lineup, 1, 9, 9, false))
        .Event<TimeoutStarted>(90) // Timeout not ended
        // Jam 10
        .Event<JamStarted>(76)
        .Event<JamEnded>(30)
        // Jam 11
        .Event<JamStarted>(112)
        .Event<JamEnded>(30)
        .Event<TimeoutStarted>(1)
        .Event<TimeoutTypeSet>(10).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Away))
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout))
        )
        .Event<TimeoutTypeSet>(10).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        // Jam 12
        .Event<JamStarted>(121)
        .Event<JamEnded>(29) // Late jam ended
        // Jam 13
        .Event<JamStarted>(68)
        .Event<JamEnded>(30)
        // Jam 14
        .Event<JamStarted>(60)
        .Event<JamEnded>(30)
        // Jam 15
        .Event<JamStarted>(93)
        .Event<JamEnded>(30)
        // Jam 16
        .Event<JamStarted>(112)
        .Event<JamEnded>(1)
        .Validate(new GameStageState(Stage.Intermission, 1, 16, 16, false))
        .Event<PeriodFinalized>(10 * 60)
        .Validate(new GameStageState(Stage.Intermission, 2, 0, 16, true))
        .Validate(tick => [new IntermissionClockState(true, false, Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), tick + (5 * 60 - 1) * 1000, 5 * 60 - 1)])
        .Wait(5 * 60)
        .Validate(tick => [new IntermissionClockState(true, true, Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds), tick - 1000, 0)])
        .Event<IntermissionEnded>(25)
        .Event<JamStarted>(94)
        .Validate(new GameStageState(Stage.Jam, 2, 1, 17, false))
        .Event<JamEnded>(30)
        .Event<JamStarted>(120 + 15)
        .Validate(new GameStageState(Stage.Lineup, 2, 2, 18, false))
        .Event<TimeoutStarted>(1) // Multiple timeouts
        .Event<TimeoutTypeSet>(160).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutStarted>(1)
        .Event<TimeoutTypeSet>(60).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(16)
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutStarted>(7)
        .Event<TimeoutTypeSet>(0).WithBody(new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 2, 2, 18, false),
            new TimeoutClockState(true, tick - 7000, 0, TimeoutClockStopReason.None, 7000),
            new PeriodClockState(
                false,
                false,
                tick - (94 + 30 + 120 + 15 + 160 + 60 + 16 + 7 + 2) * 1000,
                0,
                (94 + 30 + 120 + 15) * 1000),
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
        ])
        .Wait(90)
        .Event<JamStarted>(109)
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            new GameStageState(Stage.Jam, 2, 3, 19, false)
        )
        .Event<JamEnded>(30)
        .Event<JamStarted>(35)
        .Event<TimeoutStarted>(612) // Timeout started mid-jam (injury, for example)
        .Event<JamStarted>(10)
        .Validate(tick => [
            new GameStageState(Stage.Jam, 2, 5, 21, false),
            new JamClockState(true, tick - 10000, 10000, true, false),
            new PeriodClockState(
                true, 
                false,
                tick - 10000,
                (94 + 30 + 120 + 15 + 109 + 30 + 35) * 1000,
                (94 + 30 + 120 + 15 + 109 + 30 + 35 + 10) * 1000),
        ])
        .Wait(120 + 30)
        .Event<JamEnded>(30)
        .Event<JamStarted>(42)
        .Validate(
            new GameStageState(Stage.Jam, 2, 6, 22, false)
        )
        .Event<JamEnded>(30)
        .Event<JamStarted>(59)
        .Event<JamEnded>(30)
        .Event<TimeoutStarted>(1)
        .Event<TimeoutTypeSet>(123).WithBody(new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Away))
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.Review))
        )
        .Event<TimeoutEnded>(0)
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(0, ReviewStatus.Used, TimeoutInUse.None))
        )
        .Wait(15)
        .Event<JamStarted>(119) // Jam at nearly full length
        .Event<JamEnded>(30)
        .Event<JamStarted>(52)
        .Event<JamEnded>(15)
        .Validate(tick => [
            new GameStageState(Stage.Lineup, 2, 9, 25, false),
            new LineupClockState(true, tick - 15000, 15000),
        ])
        .Wait(15)
        .Event<JamStarted>(49)
        .Event<JamEnded>(30)
        .Event<JamStarted>(72)
        .Event<JamEnded>(30)
        .Event<JamStarted>(95)
        .Event<JamEnded>(30)
        .Event<JamStarted>(27)
        .Event<JamEnded>(30)
        .Event<JamStarted>(20)
        .Event<JamEnded>(30)
        .Event<JamStarted>(97)
        .Event<JamEnded>(30)
        .Event<JamStarted>(58)
        .Event<JamEnded>(30)
        .Event<JamStarted>(73)
        .Event<JamEnded>(30)
        .Event<JamStarted>(52)
        .Event<JamEnded>(30)
        .Event<JamStarted>(30)
        .Event<JamEnded>(22)
        .Validate(new GameStageState(Stage.AfterGame, 2, 19, 35, false))
        .Wait(10)
        .Event<PeriodFinalized>(1)
        .Validate(new GameStageState(Stage.AfterGame, 2, 19, 35, true))
        .Build();

    public static readonly GameTeam HomeTeam = new(
        new() { ["team"] =  "Test Home Team" },
        new TeamColor(Color.Black, Color.White),
        [
            new("0", "Test Skater 1", true),
            new("12", "Test Skater 2", true),
            new("267", "Test Skater 3", true),
            new("3876", "Test Skater 4", true),
            new("4", "Test Skater 5", true),
            new("52", "Test Skater 6", true),
            new("697", "Test Skater 7", true),
            new("7293", "Test Skater 8", true),
            new("8", "Test Skater 9", true),
            new("90", "Test Skater 10", true),
        ]);

    public static readonly GameTeam AwayTeam = new(
        new() { ["team"] = "Test Away Team" },
        new TeamColor(Color.White, Color.Black),
        [
            new("0583", "Test Skater 1", true),
            new("183", "Test Skater 2", true),
            new("28", "Test Skater 3", true),
            new("3", "Test Skater 4", true),
            new("4957", "Test Skater 5", true),
            new("572", "Test Skater 6", true),
            new("60", "Test Skater 7", true),
            new("7", "Test Skater 8", true),
            new("8273", "Test Skater 9", true),
            new("984", "Test Skater 10", true),
        ]);

    public static PenaltySheetState GetPenaltySheetState(GameTeam team, amethyst.Reducers.PenaltySheetLine[] modifiedLines) =>
        new(
            team.Roster.Select(s =>
                    new amethyst.Reducers.PenaltySheetLine(
                        s.Number,
                        null,
                        modifiedLines.SingleOrDefault(l => l.SkaterNumber == s.Number)?.Penalties ?? []))
                .ToArray()
        );
}
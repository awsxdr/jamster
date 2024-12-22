using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;

namespace amethyst.tests.EventHandling;

public static class TestGameEventsSource
{
    public static Event[] TwoJamsWithScores => new EventsBuilder(0, [])
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Colors["Black"], HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Colors["White"], AwayTeam.Roster)))
        .Validate(
            new GameStageState(Stage.BeforeGame, 0, 0, false),
            ("Home", new TeamDetailsState(new GameTeam(HomeTeam.Names, HomeTeam.Colors["Black"], HomeTeam.Roster))),
            ("Away", new TeamDetailsState(new GameTeam(AwayTeam.Names, AwayTeam.Colors["White"], AwayTeam.Roster)))
        )
        .Event<IntermissionEnded>(15)
        .Validate(new GameStageState(Stage.Lineup, 1, 0, false))
        .Event<JamStarted>(12)
        // TODO: Lead jammer
        .Wait(15)
        .Validate(
            ("Home", new TeamScoreState(0, 0)),
            ("Away", new TeamScoreState(0, 0)),
            ("Home", new TripScoreState(null, 0)),
            ("Away", new TripScoreState(null, 0))
        )
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 4))
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(0, 0)),
            ("Home", new TripScoreState(4, tick)),
            ("Away", new TripScoreState(null, 0))
        ])
        .Wait(2)
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4))
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(4, tick - 2000)),
            ("Away", new TripScoreState(4, tick))
        ])
        .Wait(2)
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(null, tick - 1000)),
            ("Away", new TripScoreState(4, tick - 2000))
        ])
        .Wait(2)
        .Validate(tick => [
            ("Home", new TeamScoreState(4, 4)),
            ("Away", new TeamScoreState(4, 4)),
            ("Home", new TripScoreState(null, tick - 3000)),
            ("Away", new TripScoreState(null, tick - 1000))
        ])
        .Wait(15)
        .Event<JamEnded>(2)
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 3))
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 1))
        .Validate(
            ("Home", new TeamScoreState(7, 7)),
            ("Away", new TeamScoreState(5, 5))
        )
        .Event<JamStarted>(2)
        .Validate(
            ("Home", new TeamScoreState(7, 0)),
            ("Away", new TeamScoreState(5, 0))
        )
        .Event<ScoreModifiedRelative>(0).WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4))
        .Validate(
            ("Home", new TeamScoreState(7, 0)),
            ("Away", new TeamScoreState(9, 4))
        )
        .Build();

    public static Event[] SingleJamStartedWithoutEndingIntermission => new EventsBuilder(0, [])
        .Validate(new GameStageState(Stage.BeforeGame, 0, 0, false))
        .Event<JamStarted>(30)
        .Validate(new GameStageState(Stage.Jam, 1, 1, false))
        .Build();

    public static Event[] FullGame => new EventsBuilder(0, [])
        .Validate(new GameStageState(Stage.BeforeGame, 0, 0, false))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Home, new GameTeam(HomeTeam.Names, HomeTeam.Colors["Black"], HomeTeam.Roster)))
        .Event<TeamSet>(0).WithBody(new TeamSetBody(TeamSide.Away, new GameTeam(AwayTeam.Names, AwayTeam.Colors["White"], AwayTeam.Roster)))
        .Validate(
            new GameStageState(Stage.BeforeGame, 0, 0, false),
            ("Home", new TeamDetailsState(new GameTeam(HomeTeam.Names, HomeTeam.Colors["Black"], HomeTeam.Roster))),
            ("Away", new TeamDetailsState(new GameTeam(AwayTeam.Names, AwayTeam.Colors["White"], AwayTeam.Roster)))
        )
        .Event<IntermissionEnded>(15)
        .Validate(new GameStageState(Stage.Lineup, 1, 0, false))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[3].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Home, HomeTeam.Roster[6].Number, SkaterPosition.Pivot))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[2].Number, SkaterPosition.Jammer))
        .Event<SkaterOnTrack>(1).WithBody(new SkaterOnTrackBody(TeamSide.Away, AwayTeam.Roster[7].Number, SkaterPosition.Pivot))
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Home", new JamLineupState(HomeTeam.Roster[3].Number, HomeTeam.Roster[6].Number)),
            ("Away", new JamLineupState(AwayTeam.Roster[2].Number, AwayTeam.Roster[7].Number))
        )
        .Event<JamStarted>(120 + 30) // Jam that runs for full duration
        .Validate(
            ("Home", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Home", new JamLineupState(null, null)),
            ("Away", new JamLineupState(null, null))
        )
        .Wait(0)
        .Event<JamStarted>(57) // Jam that is called
        .Validate(new GameStageState(Stage.Jam, 1, 2, false))
        .Event<JamEnded>(30)
        .Event<JamStarted>(95)
        .Event<JamEnded>(25)
        .Event<TimeoutStarted>(0) // Team timeout
        .Event<TimeoutTypeSet>(60).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(25)
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<JamStarted>(30)
        .Event<JamEnded>(30)
        .Event<JamStarted>(107)
        .Event<JamStarted>(1) // Invalid jam start
        .Event<JamEnded>(30)
        .Event<JamStarted>(145)
        .Event<TimeoutStarted>(0) // Official timeout
        .Event<TimeoutTypeSet>(218).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(15)
        .Event<JamStarted>(82)
        .Event<JamEnded>(31) // Overrunning lineup
        .Event<JamStarted>(58)
        .Event<JamEnded>(30)
        .Event<JamStarted>(120 + 15)
        .Wait(0)
        .Validate(new GameStageState(Stage.Lineup, 1, 9, false))
        .Event<TimeoutStarted>(90) // Timeout not ended
        .Event<JamStarted>(76)
        .Event<JamEnded>(30)
        .Event<JamStarted>(112)
        .Event<JamEnded>(30)
        .Event<TimeoutStarted>(0)
        .Event<TimeoutTypeSet>(10).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Away))
        .Validate(
            ("Home", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.Timeout))
        )
        .Event<TimeoutTypeSet>(10).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<JamStarted>(121)
        .Event<JamEnded>(29) // Late jam ended
        .Event<JamStarted>(68)
        .Event<JamEnded>(30)
        .Event<JamStarted>(60)
        .Event<JamEnded>(30)
        .Event<JamStarted>(93)
        .Event<JamEnded>(30)
        .Event<JamStarted>(112)
        .Event<JamEnded>(0)
        .Validate(new GameStageState(Stage.Intermission, 1, 16, false))
        .Event<PeriodFinalized>(10 * 60)
        .Validate(new GameStageState(Stage.Intermission, 2, 0, true))
        .Validate(tick => [new IntermissionClockState(true, false, tick + 5 * 60 * 1000, 5 * 60)])
        .Wait(5 * 60)
        .Validate(tick => [new IntermissionClockState(true, true, tick, 0)])
        .Event<JamStarted>(94)
        .Validate(new GameStageState(Stage.Jam, 2, 1, false))
        .Event<JamEnded>(30)
        .Event<JamStarted>(120 + 15)
        .Validate(new GameStageState(Stage.Lineup, 2, 2, false))
        .Event<TimeoutStarted>(0) // Multiple timeouts
        .Event<TimeoutTypeSet>(160).WithBody(new TimeoutTypeSetBody(TimeoutType.Official, null))
        .Validate(
            ("Home", new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutStarted>(0)
        .Event<TimeoutTypeSet>(60).WithBody(new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home))
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.Timeout)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutEnded>(16)
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        )
        .Event<TimeoutStarted>(7)
        .Event<TimeoutTypeSet>(0).WithBody(new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home))
        .Validate(tick => [
            new GameStageState(Stage.Timeout, 2, 2, false),
            new TimeoutClockState(true, tick - 7000, 0, 7000, 7),
            new PeriodClockState(
                false,
                false,
                tick - (94 + 30 + 120 + 15 + 160 + 60 + 16 + 7) * 1000,
                0,
                (94 + 30 + 120 + 15) * 1000,
                94 + 30 + 120 + 15),
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.Review)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
        ])
        .Event<JamStarted>(109)
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None)),
            new GameStageState(Stage.Jam, 2, 3, false)
        )
        .Event<JamEnded>(30)
        .Event<JamStarted>(35)
        .Event<TimeoutStarted>(612) // Timeout started mid-jam (injury, for example)
        .Event<JamStarted>(10)
        .Validate(tick => [
            new GameStageState(Stage.Jam, 2, 5, false),
            new JamClockState(true, tick - 10000, 10000, 10),
            new PeriodClockState(
                true, 
                false,
                tick - 10000,
                (94 + 30 + 120 + 15 + 109 + 30 + 35) * 1000,
                (94 + 30 + 120 + 15 + 109 + 30 + 35 + 10) * 1000,
                94 + 30 + 120 + 15 + 109 + 30 + 35 + 10),
        ])
        .Wait(120 + 30)
        .Event<JamEnded>(0)
        .Event<JamStarted>(42)
        .Validate(
            new GameStageState(Stage.Jam, 2, 6, false)
        )
        .Event<JamEnded>(30)
        .Event<JamStarted>(59)
        .Event<JamEnded>(30)
        .Event<TimeoutStarted>(0)
        .Event<TimeoutTypeSet>(123).WithBody(new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Away))
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review))
        )
        .Event<TimeoutEnded>(0)
        .Validate(
            ("Home", new TeamTimeoutsState(0, ReviewStatus.Used, TimeoutInUse.None)),
            ("Away", new TeamTimeoutsState(3, ReviewStatus.Used, TimeoutInUse.None))
        )
        .Event<JamStarted>(119) // Jam at nearly full length
        .Event<JamEnded>(30)
        .Event<JamStarted>(52)
        .Event<JamEnded>(15)
        .Validate(tick => [
            new GameStageState(Stage.Lineup, 2, 9, false),
            new LineupClockState(true, tick - 15000, 15000, 15),
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
        .Validate(new GameStageState(Stage.AfterGame, 2, 19, false))
        .Wait(10)
        .Event<PeriodFinalized>(1)
        .Validate(new GameStageState(Stage.AfterGame, 2, 19, true))
        .Build();

    private static readonly Team HomeTeam = new(
        Guid.NewGuid(),
        new()
        {
            ["default"] = "Test Home Team"
        },
        new()
        {
            ["Black"] = new TeamColor(Color.Black, Color.White),
            ["White"] = new TeamColor(Color.White, Color.Black),
        },
        [
            new("0", "Test Skater 1"),
            new("12", "Test Skater 2"),
            new("267", "Test Skater 3"),
            new("3876", "Test Skater 4"),
            new("4", "Test Skater 5"),
            new("52", "Test Skater 6"),
            new("697", "Test Skater 7"),
            new("7293", "Test Skater 8"),
            new("8", "Test Skater 9"),
            new("90", "Test Skater 10"),
        ],
        DateTimeOffset.UtcNow);

    private static readonly Team AwayTeam = new(
        Guid.NewGuid(),
        new()
        {
            ["default"] = "Test Away Team"
        },
        new()
        {
            ["Black"] = new TeamColor(Color.Black, Color.White),
            ["White"] = new TeamColor(Color.White, Color.Black),
        },
        [
            new("0583", "Test Skater 1"),
            new("183", "Test Skater 2"),
            new("28", "Test Skater 3"),
            new("3", "Test Skater 4"),
            new("4957", "Test Skater 5"),
            new("572", "Test Skater 6"),
            new("60", "Test Skater 7"),
            new("7", "Test Skater 8"),
            new("8273", "Test Skater 9"),
            new("984", "Test Skater 10"),
        ],
        DateTimeOffset.UtcNow);
}
using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Serialization;
using jamster.engine.Services;

namespace jamster.engine.tests.Serialization;

public class LineupSheetSerializerUnitTests : UnitTest<LineupSheetSerializer>
{
    [Test]
    public void Serialize_CorrectlyRecordsSkaterNumbers()
    {
        var homeRoster = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        var awayRoster = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();

        var homeLineupSheetState = new LineupSheetState([
            new(1, 1, false, homeRoster[0], homeRoster[2], [homeRoster[4], homeRoster[5], homeRoster[6]]),          // Standard jam
            new(1, 2, true, homeRoster[1], homeRoster[3], [homeRoster[7], homeRoster[8], homeRoster[9]]),           // Jam with star pass
            new(1, 3, false, homeRoster[0], null, [homeRoster[4], homeRoster[5], homeRoster[6], homeRoster[7]]),    // No pivot
            new(1, 4, false, homeRoster[1], homeRoster[3], [homeRoster[4], homeRoster[5]]),                         // Missing blocker
        ]);

        var awayLineupSheetState = new LineupSheetState([
            new(1, 1, false, awayRoster[0], awayRoster[2], [awayRoster[4], awayRoster[5], awayRoster[6]]),
            new(1, 2, false, awayRoster[1], awayRoster[3], [awayRoster[7], awayRoster[8], awayRoster[9]]),
            new(1, 3, false, awayRoster[0], awayRoster[2], [awayRoster[4], awayRoster[5], awayRoster[6]]),
            new(1, 4, false, awayRoster[1], awayRoster[3], [awayRoster[7], awayRoster[8], awayRoster[9]]),
        ]);

        MockKeyedState(TeamSide.Home, homeLineupSheetState);
        MockKeyedState(TeamSide.Away, awayLineupSheetState);
        MockKeyedState(TeamSide.Home, new BoxTripsState([], false));
        MockKeyedState(TeamSide.Away, new BoxTripsState([], false));
        MockKeyedState(TeamSide.Home, TeamDetails(
            (homeRoster[0], "123"),
            (homeRoster[1], "234"),
            (homeRoster[2], "321"),
            (homeRoster[3], "432"),
            (homeRoster[4], "1"), 
            (homeRoster[5], "2"),
            (homeRoster[6], "3"),
            (homeRoster[7], "4"),
            (homeRoster[8], "5"),
            (homeRoster[9], "6")));

        MockKeyedState(TeamSide.Away, TeamDetails(
            (awayRoster[0], "789"),
            (awayRoster[1], "456"),
            (awayRoster[2], "987"),
            (awayRoster[3], "654"),
            (awayRoster[4], "7"),
            (awayRoster[5], "8"),
            (awayRoster[6], "9"),
            (awayRoster[7], "17"), 
            (awayRoster[8], "18"), 
            (awayRoster[9], "19")));

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(5);

        result.HomePeriod1.Lines[0].Should().Be(new LineupSheetLine("1", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""])]));
        result.HomePeriod1.Lines[1].Should().Be(new LineupSheetLine("2", false, [new("234", ["", "", ""]), new("432", ["", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[2].Should().Be(new LineupSheetLine("SP", true, [new("432", ["", "", ""]), new("234", ["", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[3].Should().Be(new LineupSheetLine("3", true, [new("123", ["", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""]), new("4", ["", "", ""])]));
        result.HomePeriod1.Lines[4].Should().Be(new LineupSheetLine("4", false, [new("234", ["", "", ""]), new("432", ["", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("?", ["", "", ""])]));

        result.AwayPeriod1.Lines.Should().HaveCount(5);

        result.AwayPeriod1.Lines[0].Should().Be(new LineupSheetLine("1", false, [new("789", ["", "", ""]), new("987", ["", "", ""]), new("7", ["", "", ""]), new("8", ["", "", ""]), new("9", ["", "", ""])]));
        result.AwayPeriod1.Lines[1].Should().Be(new LineupSheetLine("2", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("17", ["", "", ""]), new("18", ["", "", ""]), new("19", ["", "", ""])]));
        result.AwayPeriod1.Lines[2].Should().Be(new LineupSheetLine("SP*", false, [new("", ["", "", ""]), new("", ["", "", ""]), new("", ["", "", ""]), new("", ["", "", ""]), new("", ["", "", ""])]));
        result.AwayPeriod1.Lines[3].Should().Be(new LineupSheetLine("3", false, [new("789", ["", "", ""]), new("987", ["", "", ""]), new("7", ["", "", ""]), new("8", ["", "", ""]), new("9", ["", "", ""])]));
        result.AwayPeriod1.Lines[4].Should().Be(new LineupSheetLine("4", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("17", ["", "", ""]), new("18", ["", "", ""]), new("19", ["", "", ""])]));
    }

    [Test]
    public void Serialize_CorrectlyRecordsBoxTrips()
    {
        var homeRoster = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();

        var homeLineupSheetState = new LineupSheetState([
            new(1, 1, false, homeRoster[0], homeRoster[2], [homeRoster[4], homeRoster[5], homeRoster[6]]),
            new(1, 2, false, homeRoster[1], homeRoster[3], [homeRoster[7], homeRoster[8], homeRoster[6]]),
            new(1, 3, false, homeRoster[0], homeRoster[2], [homeRoster[4], homeRoster[5], homeRoster[6]]),
            new(1, 4, false, homeRoster[1], homeRoster[3], [homeRoster[4], homeRoster[5], homeRoster[8]]),
            new(1, 5, false, homeRoster[0], homeRoster[2], [homeRoster[4], homeRoster[5], homeRoster[6]]),
            new(1, 6, true, homeRoster[1], homeRoster[3], [homeRoster[7], homeRoster[8], homeRoster[9]]),
            new(1, 7, false, homeRoster[0], homeRoster[2], [homeRoster[4], homeRoster[8], homeRoster[9]]),
            new(1, 8, true, homeRoster[1], homeRoster[3], [homeRoster[7], homeRoster[8], homeRoster[9]]),
            new(1, 9, false, homeRoster[0], homeRoster[2], [homeRoster[7], homeRoster[5], homeRoster[6]]),
            new(1, 10, true, homeRoster[1], homeRoster[3], [homeRoster[7], homeRoster[8], homeRoster[9]]),
            new(1, 11, false, homeRoster[0], homeRoster[2], [homeRoster[7], homeRoster[5], homeRoster[6]]),
        ]);

        var homeBoxTrips = new BoxTrip[]
        {
            new(1, 1, 1, false, false, homeRoster[2], SkaterPosition.Pivot, 0, false, [], 0, 0, 0),      // In and out same jam, no star pass
            new(1, 2, 2, false, true, homeRoster[6], SkaterPosition.Blocker, 0, false, [], 0, 0, 0),  // In between jams, out same jam, no star pass
            new(1, 3, 3, false, false, homeRoster[4], SkaterPosition.Blocker, 1, false, [], 0, 0, 0), // In one jam, out next, no star pass
            new(1, 3, 3, false, false, homeRoster[5], SkaterPosition.Blocker, 2, false, [], 0, 0, 0), // In one jam, out two after, no star pass
            new(1, 6, 6, false, false, homeRoster[3], SkaterPosition.Pivot, 0, false, [], 0, 0, 0),      // In and out before star pass
            new(1, 6, 6, true, false, homeRoster[1], SkaterPosition.Blocker, 0, true, [], 0, 0, 0),     // In and out after star pass
            new(1, 6, 6, false, false, homeRoster[7], SkaterPosition.Blocker, 0, true, [], 0, 0, 0),  // In before star pass and out after
            new(1, 6, 6, false, false, homeRoster[8], SkaterPosition.Pivot, 1, false, [], 0, 0, 0),   // In before star pass, out next jam
            new(1, 6, 6, true, false, homeRoster[9], SkaterPosition.Jammer, 1, false, [], 0, 0, 0),   // In after star pass, out next jam
            new(1, 7, 7, false, false, homeRoster[8], SkaterPosition.Blocker, 1, false, [], 0, 0, 0), // In one jam, out next, before star pass
            new(1, 7, 7, false, false, homeRoster[9], SkaterPosition.Blocker, 1, true, [], 0, 0, 0),  // In one jam, out next, after star pass
            new(1, 9, 9, false, true, homeRoster[7], SkaterPosition.Blocker, 2, false, [], 0, 0, 0),  // Multiple jams spanning star pass
        };

        MockKeyedState(TeamSide.Home, homeLineupSheetState);
        MockKeyedState(TeamSide.Away, new LineupSheetState(Enumerable.Range(1, homeLineupSheetState.Jams.Length).Select(i => new LineupSheetJam(1, i, false, null, null, [])).ToArray()));
        MockKeyedState(TeamSide.Home, new BoxTripsState(homeBoxTrips, false));
        MockKeyedState(TeamSide.Away, new BoxTripsState([], false));
        MockKeyedState(TeamSide.Home, TeamDetails(
            (homeRoster[0], "123"),
            (homeRoster[1], "456"),
            (homeRoster[2], "321"),
            (homeRoster[3], "654"),
            (homeRoster[4], "1"),
            (homeRoster[5], "2"),
            (homeRoster[6], "3"),
            (homeRoster[7], "4"),
            (homeRoster[8], "5"),
            (homeRoster[9], "6")));
        MockKeyedState(TeamSide.Away, TeamDetails());

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(14);

        result.HomePeriod1.Lines[0].Should().Be(new LineupSheetLine("1", false, [new("123", ["", "", ""]), new("321", ["+", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""])]));
        result.HomePeriod1.Lines[1].Should().Be(new LineupSheetLine("2", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("3", ["$", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""])]));
        result.HomePeriod1.Lines[2].Should().Be(new LineupSheetLine("3", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("1", ["-", "", ""]), new("2", ["-", "", ""]), new("3", ["", "", ""])]));
        result.HomePeriod1.Lines[3].Should().Be(new LineupSheetLine("4", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("1", ["$", "", ""]), new("2", ["S", "", ""]), new("5", ["", "", ""])]));
        result.HomePeriod1.Lines[4].Should().Be(new LineupSheetLine("5", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("1", ["", "", ""]), new("2", ["$", "", ""]), new("3", ["", "", ""])]));
        result.HomePeriod1.Lines[5].Should().Be(new LineupSheetLine("6", false, [new("456", ["", "", ""]), new("654", ["+", "", ""]), new("4", ["-", "", ""]), new("5", ["-", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[6].Should().Be(new LineupSheetLine("SP", true, [new("654", ["", "", ""]), new("456", ["+", "", ""]), new("4", ["$", "", ""]), new("5", ["S", "", ""]), new("6", ["-", "", ""])]));
        result.HomePeriod1.Lines[7].Should().Be(new LineupSheetLine("7", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("1", ["", "", ""]), new("5", ["$", "-", ""]), new("6", ["$", "-", ""])]));
        result.HomePeriod1.Lines[8].Should().Be(new LineupSheetLine("8", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("4", ["", "", ""]), new("5", ["$", "", ""]), new("6", ["S", "", ""])]));
        result.HomePeriod1.Lines[9].Should().Be(new LineupSheetLine("SP", true, [new("654", ["", "", ""]), new("456", ["", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""]), new("6", ["$", "", ""])]));
        result.HomePeriod1.Lines[10].Should().Be(new LineupSheetLine("9", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""]), new("4", ["S", "", ""])]));
        result.HomePeriod1.Lines[11].Should().Be(new LineupSheetLine("10", false, [new("456", ["", "", ""]), new("654", ["", "", ""]), new("4", ["S", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[12].Should().Be(new LineupSheetLine("SP", true, [new("654", ["", "", ""]), new("456", ["", "", ""]), new("4", ["S", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[13].Should().Be(new LineupSheetLine("11", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""]), new("4", ["$", "", ""])]));
    }

    private static TeamDetailsState TeamDetails(params (Guid Id, string Number)[] skaters) =>
        new(new GameTeam(
            [],
            new TeamColor(Color.Black, Color.White),
            skaters.Select(s => new GameSkater(s.Id, s.Number, "", true)).ToList()
        ));

    private void MockKeyedState<TKey, TState>(TKey key, TState state) where TKey : notnull where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetKeyedState<TState>(key.ToString() ?? "")).Returns(state);
}

using amethyst.Domain;
using amethyst.Reducers;
using amethyst.Serialization;
using amethyst.Services;
using FluentAssertions;

namespace amethyst.tests.Serialization;

public class LineupSheetSerializerUnitTests : UnitTest<LineupSheetSerializer>
{
    [Test]
    public void Serialize_CorrectlyRecordsSkaterNumbers()
    {
        var homeLineupSheetState = new LineupSheetState([
            new(1, 1, false, "123", "321", ["1", "2", "3"]), // Standard jam
            new(1, 2, true, "234", "432", ["4", "5", "6"]), // Jam with star pass
            new(1, 3, false, "123", null, ["1", "2", "3", "4"]), // No pivot
            new(1, 4, false, "234", "432", ["1", "2"]), // Missing blocker
        ]);

        var awayLineupSheetState = new LineupSheetState([
            new(1, 1, false, "789", "987", ["7", "8", "9"]),
            new(1, 2, false, "456", "654", ["17", "18", "19"]),
            new(1, 3, false, "789", "987", ["7", "8", "9"]),
            new(1, 4, false, "456", "654", ["17", "18", "19"]),
        ]);

        MockKeyedState(TeamSide.Home, homeLineupSheetState);
        MockKeyedState(TeamSide.Away, awayLineupSheetState);
        MockKeyedState(TeamSide.Home, new BoxTripsState([], false));
        MockKeyedState(TeamSide.Away, new BoxTripsState([], false));

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(5);

        result.HomePeriod1.Lines[0].Should().Be(new LineupSheetLine("1", false, [new("123", ["", "", ""]), new("321", ["", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""])]));
        result.HomePeriod1.Lines[1].Should().Be(new LineupSheetLine("2", false, [new("234", ["", "", ""]), new("432", ["", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[2].Should().Be(new LineupSheetLine("SP", true, [new("432", ["", "", ""]), new("234", ["", "", ""]), new("4", ["", "", ""]), new("5", ["", "", ""]), new("6", ["", "", ""])]));
        result.HomePeriod1.Lines[3].Should().Be(new LineupSheetLine("3", true, [new("123", ["", "", ""]), new("4", ["", "", ""]), new("1", ["", "", ""]), new("2", ["", "", ""]), new("3", ["", "", ""])]));
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
        var homeLineupSheetState = new LineupSheetState([
            new(1, 1, false, "123", "321", ["1", "2", "3"]),
            new(1, 2, false, "456", "654", ["4", "5", "3"]),
            new(1, 3, false, "123", "321", ["1", "2", "3"]),
            new(1, 4, false, "456", "654", ["1", "2", "5"]),
            new(1, 5, false, "123", "321", ["1", "2", "3"]),
            new(1, 6, true, "456", "654", ["4", "5", "6"]),
            new(1, 7, false, "123", "321", ["1", "5", "6"]),
            new(1, 8, true, "456", "654", ["4", "5", "6"]),
            new(1, 9, false, "123", "321", ["4", "2", "3"]),
            new(1, 10, true, "456", "654", ["4", "5", "6"]),
            new(1, 11, false, "123", "321", ["4", "2", "3"]),
        ]);

        var homeBoxTrips = new BoxTrip[]
        {
            new(1, 1, 1, false, false, "321", SkaterPosition.Pivot, 0, false, [], 0, 0, 0), // In and out same jam, no star pass
            new(1, 2, 2, false, true, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 0), // In between jams, out same jam, no star pass
            new(1, 3, 3, false, false, "1", SkaterPosition.Blocker, 1, false, [], 0, 0, 0), // In one jam, out next, no star pass
            new(1, 3, 3, false, false, "2", SkaterPosition.Blocker, 2, false, [], 0, 0, 0), // In one jam, out two after, no star pass
            new(1, 6, 6, false, false, "654", SkaterPosition.Pivot, 0, false, [], 0, 0, 0), // In and out before star pass
            new(1, 6, 6, true, false, "456", SkaterPosition.Blocker, 0, true, [], 0, 0, 0), // In and out after star pass
            new(1, 6, 6, false, false, "4", SkaterPosition.Blocker, 0, true, [], 0, 0, 0), // In before star pass and out after
            new(1, 6, 6, false, false, "5", SkaterPosition.Pivot, 1, false, [], 0, 0, 0), // In before star pass, out next jam
            new(1, 6, 6, true, false, "6", SkaterPosition.Jammer, 1, false, [], 0, 0, 0), // In after star pass, out next jam
            new(1, 7, 7, false, false, "5", SkaterPosition.Blocker, 1, false, [], 0, 0, 0), // In one jam, out next, before star pass
            new(1, 7, 7, false, false, "6", SkaterPosition.Blocker, 1, true, [], 0, 0, 0), // In one jam, out next, after star pass
            new(1, 9, 9, false, true, "4", SkaterPosition.Blocker, 2, false, [], 0, 0, 0), // Multiple jams spanning star pass
        };

        MockKeyedState(TeamSide.Home, homeLineupSheetState);
        MockKeyedState(TeamSide.Away, new LineupSheetState(Enumerable.Range(1, homeLineupSheetState.Jams.Length).Select(i => new LineupSheetJam(1, i, false, null, null, [])).ToArray()));
        MockKeyedState(TeamSide.Home, new BoxTripsState(homeBoxTrips, false));
        MockKeyedState(TeamSide.Away, new BoxTripsState([], false));

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

    private void MockKeyedState<TKey, TState>(TKey key, TState state) where TKey : notnull where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetKeyedState<TState>(key.ToString() ?? "")).Returns(state);
}
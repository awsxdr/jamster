using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class LineupSheetUnitTests : ReducerUnitTest<HomeLineupSheet, LineupSheetState>
{
    [Test]
    public async Task JamEnded_CreatesNewJamWithExpectedDefaults()
    {
        State = new([]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new JamEnded(0));

        State.Jams.Should().ContainSingle()
            .Which.Should().Be(new LineupSheetJam(2, 7, null, null, [null, null, null]));
    }

    private static readonly IEnumerable<TestCaseData> SkaterAddedToJamTestCases =
    [
        new(SkaterPosition.Jammer, "123", "123", "2", (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Jammer, "1", "1", "2", (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Jammer, "2", "2", null, (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Jammer, "3", "3", "2", (string?[]) ["4", "5", null]),
        new(SkaterPosition.Jammer, "4", "4", "2", (string?[]) ["3", "5", null]),
        new(SkaterPosition.Pivot, "123", "1", "123", (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Pivot, "1", null, "1", (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Pivot, "2", "1", "2", (string?[]) ["3", "4", "5"]),
        new(SkaterPosition.Pivot, "3", "1", "3", (string?[]) ["4", "5", null]),
        new(SkaterPosition.Pivot, "4", "1", "4", (string?[]) ["3", "5", null]),
        new(SkaterPosition.Blocker, "123", "1", "2", (string?[]) ["4", "5", "123"]),
        new(SkaterPosition.Blocker, "1", null, "2", (string?[]) ["4", "5", "1"]),
        new(SkaterPosition.Blocker, "2", "1", null, (string?[]) ["3", "4", "5", "2"]),
        new(SkaterPosition.Blocker, "3", "1", "2", (string?[]) ["4", "5", "3"]),
        new(SkaterPosition.Blocker, "4", "1", "2", (string?[]) ["3", "5", "4"]),
    ];

    [TestCaseSource(nameof(SkaterAddedToJamTestCases))]
    public async Task SkaterAddedToJam_WhenJamExists_SetsSkaterCorrectly(SkaterPosition position, string skaterNumber, string expectedJammerNumber, string expectedPivotNumber, string[] expectedBlockers)
    {
        State = new([
            new(1, 1, "11", "12", ["13", "14", "15"]),
            new(1, 2, "1", "2", ["3", "4", "5"]),
            new(1, 3, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = new(1, 2, expectedJammerNumber, expectedPivotNumber, expectedBlockers);
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, skaterNumber, position)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_When4BlockersListed_RemovesFirstBlocker()
    {
        State = new([
            new(1, 1, "11", "12", ["13", "14", "15"]),
            new(1, 2, "1", null, ["2", "3", "4", "5"]),
            new(1, 3, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            PivotNumber = "6",
            BlockerNumbers = ["3", "4", "5"],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, "6", SkaterPosition.Pivot)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_When3BlockersListed_AndNoPivot_Adds4thBlocker()
    {
        State = new([
            new(1, 1, "11", "12", ["13", "14", "15"]),
            new(1, 2, "1", null, ["2", "3", "4"]),
            new(1, 3, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            BlockerNumbers = ["2", "3", "4", "5"],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, "5", SkaterPosition.Blocker)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenReplacesPivot_And3BlockersListed_Adds4thBlocker()
    {
        State = new([
            new(1, 1, "11", "12", ["13", "14", "15"]),
            new(1, 2, "1", "5", ["2", "3", "4"]),
            new(1, 3, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            PivotNumber = null,
            BlockerNumbers = ["2", "3", "4", "5"],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, "5", SkaterPosition.Blocker)));

        State.Should().Be(expectedResult);
    }

    private static readonly IEnumerable<TestCaseData> SkaterRemovedFromJamTestCases =
    [
        new("1", null, "2", (string?[])["3", "4", "5"]),
        new("2", "1", null, (string?[])["3", "4", "5"]),
        new("3", "1", "2", (string?[])["4", "5", null]),
        new("4", "1", "2", (string?[])["3", "5", null]),
        new("5", "1", "2", (string?[])["3", "4", null]),
        new("6", "1", "2", (string?[])["3", "4", "5"])
    ];

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamExists_RemovesSkater(string skaterNumber, string? expectedJammerNumber, string? expectedPivotNumber, string?[] expectedBlockers)
    {
        State = new([
            new(1, 1, "11", "12", ["13", "14", "15"]),
            new(1, 2, "1", "2", ["3", "4", "5"]),
            new(1, 3, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = new(1, 2, expectedJammerNumber, expectedPivotNumber, expectedBlockers);
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 1, 2, skaterNumber)));

        State.Should().Be(expectedResult);
    }
}
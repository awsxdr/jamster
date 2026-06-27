using AwesomeAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class LineupSheetUnitTests : ReducerUnitTest<HomeLineupSheet, LineupSheetState>
{
    private static Guid SkaterId(int n) => new(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    [Test]
    public async Task JamEnded_CreatesNewJamWithExpectedDefaults()
    {
        State = new([]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new JamEnded(0));

        State.Jams.Should().ContainSingle()
            .Which.Should().Be(new LineupSheetJam(2, 7, false, null, null, [null, null, null]));
    }

    private static readonly IEnumerable<TestCaseData> SkaterAddedToJamTestCases =
    [
        new(SkaterPosition.Jammer, SkaterId(123), SkaterId(123), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Jammer, SkaterId(1), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Jammer, SkaterId(2), SkaterId(2), null, (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Jammer, SkaterId(3), SkaterId(3), SkaterId(2), (Guid?[])[SkaterId(4), SkaterId(5), null]),
        new(SkaterPosition.Jammer, SkaterId(4), SkaterId(4), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(5), null]),
        new(SkaterPosition.Pivot, SkaterId(123), SkaterId(1), SkaterId(123), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Pivot, SkaterId(1), null, SkaterId(1), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Pivot, SkaterId(2), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterPosition.Pivot, SkaterId(3), SkaterId(1), SkaterId(3), (Guid?[])[SkaterId(4), SkaterId(5), null]),
        new(SkaterPosition.Pivot, SkaterId(4), SkaterId(1), SkaterId(4), (Guid?[])[SkaterId(3), SkaterId(5), null]),
        new(SkaterPosition.Blocker, SkaterId(123), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(4), SkaterId(5), SkaterId(123)]),
        new(SkaterPosition.Blocker, SkaterId(1), null, SkaterId(2), (Guid?[])[SkaterId(4), SkaterId(5), SkaterId(1)]),
        new(SkaterPosition.Blocker, SkaterId(2), SkaterId(1), null, (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5), SkaterId(2)]),
        new(SkaterPosition.Blocker, SkaterId(3), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(4), SkaterId(5), SkaterId(3)]),
        new(SkaterPosition.Blocker, SkaterId(4), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(5), SkaterId(4)]),
    ];

    [TestCaseSource(nameof(SkaterAddedToJamTestCases))]
    public async Task SkaterAddedToJam_WhenJamExists_SetsSkaterCorrectly(SkaterPosition position, Guid skaterId, Guid? expectedJammerId, Guid? expectedPivotId, Guid?[] expectedBlockerIds)
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), SkaterId(2), [SkaterId(3), SkaterId(4), SkaterId(5)]),
            new(1, 3, false, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = new(1, 2, false, expectedJammerId, expectedPivotId, expectedBlockerIds);
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, skaterId, position)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_When4BlockersListed_RemovesFirstBlocker()
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), null, [SkaterId(2), SkaterId(3), SkaterId(4), SkaterId(5)]),
            new(1, 3, false, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            PivotId = SkaterId(6),
            BlockerIds = [SkaterId(3), SkaterId(4), SkaterId(5)],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, SkaterId(6), SkaterPosition.Pivot)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_When3BlockersListed_AndNoPivot_Adds4thBlocker()
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), null, [SkaterId(2), SkaterId(3), SkaterId(4)]),
            new(1, 3, false, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            BlockerIds = [SkaterId(2), SkaterId(3), SkaterId(4), SkaterId(5)],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, SkaterId(5), SkaterPosition.Blocker)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenReplacesPivot_And3BlockersListed_Adds4thBlocker()
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), SkaterId(5), [SkaterId(2), SkaterId(3), SkaterId(4)]),
            new(1, 3, false, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = expectedJams[1] with
        {
            PivotId = null,
            BlockerIds = [SkaterId(2), SkaterId(3), SkaterId(4), SkaterId(5)],
        };
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 1, 2, SkaterId(5), SkaterPosition.Blocker)));

        State.Should().Be(expectedResult);
    }

    private static readonly IEnumerable<TestCaseData> SkaterRemovedFromJamTestCases =
    [
        new(SkaterId(1), null, SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterId(2), SkaterId(1), null, (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
        new(SkaterId(3), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(4), SkaterId(5), null]),
        new(SkaterId(4), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(5), null]),
        new(SkaterId(5), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), null]),
        new(SkaterId(6), SkaterId(1), SkaterId(2), (Guid?[])[SkaterId(3), SkaterId(4), SkaterId(5)]),
    ];

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamExists_RemovesSkater(Guid skaterId, Guid? expectedJammerId, Guid? expectedPivotId, Guid?[] expectedBlockerIds)
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), SkaterId(2), [SkaterId(3), SkaterId(4), SkaterId(5)]),
            new(1, 3, false, null, null, [null, null, null])
        ]);

        var expectedJams = (LineupSheetJam[])State.Jams.Clone();
        expectedJams[1] = new(1, 2, false, expectedJammerId, expectedPivotId, expectedBlockerIds);
        var expectedResult = new LineupSheetState(expectedJams);

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 1, 2, skaterId)));

        State.Should().Be(expectedResult);
    }

    [Test]
    public async Task PeriodFinalized_MovesLastJamOnSheetToStartOfNextPeriod()
    {
        State = new([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(1, 2, false, SkaterId(1), SkaterId(2), [SkaterId(3), SkaterId(4), SkaterId(5)]),
        ]);

        await Subject.Handle(new PeriodFinalized(0));

        State.Should().Be(new LineupSheetState([
            new(1, 1, false, SkaterId(11), SkaterId(12), [SkaterId(13), SkaterId(14), SkaterId(15)]),
            new(2, 1, false, SkaterId(1), SkaterId(2), [SkaterId(3), SkaterId(4), SkaterId(5)]),
        ]));
    }

    [Test]
    public async Task StarPassMarked_WhenTeamMatches_SetsStarPassAccordingly([Values] bool starPass)
    {
        State = new([
            new(1, 1, !starPass, SkaterId(123), SkaterId(321), [SkaterId(1), SkaterId(2), SkaterId(3)])
        ]);

        await Subject.Handle(new StarPassMarked(0, new(TeamSide.Home, starPass)));

        State.Jams[0].HasStarPass.Should().Be(starPass);
    }

    [Test]
    public async Task StarPassMarked_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(1, 1, false, SkaterId(123), SkaterId(321), [SkaterId(1), SkaterId(2), SkaterId(3)])
        ]);

        await Subject.Handle(new StarPassMarked(0, new(TeamSide.Away, true)));

        State.Jams[0].HasStarPass.Should().BeFalse();
    }
}

using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using FluentAssertions;

namespace jamster.engine.tests.Reducers;

public class JamLineupUnitTests : ReducerUnitTest<HomeTeamJamLineup, JamLineupState>
{
    [Test]
    public async Task SkaterOnTrack_WithJammer_ReturnsSkaterAddedToJamEvent()
    {
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, "123", SkaterPosition.Jammer)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, "123", SkaterPosition.Jammer));
    }

    private static readonly IEnumerable<TestCaseData> JammerLineupTestCases =
    [
        new(new JamLineupState(null, null, [null, null, null]), "123", new JamLineupState("123", null, [null, null, null])),
        new(new JamLineupState("321", null, [null, null, null]), "123", new JamLineupState("123", null, [null, null, null])),
        new(new JamLineupState(null, "123", [null, null, null]), "123", new JamLineupState("123", null, [null, null, null])),
        new(new JamLineupState(null, null, ["321", "123", "444"]), "123", new JamLineupState("123", null, ["321", "444", null])),
        new(new JamLineupState(null, "321", [null, null, null]), "123", new JamLineupState("123", "321", [null, null, null])),
    ];

    [TestCaseSource(nameof(JammerLineupTestCases))]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsCurrent_AndInJam_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterNumber, SkaterPosition.Jammer)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(JammerLineupTestCases))]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterNumber, SkaterPosition.Jammer)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, "555", SkaterPosition.Jammer)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, "555", SkaterPosition.Jammer)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOnTrack_WithPivot_ReturnsSkaterAddedToJamEvent()
    {
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, "123", SkaterPosition.Pivot)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, "123", SkaterPosition.Pivot));
    }


    private static readonly IEnumerable<TestCaseData> PivotLineupTestCases =
    [
        new TestCaseData(new JamLineupState(null, null, [null, null, null]), "123", new JamLineupState(null, "123", [null, null, null])).SetName("Clean lineup"),
        new TestCaseData(new JamLineupState(null, "321", [null, null, null]), "123", new JamLineupState(null, "123", [null, null, null])).SetName("Pivot already set"),
        new TestCaseData(new JamLineupState("123", null, [null, null, null]), "123", new JamLineupState(null, "123", [null, null, null])).SetName("Replaces jammer"),
        new TestCaseData(new JamLineupState(null, null, ["321", "123", "444"]), "123", new JamLineupState(null, "123", ["321", "444", null])).SetName("Replaces blocker"),
        new TestCaseData(new JamLineupState("321", null, [null, null, null]), "123", new JamLineupState("321", "123", [null, null, null])).SetName("Adds to existing"),
        new TestCaseData(new JamLineupState(null, null, ["1", "2", "3", "4"]), "5", new JamLineupState(null, "5", ["2", "3", "4"])).SetName("Clears 4th blocker"),
        new TestCaseData(new JamLineupState(null, null, ["1", "2", "3", "4"]), "3", new JamLineupState(null, "3", ["1", "2", "4"])).SetName("Replaces 4th blocker"),
    ];

    [TestCaseSource(nameof(PivotLineupTestCases))]
    public async Task SkaterOnTrack_SkaterAddedToJam_WithPivot_WhenJamIsCurrent_SetsLineupCorrectlyWithPivot_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterNumber, SkaterPosition.Pivot)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(PivotLineupTestCases))]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterNumber, SkaterPosition.Pivot)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, "555", SkaterPosition.Pivot)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, "555", SkaterPosition.Pivot)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOnTrack_WithBlocker_ReturnsSkaterAddedToJamEvent()
    {
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, "123", SkaterPosition.Blocker)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, "123", SkaterPosition.Blocker));
    }

    private static readonly IEnumerable<TestCaseData> BlockerLineupTestCases =
    [
        new(new JamLineupState(null, null, [null, null, null]), "123", new JamLineupState(null, null, ["123", null, null])),
        new(new JamLineupState(null, null, ["321", null, null]), "123", new JamLineupState(null, null, ["321", "123", null])),
        new(new JamLineupState(null, null, ["1", "2", "3"]), "123", new JamLineupState(null, null, ["1", "2", "3", "123"])),
        new(new JamLineupState(null, "4", ["1", "2", "3"]), "123", new JamLineupState(null, "4", ["2", "3", "123"])),
        new(new JamLineupState(null, null, ["1", "123", "3"]), "123", new JamLineupState(null, null, ["1", "3", "123"])),
        new(new JamLineupState("123", null, [null, null, null]), "123", new JamLineupState(null, null, ["123", null, null])),
        new(new JamLineupState(null, "123", [null, null, null]), "123", new JamLineupState(null, null, ["123", null, null])),
        new(new JamLineupState(null, "123", ["1", "2", "3"]), "123", new JamLineupState(null, null, ["1", "2", "3", "123"])),
    ];

    [TestCaseSource(nameof(BlockerLineupTestCases))]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsCurrent_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterNumber, SkaterPosition.Blocker)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(BlockerLineupTestCases))]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, string newSkaterNumber, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterNumber, SkaterPosition.Blocker)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, "555", SkaterPosition.Blocker)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new("123", "321", ["1", "2", "3"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, "555", SkaterPosition.Blocker)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOffTrack_WhenInJam_AndTeamMatches_RaisesSkaterRemovedFromJamEvent()
    {
        State = new("1", "2", ["3", "4", "5"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, "4")));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 6, "4"));
    }

    [Test]
    public async Task SkaterOffTrack_WhenNotInJam_AndTeamMatches_RaisesSkaterRemovedFromJamEvent()
    {
        State = new("1", "2", ["3", "4", "5"]);
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, "4")));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 7, "4"));
    }

    [Test]
    public async Task SkaterOffTrack_WhenInJam_AndTeamDoesNotMatch_DoesNotRaiseSkaterRemovedFromJamEvent()
    {
        State = new("1", "2", ["3", "4", "5"]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Away, "4")));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().BeEmpty();
    }

    private static readonly IEnumerable<TestCaseData> SkaterRemovedFromJamTestCases =
    [
        new(new JamLineupState("123", "1", ["2", "3", "4"]), new JamLineupState(null, "1", ["2", "3", "4"])),
        new(new JamLineupState("1", "123", ["2", "3", "4"]), new JamLineupState("1", null, ["2", "3", "4"])),
        new(new JamLineupState("1", "2", ["123", "3", "4"]), new JamLineupState("1", "2", ["3", "4", null])),
        new(new JamLineupState("1", "2", ["3", "123", "4"]), new JamLineupState("1", "2", ["3", "4", null])),
        new(new JamLineupState("1", "2", ["3", "4", "123"]), new JamLineupState("1", "2", ["3", "4", null])),
        new(new JamLineupState("1", "2", ["3", "4", "5"]), new JamLineupState("1", "2", ["3", "4", "5"])),
    ];

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamIsCurrent_AndInJam_ClearsSkaterFromLineup(JamLineupState initialState, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 2, 6, "123")));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamIsUpcoming_AndNotInJam_ClearsSkaterFromLineup(JamLineupState initialState, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 2, 7, "123")));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterOffTrack_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new ("123", "1", ["2", "3", "4"]);

        await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterOffTrack_RaisesSkaterRemovedFromJamEvent()
    {
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, "666")));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 6, "666"));
    }

    [Test]
    public async Task JamEnded_ClearsLineup()
    {
        State = new("123", "321", [null, null, null]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new([], []));

        _ = await Subject.Handle(new JamEnded(0));

        State.Should().Be(new JamLineupState(null, null, [null, null, null]));
    }

    [Test]
    public async Task JamEnded_RaisesSkaterOnTrackEvent_ForEachSkaterInBoxOrQueuedForBox()
    {
        State = new("1", "2", ["3", "4", "5"]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new(["1", "2"], ["3"]));
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Away), new(["3", "4"], []));

        var implicitEvents = await Subject.Handle(new JamEnded(1234));

        implicitEvents.OfType<SkaterOnTrack>().Should().HaveCount(3)
            .And.AllSatisfy(e => e.Tick.Should().Be(1234))
            .And.Subject.Select(x => x.Body).Should().BeEquivalentTo((SkaterOnTrackBody[])
            [
                new(TeamSide.Home, "1", SkaterPosition.Jammer),
                new(TeamSide.Home, "2", SkaterPosition.Pivot),
                new(TeamSide.Home, "3", SkaterPosition.Blocker),
            ]);
    }

    [Test]
    public async Task SkaterSubstitutedInBox_RaisesEventsToReplaceSkaterInLineup()
    {
        State = new("1", "2", ["3", "4", "5"]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new(["2"], []));
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Away), new([], []));

        var implicitEvents = await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, "2", "6")));
        implicitEvents = implicitEvents.ToArray();

        implicitEvents.Where(e => e is SkaterOffTrack or SkaterOnTrack).Should().HaveCount(2);
        implicitEvents.Where(e => e is SkaterOffTrack { Body: { SkaterNumber: "2", TeamSide: TeamSide.Home } }).Should().ContainSingle();
        implicitEvents.Where(e => e is SkaterOnTrack { Body: { SkaterNumber: "6", TeamSide: TeamSide.Home } }).Should().ContainSingle();
    }


    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadMarked_RaisesLostMarkedEvent()
    {
        State = new("123", "1", ["2", "3", "4"]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, true, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "123", "X")));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadMarkedForOpponent_DoesNotRaiseLostMarkedEvent()
    {
        State = new("123", "1", ["2", "3", "4"]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(true, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "123", "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadNotMarkedForEitherTeam_RaisesLostMarkedEvent()
    {
        State = new("123", "1", ["2", "3", "4"]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "123", "X")));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task PenaltyAssessed_WhenNotAgainstJammer_DoesNotRaiseLostMarkedEvent()
    {
        State = new("123", "1", ["2", "3", "4"]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "1", "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenTeamDoesNotMatch_DoesNotRaiseLostMarkedEvent()
    {
        State = new("123", "1", ["2", "3", "4"]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Away, "123", "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNotInLineup_AndSkaterInPreviousLineup_AddsSkaterToLineup()
    {
        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "123", "X")));

        implicitEvents.OfType<PreviousJamSkaterOnTrack>().Should().ContainSingle()
            .Which.Body.SkaterNumber.Should().Be("123");
    }
}
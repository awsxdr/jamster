using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class JamLineupUnitTests : ReducerUnitTest<HomeTeamJamLineup, JamLineupState>
{
    [Test]
    public async Task SkaterOnTrack_WithJammer_ReturnsSkaterAddedToJamEvent()
    {
        var skaterId = Guid.NewGuid();

        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, skaterId, SkaterPosition.Jammer)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, skaterId, SkaterPosition.Jammer));
    }

    private static IEnumerable<TestCaseData> JammerLineupTestCases()
    {
        var skaterIds = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToArray();

        return [
            new(new JamLineupState(null, null, [null, null, null]), skaterIds[0], new JamLineupState(skaterIds[0], null, [null, null, null])),
            new(new JamLineupState(skaterIds[0], null, [null, null, null]), skaterIds[0], new JamLineupState(skaterIds[0], null, [null, null, null])),
            new(new JamLineupState(null, skaterIds[0], [null, null, null]), skaterIds[0], new JamLineupState(skaterIds[0], null, [null, null, null])),
            new(new JamLineupState(null, null, [skaterIds[1], skaterIds[0], skaterIds[2]]), skaterIds[0], new JamLineupState(skaterIds[0], null, [skaterIds[1], skaterIds[2], null])),
            new(new JamLineupState(null, skaterIds[1], [null, null, null]), skaterIds[0], new JamLineupState(skaterIds[0], skaterIds[1], [null, null, null])),
        ];
    }

    [TestCaseSource(nameof(JammerLineupTestCases))]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsCurrent_AndInJam_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterId, SkaterPosition.Jammer)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(JammerLineupTestCases))]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterId, SkaterPosition.Jammer)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, Guid.NewGuid(), SkaterPosition.Jammer)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithJammer_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, Guid.NewGuid(), SkaterPosition.Jammer)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOnTrack_WithPivot_ReturnsSkaterAddedToJamEvent()
    {
        var skaterId = Guid.NewGuid();

        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, skaterId, SkaterPosition.Pivot)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, skaterId, SkaterPosition.Pivot));
    }


    private static IEnumerable<TestCaseData> PivotLineupTestCases()
    {
        var skaterIds = Enumerable.Range(0, 8).Select(_ => Guid.NewGuid()).ToArray();

        return [
            new TestCaseData(new JamLineupState(null, null, [null, null, null]), skaterIds[0], new JamLineupState(null, skaterIds[0], [null, null, null])).SetName("Clean lineup"),
            new TestCaseData(new JamLineupState(null, skaterIds[1], [null, null, null]), skaterIds[0], new JamLineupState(null, skaterIds[0], [null, null, null])).SetName("Pivot already set"),
            new TestCaseData(new JamLineupState(skaterIds[0], null, [null, null, null]), skaterIds[0], new JamLineupState(null, skaterIds[0], [null, null, null])).SetName("Replaces jammer"),
            new TestCaseData(new JamLineupState(null, null, [skaterIds[1], skaterIds[0], skaterIds[2]]), skaterIds[0], new JamLineupState(null, skaterIds[0], [skaterIds[1], skaterIds[2], null])).SetName("Replaces blocker"),
            new TestCaseData(new JamLineupState(skaterIds[1], null, [null, null, null]), skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[0], [null, null, null])).SetName("Adds to existing"),
            new TestCaseData(new JamLineupState(null, null, [skaterIds[3], skaterIds[4], skaterIds[5], skaterIds[6]]), skaterIds[7], new JamLineupState(null, skaterIds[7], [skaterIds[4], skaterIds[5], skaterIds[6]])).SetName("Clears 4th blocker"),
            new TestCaseData(new JamLineupState(null, null, [skaterIds[3], skaterIds[4], skaterIds[5], skaterIds[6]]), skaterIds[5], new JamLineupState(null, skaterIds[5], [skaterIds[3], skaterIds[4], skaterIds[6]])).SetName("Replaces 4th blocker"),
        ];
    }

    [TestCaseSource(nameof(PivotLineupTestCases))]
    public async Task SkaterOnTrack_SkaterAddedToJam_WithPivot_WhenJamIsCurrent_SetsLineupCorrectlyWithPivot_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterId, SkaterPosition.Pivot)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(PivotLineupTestCases))]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterId, SkaterPosition.Pivot)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, Guid.NewGuid(), SkaterPosition.Pivot)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithPivot_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, Guid.NewGuid(), SkaterPosition.Pivot)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOnTrack_WithBlocker_ReturnsSkaterAddedToJamEvent()
    {
        var skaterId = Guid.NewGuid();

        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, skaterId, SkaterPosition.Blocker)));

        implicitEvents.OfType<SkaterAddedToJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterAddedToJamBody(TeamSide.Home, 2, 6, skaterId, SkaterPosition.Blocker));
    }

    private static IEnumerable<TestCaseData> BlockerLineupTestCases()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();

        return [
            new(new JamLineupState(null, null, [null, null, null]), skaterIds[0], new JamLineupState(null, null, [skaterIds[0], null, null])),
            new(new JamLineupState(null, null, [skaterIds[1], null, null]), skaterIds[0], new JamLineupState(null, null, [skaterIds[1], skaterIds[0], null])),
            new(new JamLineupState(null, null, [skaterIds[2], skaterIds[3], skaterIds[4]]), skaterIds[0], new JamLineupState(null, null, [skaterIds[2], skaterIds[3], skaterIds[4], skaterIds[0]])),
            new(new JamLineupState(null, skaterIds[5], [skaterIds[2], skaterIds[3], skaterIds[4]]), skaterIds[0], new JamLineupState(null, skaterIds[5], [skaterIds[3], skaterIds[4], skaterIds[0]])),
            new(new JamLineupState(null, null, [skaterIds[2], skaterIds[0], skaterIds[4]]), skaterIds[0], new JamLineupState(null, null, [skaterIds[2], skaterIds[4], skaterIds[0]])),
            new(new JamLineupState(skaterIds[0], null, [null, null, null]), skaterIds[0], new JamLineupState(null, null, [skaterIds[0], null, null])),
            new(new JamLineupState(null, skaterIds[0], [null, null, null]), skaterIds[0], new JamLineupState(null, null, [skaterIds[0], null, null])),
            new(new JamLineupState(null, skaterIds[0], [skaterIds[2], skaterIds[3], skaterIds[4]]), skaterIds[0], new JamLineupState(null, null, [skaterIds[2], skaterIds[3], skaterIds[4], skaterIds[0]])),
        ];
    }

    [TestCaseSource(nameof(BlockerLineupTestCases))]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsCurrent_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 6, newSkaterId, SkaterPosition.Blocker)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(BlockerLineupTestCases))]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsUpcoming_AndNotInJam_SetsLineupCorrectly(JamLineupState initialState, Guid newSkaterId, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, newSkaterId, SkaterPosition.Blocker)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsPrevious_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 5, Guid.NewGuid(), SkaterPosition.Blocker)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterAddedToJam_WithBlocker_WhenJamIsUpcoming_AndInJam_DoesNotChangeState()
    {
        var initialState = State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterAddedToJam(0, new(TeamSide.Home, 2, 7, Guid.NewGuid(), SkaterPosition.Blocker)));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task SkaterOffTrack_WhenInJam_AndTeamMatches_RaisesSkaterRemovedFromJamEvent()
    {
        var skaterId = Guid.NewGuid();
        State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), skaterId, Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, skaterId)));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 6, skaterId));
    }

    [Test]
    public async Task SkaterOffTrack_WhenNotInJam_AndTeamMatches_RaisesSkaterRemovedFromJamEvent()
    {
        var skaterId = Guid.NewGuid();
        State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), skaterId, Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, skaterId)));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 7, skaterId));
    }

    [Test]
    public async Task SkaterOffTrack_WhenInJam_AndTeamDoesNotMatch_DoesNotRaiseSkaterRemovedFromJamEvent()
    {
        var skaterId = Guid.NewGuid();
        State = new(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid(), skaterId, Guid.NewGuid()]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Away, skaterId)));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().BeEmpty();
    }

    private static IEnumerable<TestCaseData> SkaterRemovedFromJamTestCases()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();

        return [
            new(skaterIds[0], new JamLineupState(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]), new JamLineupState(null, skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]])),
            new(skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[0], [skaterIds[2], skaterIds[3], skaterIds[4]]), new JamLineupState(skaterIds[1], null, [skaterIds[2], skaterIds[3], skaterIds[4]])),
            new(skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[0], skaterIds[3], skaterIds[4]]), new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], null])),
            new(skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[0], skaterIds[4]]), new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], null])),
            new(skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], skaterIds[0]]), new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], null])),
            new(skaterIds[0], new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], skaterIds[5]]), new JamLineupState(skaterIds[1], skaterIds[2], [skaterIds[3], skaterIds[4], skaterIds[5]])),
        ];
    }

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamIsCurrent_AndInJam_ClearsSkaterFromLineup(Guid skaterId, JamLineupState initialState, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 2, 6, skaterId)));

        State.Should().Be(expectedState);
    }

    [TestCaseSource(nameof(SkaterRemovedFromJamTestCases))]
    public async Task SkaterRemovedFromJam_WhenJamIsUpcoming_AndNotInJam_ClearsSkaterFromLineup(Guid skaterId, JamLineupState initialState, JamLineupState expectedState)
    {
        State = initialState;
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false, false));

        await Subject.Handle(new SkaterRemovedFromJam(0, new(TeamSide.Home, 2, 7, skaterId)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task SkaterOffTrack_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var originalState = State = new (skaterId, Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

        await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Away, skaterId)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterOffTrack_RaisesSkaterRemovedFromJamEvent()
    {
        var skaterId = Guid.NewGuid();
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false, false));

        var implicitEvents = await Subject.Handle(new SkaterOffTrack(0, new(TeamSide.Home, skaterId)));

        implicitEvents.OfType<SkaterRemovedFromJam>().Should().ContainSingle()
            .Which.Body.Should().Be(new SkaterRemovedFromJamBody(TeamSide.Home, 2, 6, skaterId));
    }

    [Test]
    public async Task JamEnded_ClearsLineup()
    {
        State = new(Guid.NewGuid(), Guid.NewGuid(), [null, null, null]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new([], []));

        _ = await Subject.Handle(new JamEnded(0));

        State.Should().Be(new JamLineupState(null, null, [null, null, null]));
    }

    [Test]
    public async Task JamEnded_RaisesSkaterOnTrackEvent_ForEachSkaterInBoxOrQueuedForBox()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new([skaterIds[0], skaterIds[1]], [skaterIds[2]]));
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Away), new([skaterIds[2], skaterIds[3]], []));

        var implicitEvents = await Subject.Handle(new JamEnded(1234));

        implicitEvents.OfType<SkaterOnTrack>().Should().HaveCount(3)
            .And.AllSatisfy(e => e.Tick.Should().Be(1234))
            .And.Subject.Select(x => x.Body).Should().BeEquivalentTo((SkaterOnTrackBody[])
            [
                new(TeamSide.Home, skaterIds[0], SkaterPosition.Jammer),
                new(TeamSide.Home, skaterIds[1], SkaterPosition.Pivot),
                new(TeamSide.Home, skaterIds[2], SkaterPosition.Blocker),
            ]);
    }

    [Test]
    public async Task SkaterSubstitutedInBox_RaisesEventsToReplaceSkaterInLineup()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Home), new([skaterIds[1]], []));
        MockKeyedState<PenaltyBoxState>(nameof(TeamSide.Away), new([], []));

        var implicitEvents = await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, skaterIds[1], skaterIds[5])));
        implicitEvents = implicitEvents.ToArray();

        implicitEvents.Where(e => e is SkaterOffTrack or SkaterOnTrack).Should().HaveCount(2);
        implicitEvents.Where(e => e is SkaterOffTrack { Body.TeamSide: TeamSide.Home } s && s.Body.SkaterId == skaterIds[1]).Should().ContainSingle();
        implicitEvents.Where(e => e is SkaterOnTrack { Body.TeamSide: TeamSide.Home } s && s.Body.SkaterId == skaterIds[5]).Should().ContainSingle();
    }


    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadMarked_RaisesLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, true, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterIds[0], "X")));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadMarkedForOpponent_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(true, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterIds[0], "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndLeadNotMarkedForEitherTeam_RaisesLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterIds[0], "X")));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task PenaltyAssessed_WhenNotAgainstJammer_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterIds[2], "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenTeamDoesNotMatch_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Away, skaterIds[0], "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNotInLineup_AndSkaterInPreviousLineup_AddsSkaterToLineup()
    {
        var skaterId = Guid.NewGuid();
        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterId, "X")));

        implicitEvents.OfType<PreviousJamSkaterOnTrack>().Should().ContainSingle()
            .Which.Body.SkaterId.Should().Be(skaterId);
    }

    [Test]
    public async Task PenaltyAssessed_WhenAgainstJammer_AndInOvertime_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState(new OvertimeState(true));

        var implicitEvents = await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, skaterIds[0], "X")));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task SkaterSatInBox_WhenJammerSatInBox_AndLeadMarked_RaisesLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, true, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterIds[0])));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task SkaterSatInBox_WhenJammerSatInBox_AndLeadMarkedForOpponent_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(true, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterIds[0])));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task SkaterSatInBox_WhenJammerSatInBox_AndLeadNotMarkedForEitherTeam_RaisesLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterIds[0])));

        implicitEvents.OfType<LostMarked>().Should().ContainSingle()
            .Which.Body.Lost.Should().BeTrue();
    }

    [Test]
    public async Task SkaterSatInBox_WhenNotJammer_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterIds[2])));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState<OvertimeState>(new(false));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Away, skaterIds[0])));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }

    [Test]
    public async Task SkaterSatInBox_WhenJammerSatInBox_AndInOvertime_DoesNotRaiseLostMarkedEvent()
    {
        var skaterIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        State = new(skaterIds[0], skaterIds[1], [skaterIds[2], skaterIds[3], skaterIds[4]]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Away), new(false, false, false, false, false));
        MockState(new OvertimeState(true));

        var implicitEvents = await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterIds[0])));

        implicitEvents.OfType<LostMarked>().Should().BeEmpty();
    }
}
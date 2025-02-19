using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class ScoreSheetUnitTests : ReducerUnitTest<HomeScoreSheet, ScoreSheetState>
{
    [Test]
    public async Task JamStarted_WhenNoPreviousJam_CreatesNewJamWithExpectedDefaults()
    {
        State = new([]);
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", [null, null, null]));
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new JamStarted(0));

        var jam = State.Jams.Should().ContainSingle().Subject;
        jam.Trips.Should().BeEmpty();
        jam.Lead.Should().BeFalse();
        jam.Lost.Should().BeFalse();
        jam.Called.Should().BeFalse();
        jam.Injury.Should().BeFalse();
        jam.NoInitial.Should().BeTrue();
        jam.GameTotal.Should().Be(0);
        jam.Period.Should().Be(2);
        jam.Jam.Should().Be(6);
        jam.JamTotal.Should().Be(0);
        jam.JammerNumber.Should().Be("123");
        jam.PivotNumber.Should().Be("321");
    }

    [Test]
    public async Task JamStarted_WhenPreviousJam_AddsJamWithExpectedGameTotal()
    {
        State = new([new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4)], null, 8, 8)]);
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", [null, null, null]));
        MockState<GameStageState>(new(Stage.Jam, 1, 2, 2, false));

        await Subject.Handle(new JamStarted(0));

        State.Jams.Should().HaveCount(2)
            .And.Subject.Last().GameTotal.Should().Be(8);
    }

    [Test]
    public async Task JamStarted_WhenJammerNumberNotSetAtJamStart_SetsJammerNumberAsQuestionMark()
    {
        State = new([]);
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new(null, "321", [null, null, null]));
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));

        await Subject.Handle(new JamStarted(0));

        State.Jams.Should().ContainSingle()
            .Which.JammerNumber.Should().Be("?");

    }

    [Test]
    public async Task SkaterAddedToJam_WhenJamDoesNotExistInSheet_AndSkaterIsJammer_AndTeamMatches_DoesNotChangeState([Values] Stage gameStage)
    {
        if (gameStage == Stage.Jam) return;

        var jam = new ScoreSheetJam(1, 1, "321", "555", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterAddedToJam(1000, new(TeamSide.Home, 1, 2, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task SkaterAddedToJam_WhenJamExistsInSheet_AndSkaterIsJammer_AndTeamMatches_SetsJammerNumber()
    {
        State = new([new(1, 1, "321", "555", false, false, false, false, false, [], null, 0, 0)]);

        await Subject.Handle(new SkaterAddedToJam(1000, new(TeamSide.Home, 1, 1, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.JammerNumber.Should().Be("123");
    }

    [Test]
    public async Task SkaterAddedToJam_WhenJamExistsInSheet_AndSkaterIsPivot_AndTeamMatches_SetsPivotNumber()
    {
        State = new([new(1, 1, "321", "555", false, false, false, false, false, [], null, 0, 0)]);

        await Subject.Handle(new SkaterAddedToJam(1000, new(TeamSide.Home, 1, 1, "123", SkaterPosition.Pivot)));

        State.Jams.Should().ContainSingle().Which.PivotNumber.Should().Be("123");
    }

    [Test]
    public async Task SkaterAddedToJam_WhenJamExistsInSheet_AndSkaterIsJammer_AndTeamDoesNotMatch_DoesNotChangeState()
    {
        var jam = new ScoreSheetJam(1, 1, "321", "555", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterAddedToJam(1000, new(TeamSide.Away, 1, 1, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task SkaterRemovedFromJam_WhenJamExistsInSheet_AndSkaterNumberIsJammer_RemovesJammerFromJam()
    {
        var jam = new ScoreSheetJam(1, 1, "321", "123", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterRemovedFromJam(1000, new(TeamSide.Home, 1, 1, "321")));

        State.Jams.Should().ContainSingle().Which.JammerNumber.Should().Be(string.Empty);
    }

    [Test]
    public async Task SkaterRemovedFromJam_WhenJamExistsInSheet_AndSkaterNumberIsPivot_RemovesPivotFromJam()
    {
        var jam = new ScoreSheetJam(1, 1, "321", "123", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterRemovedFromJam(1000, new(TeamSide.Home, 1, 1, "123")));

        State.Jams.Should().ContainSingle().Which.PivotNumber.Should().Be(string.Empty);
    }

    [Test]
    public async Task SkaterRemovedFromJam_WhenJamExistsInSheet_AndSkaterNumberIsNeitherJammerNorPivot_DoesNotChangeState()
    {
        var jam = new ScoreSheetJam(1, 1, "321", "123", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterRemovedFromJam(1000, new(TeamSide.Home, 1, 1, "555")));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task SkaterRemovedFromJam_WhenJamExistsInSheet_AndTeamDoesNotMatch_DoesNotChangeState()
    {
        var jam = new ScoreSheetJam(1, 1, "321", "123", false, false, false, false, false, [], null, 0, 0);
        State = new([jam]);

        await Subject.Handle(new SkaterRemovedFromJam(1000, new(TeamSide.Away, 1, 1, "123")));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task InitialTripCompleted_WhenCompletedTrue_AndTeamMatches_AddsASingleTrip()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [], null, 0, 13),
        ]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, true));

        await Subject.Handle(new InitialTripCompleted(1000, new(TeamSide.Home, true)));

        State.Jams.Should().HaveCount(3)
            .And.Subject.Last().Trips.Should().ContainSingle()
            .Which.Score.Should().BeNull();
    }

    [Test]
    public async Task InitialTripCompleted_WhenCompletedFalse_AndTeamMatches_ClearsAllTrips()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(0), new(0)], null, 0, 13),
        ]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));

        await Subject.Handle(new InitialTripCompleted(1000, new(TeamSide.Home, false)));

        State.Jams.Should().HaveCount(3)
            .And.Subject.Last().Trips.Should().BeEmpty();
    }

    [Test]
    public async Task InitialTripCompleted_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(0), new(0)], null, 0, 13),
        ]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));

        await Subject.Handle(new InitialTripCompleted(1000, new(TeamSide.Away, false)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TripCompleted_WhenTeamMatches_AddsBlankTripToLatestJam()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4)], null, 8, 21),
        ]);

        await Subject.Handle(new TripCompleted(1000, new(TeamSide.Home)));

        State.Jams.Should().HaveCount(3)
            .And.Subject.Last().Trips.Should().HaveCount(3)
            .And.Subject.Select(s => s.Score).Should().BeEquivalentTo([4, 4, (int?)null]);
    }

    [Test]
    public async Task TripCompleted_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4)], null, 8, 21),
        ]);

        await Subject.Handle(new TripCompleted(1000, new(TeamSide.Away)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreModifiedRelative_WhenTeamMatches_AddsScoreToLastTripScore_AndUpdatesJamAndGameTotals()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreModifiedRelative(1000, new(TeamSide.Home, 2)));

        State.Jams.Should().HaveCount(3)
            .And.Subject.Last().Trips.Should().HaveCount(3);

        var jam = State.Jams.Last();
        jam.Trips.Last().Score.Should().Be(2);
        jam.JamTotal.Should().Be(10);
        jam.GameTotal.Should().Be(23);
    }

    [Test]
    public async Task ScoreModifiedRelative_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreModifiedRelative(1000, new(TeamSide.Away, 2)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task LeadMarked_WhenTeamMatches_SetsCurrentJamLeadToValue([Values] bool isLead)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", !isLead, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[2] = expectedState.Jams[2] with { Lead = isLead };

        await Subject.Handle(new LeadMarked(1000, new(TeamSide.Home, isLead)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task LeadMarked_WhenTeamDoesNotMatch_DoesNotChangeState([Values] bool isLead)
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", !isLead, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new LeadMarked(1000, new(TeamSide.Away, isLead)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task LostMarked_WhenTeamMatches_SetsCurrentJamLostToValue([Values] bool isLost)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, !isLost, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[2] = expectedState.Jams[2] with { Lost = isLost };

        await Subject.Handle(new LostMarked(1000, new(TeamSide.Home, isLost)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task LostMarked_WhenTeamDoesNotMatch_DoesNotChangeState([Values] bool isLost)
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, !isLost, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new LeadMarked(1000, new(TeamSide.Away, isLost)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task CalledMarked_WhenTeamMatches_SetsCurrentJamCalledToValue([Values] bool called)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, !called, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[2] = expectedState.Jams[2] with { Called = called };

        await Subject.Handle(new CallMarked(1000, new(TeamSide.Home, called)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task CalledMarked_WhenTeamDoesNotMatch_DoesNotChangeState([Values] bool called)
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, called, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new CallMarked(1000, new(TeamSide.Away, called)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetJammerNumberSet_WhenJamLineExists_AndTeamMatches_SetsJammerNumberToValue()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { JammerNumber = "4444" };

        await Subject.Handle(new ScoreSheetJammerNumberSet(1000, new(TeamSide.Home, 1, "4444")));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetJammerNumberSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetJammerNumberSet(1000, new(TeamSide.Home, 10, "4444")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetJammerNumberSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetJammerNumberSet(1000, new(TeamSide.Away, 1, "4444")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetPivotNumberSet_WhenJamLineExists_AndTeamMatches_SetsPivotNumberToValue()
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { PivotNumber = "4444" };

        await Subject.Handle(new ScoreSheetPivotNumberSet(1000, new(TeamSide.Home, 1, "4444")));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetPivotNumberSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetPivotNumberSet(1000, new(TeamSide.Home, 10, "4444")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetPivotNumberSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetPivotNumberSet(1000, new(TeamSide.Away, 1, "4444")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetLeadSet_WhenJamLineExists_AndTeamMatches_SetsLeadToValue([Values] bool isLead)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", !isLead, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { Lead = isLead };

        await Subject.Handle(new ScoreSheetLeadSet(1000, new(TeamSide.Home, 1, isLead)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetLeadSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetLeadSet(1000, new(TeamSide.Home, 10, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetLeadSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetLeadSet(1000, new(TeamSide.Away, 1, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetLostSet_WhenJamLineExists_AndTeamMatches_SetsLostToValue([Values] bool isLost)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, !isLost, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { Lost = isLost };

        await Subject.Handle(new ScoreSheetLostSet(1000, new(TeamSide.Home, 1, isLost)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetLostSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetLostSet(1000, new(TeamSide.Home, 10, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetLostSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetLostSet(1000, new(TeamSide.Away, 1, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetCalledSet_WhenJamLineExists_AndTeamMatches_SetsCalledToValue([Values] bool isCalled)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, !isCalled, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { Called = isCalled };

        await Subject.Handle(new ScoreSheetCalledSet(1000, new(TeamSide.Home, 1, isCalled)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetCalledSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetCalledSet(1000, new(TeamSide.Home, 10, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetCalledSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetCalledSet(1000, new(TeamSide.Away, 1, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetInjurySet_WhenJamLineExists_SetsInjuryToValue([Values] bool isInjury)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, !isInjury, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { Injury = isInjury };

        await Subject.Handle(new ScoreSheetInjurySet(1000, new(1, isInjury)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetInjurySet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetInjurySet(1000, new(10, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetNoInitialSet_WhenJamLineExists_AndTeamMatches_SetsNoInitialToValue([Values] bool isNoInitial)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, !isNoInitial, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { NoInitial = isNoInitial };

        await Subject.Handle(new ScoreSheetNoInitialSet(1000, new(TeamSide.Home, 1, isNoInitial)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetNoInitialSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetNoInitialSet(1000, new(TeamSide.Home, 10, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetNoInitialSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, [new(4), new(4), new(null)], null, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetNoInitialSet(1000, new(TeamSide.Away, 1, true)));

        State.Should().Be(originalState);
    }

    [TestCase(TeamSide.Home, true, 3, null, 3)]
    [TestCase(TeamSide.Home, false, 3, 3, null)]
    [TestCase(TeamSide.Home, true, 0, null, 0)]
    [TestCase(TeamSide.Home, true, 1, null, 1)]
    [TestCase(TeamSide.Home, true, 3, 1, 3)]
    [TestCase(TeamSide.Home, false, 0, 0, null)]
    [TestCase(TeamSide.Home, false, 0, null, null)]
    [TestCase(TeamSide.Away, true, 3, null, null)]
    [TestCase(TeamSide.Away, false, 3, 3, 3)]
    public async Task StarPassMarked_SetsStarPassTripAsExpected(TeamSide teamSide, bool starPass, int currentTripCount, int? initialStarPassTrip, int? expectedStarPassTrip)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, Enumerable.Repeat(new JamLineTrip(4), currentTripCount).ToArray(), initialStarPassTrip, 8, 21),
        ]);

        await Subject.Handle(new StarPassMarked(1000, new(teamSide, starPass)));

        State.Jams[2].StarPassTrip.Should().Be(expectedStarPassTrip);
    }

    [TestCase(TeamSide.Home, 5, 3, null, 3)]
    [TestCase(TeamSide.Home, 3, 5, null, 3)]
    [TestCase(TeamSide.Home, 3, 0, null, 0)]
    [TestCase(TeamSide.Home, 3, null, 2, null)]
    [TestCase(TeamSide.Home, 3, 1, 2, 1)]
    [TestCase(TeamSide.Away, 3, 1, null, null)]
    public async Task ScoreSheetStarPassTripSet_SetsStarPassTripAsExpected(TeamSide setSide, int tripCount, int? starPassTrip, int? initialStarPassTrip, int? expectedStarPassTrip)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "?", "?", false, false, false, false, false, Enumerable.Repeat(new JamLineTrip(4), tripCount).ToArray(), initialStarPassTrip, 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetStarPassTripSet(1000, new(setSide, 2, starPassTrip)));

        State.Jams[2].StarPassTrip.Should().Be(expectedStarPassTrip);
    }

    private static TestCaseData[] ScoreSheetTripScoreSetTestCases =>
    [
        new(TeamSide.Home, 2, 3, new int?[]{4, 3, 2}, new int?[]{4, 3, 3}),
        new(TeamSide.Home, 3, 4, new int?[]{4, 4, 4}, new int?[]{4, 4, 4, 4}),
        new(TeamSide.Home, 4, 4, new int?[]{4, 4, 4}, new int?[]{4, 4, 4}),
        new(TeamSide.Home, 0, 4, new int?[]{1, 2, 3}, new int?[]{4, 2, 3}),
        new(TeamSide.Home, 2, null, new int?[]{1, 2, 3}, new int?[]{1, 2}),
        new(TeamSide.Home, 3, null, new int?[]{1, 2, 3}, new int?[]{1, 2, 3}),
        new(TeamSide.Home, 1, null, new int?[]{1, 2, 3}, new int?[]{1, 3}),
        new(TeamSide.Away, 2, 3, new int?[]{4, 3, 2}, new int?[]{4, 3, 2}),
    ];

    [TestCaseSource(nameof(ScoreSheetTripScoreSetTestCases))]
    public async Task ScoreSheetTripScoreSet_UpdatesStateAsExpected(TeamSide setSide, int tripToSet, int? setValue, int?[] currentTrips, int?[] expectedTrips)
    {
        State = new([
            new(1, 1, "123", "555", false, true, true, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "234", "555", false, false, false, false, false, [new(2)], null, 2, 13),
            new(1, 3, "345", "666", false, false, false, false, false, currentTrips.Select(s => new JamLineTrip(s)).ToArray(), null, currentTrips.Sum(i => i ?? 0), currentTrips.Sum(i => i ?? 0) + 13),
        ]);

        await Subject.Handle(new ScoreSheetTripScoreSet(1000, new(setSide, 2, tripToSet, setValue)));

        State.Jams[2].Trips.Select(t => t.Score).Should().BeEquivalentTo(expectedTrips);
        State.Jams[2].JamTotal.Should().Be(expectedTrips.Sum(i => i ?? 0));
        State.Jams[2].GameTotal.Should().Be(State.Jams[2].JamTotal + 13);
    }

    [Test]
    public async Task ScoreSheetTripScoreSet_CorrectlyRecalculatesGameTotals()
    {
        State = new([
            new(1, 1, "?", "?", false, false, false, false, false, [new(4), new(4), new(3)], null, 11, 11),
            new(1, 2, "?", "?", false, false, false, false, false, [new(4), new(2)], null, 6, 17),
            new(1, 3, "?", "?", false, false, false, false, false, [new(2)], null, 2, 19),
            new(1, 4, "?", "?", false, false, false, false, false, [new(0)], null, 0, 19),
            new(1, 5, "?", "?", false, false, false, false, false, [], null, 0, 19),
            new(1, 6, "?", "?", false, false, false, false, false, [new(4), new(4), new(4), new(4), new(3)], null, 19, 38),
        ]);

        await Subject.Handle(new ScoreSheetTripScoreSet(1000, new(TeamSide.Home, 2, 0, 4)));

        State.Jams.Select(j => j.GameTotal).Should().BeEquivalentTo([11, 17, 21, 21, 21, 40]);
    }
}
using System.Diagnostics.Contracts;
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
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321"));
        MockState<GameStageState>(new(Stage.Jam, 2, 6, false));

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
        jam.LineLabel.Should().Be("6");
    }

    [Test]
    public async Task JamStarted_WhenPreviousJam_AddsJamWithExpectedGameTotal()
    {
        State = new([new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4)], 8, 8)]);
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321"));
        MockState<GameStageState>(new(Stage.Jam, 1, 2, false));

        await Subject.Handle(new JamStarted(0));

        State.Jams.Should().HaveCount(2)
            .And.Subject.Last().GameTotal.Should().Be(8);
    }

    [Test]
    public async Task JamStarted_WhenJammerNumberNotSetAtJamStart_SetsJammerNumberAsQuestionMark()
    {
        State = new([]);
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new(null, "321"));
        MockState<GameStageState>(new(Stage.Jam, 2, 6, false));

        await Subject.Handle(new JamStarted(0));

        State.Jams.Should().ContainSingle()
            .Which.JammerNumber.Should().Be("?");

    }

    [Test]
    public async Task SkaterOnTrack_WhenGameStageNotInJam_AndSkaterIsJammer_AndTeamMatches_DoesNotChangeState([Values] Stage gameStage)
    {
        if (gameStage == Stage.Jam) return;

        var jam = new ScoreSheetJam(1, 1, "1", "321", false, false, false, false, false, [], 0, 0);
        State = new([jam]);
        MockState<GameStageState>(new(gameStage, 1, 2, false));

        await Subject.Handle(new SkaterOnTrack(1000, new(TeamSide.Home, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task SkaterOnTrack_WhenGameStageInJam_AndSkaterIsJammer_AndTeamMatches_SetsJammerNumber()
    {
        State = new([new(1, 1, "1", "321", false, false, false, false, false, [], 0, 0)]);
        MockState<GameStageState>(new(Stage.Jam, 1, 2, false));

        await Subject.Handle(new SkaterOnTrack(1000, new(TeamSide.Home, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.JammerNumber.Should().Be("123");
    }

    [Test]
    public async Task SkaterOnTrack_WhenGameStageInJam_AndSkaterIsPivot_AndTeamMatches_DoesNotChangeState()
    {
        var jam = new ScoreSheetJam(1, 1, "1", "321", false, false, false, false, false, [], 0, 0);
        State = new([jam]);
        MockState<GameStageState>(new(Stage.Jam, 1, 2, false));

        await Subject.Handle(new SkaterOnTrack(1000, new(TeamSide.Home, "123", SkaterPosition.Pivot)));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task SkaterOnTrack_WhenGameStageInJam_AndSkaterIsJammer_AndTeamDoesNotMatch_DoesNotChangeState()
    {
        var jam = new ScoreSheetJam(1, 1, "1", "321", false, false, false, false, false, [], 0, 0);
        State = new([jam]);
        MockState<GameStageState>(new(Stage.Jam, 1, 2, false));

        await Subject.Handle(new SkaterOnTrack(1000, new(TeamSide.Away, "123", SkaterPosition.Jammer)));

        State.Jams.Should().ContainSingle().Which.Should().Be(jam);
    }

    [Test]
    public async Task InitialTripCompleted_WhenCompletedTrue_AndTeamMatches_AddsASingleTrip()
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [], 0, 13),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(0), new(0)], 0, 13),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(0), new(0)], 0, 13),
        ]);
        MockKeyedState<TeamJamStatsState>(nameof(TeamSide.Home), new(false, false, false, false, false));

        await Subject.Handle(new InitialTripCompleted(1000, new(TeamSide.Away, false)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task TripCompleted_WhenTeamMatches_AddsBlankTripToLatestJam()
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4)], 8, 21),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4)], 8, 21),
        ]);

        await Subject.Handle(new TripCompleted(1000, new(TeamSide.Away)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreModifiedRelative_WhenTeamMatches_AddsScoreToLastTripScore_AndUpdatesJamAndGameTotals()
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4)], 8, 21),
        ]);

        await Subject.Handle(new ScoreModifiedRelative(1000, new(TeamSide.Away, 2)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task LeadMarked_WhenTeamMatches_SetsCurrentJamLeadToValue([Values] bool isLead)
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", !isLead, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", !isLead, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        await Subject.Handle(new LeadMarked(1000, new(TeamSide.Away, isLead)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task LostMarked_WhenTeamMatches_SetsCurrentJamLostToValue([Values] bool isLost)
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, !isLost, false, false, false, [new(4), new(4), new(null)], 8, 21),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, !isLost, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        await Subject.Handle(new LeadMarked(1000, new(TeamSide.Away, isLost)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task CalledMarked_WhenTeamMatches_SetsCurrentJamCalledToValue([Values] bool called)
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, !called, false, false, [new(4), new(4), new(null)], 8, 21),
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
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, called, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        await Subject.Handle(new CallMarked(1000, new(TeamSide.Away, called)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetJammerNumberSet_WhenJamLineExists_SetsJammerNumberToValue()
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { JammerNumber = "4444" };

        await Subject.Handle(new ScoreSheetJammerNumberSet(1000, new(1, "4444")));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetJammerNumberSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetJammerNumberSet(1000, new(10, "4444")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ScoreSheetLeadSet_WhenJamLineExists_SetsLeadToValue([Values] bool isLead)
    {
        State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", !isLead, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        var expectedState = new ScoreSheetState(State.Jams.Select(j => j).ToArray());
        expectedState.Jams[1] = expectedState.Jams[1] with { Lead = isLead };

        await Subject.Handle(new ScoreSheetLeadSet(1000, new(1, isLead)));

        State.Should().Be(expectedState);
    }

    [Test]
    public async Task ScoreSheetLeadSet_WhenJamLineDoesNotExist_DoesNotChangeState()
    {
        var originalState = State = new([
            new(1, 1, "1", "123", false, true, true, false, false, [new(4), new(4), new(3)], 11, 11),
            new(1, 2, "2", "234", false, false, false, false, false, [new(2)], 2, 13),
            new(1, 3, "3", "?", false, false, false, false, false, [new(4), new(4), new(null)], 8, 21),
        ]);

        await Subject.Handle(new ScoreSheetLeadSet(1000, new(10, true)));

        State.Should().Be(originalState);
    }
}
using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using jamster.Services;
using FluentAssertions;

namespace jamster.engine.tests.Reducers;

public class BoxTripsUnitTests : ReducerUnitTest<HomeBoxTrips, BoxTripsState>
{
    [Test]
    public async Task SkaterSatInBox_WhenNotAlreadyInBox_AddsBoxTrip([Values] bool afterStarPass)
    {
        State = new([], afterStarPass);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1234, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.Should().Be(new BoxTrip(2, 6, 20, afterStarPass, false, "123", SkaterPosition.Jammer, null, false, [], 1234, 0, 0));
    }

    [Test]
    public async Task SkaterSatInBox_WhenPreviouslyReleasedFromBox_AddsBoxTrip()
    {
        State = new([new(2, 5, 19, false, false, "123", SkaterPosition.Blocker, 0, false, [], 0, 0, 30000)], false);
        MockState<GameStageState>(new(Stage.Jam, 20, 2, 6, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(60000, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().HaveCount(2);
    }

    [Test]
    public async Task SkaterSatInBox_WhenAlreadyInBox_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, false, false, "123", SkaterPosition.Jammer, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenAnotherSkaterInBox_DoesNotImpactExistingSkater()
    {
        State = new([new(2, 6, 20, false, false, "321", SkaterPosition.Pivot, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Home, "123")));

        State.BoxTrips[0].Should().Be(originalState.BoxTrips[0]);
        State.BoxTrips.Should().HaveCount(2);
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, false, false, "321", SkaterPosition.Pivot, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenNotInJam_MarksSatBetweenJams([Values(Stage.BeforeGame, Stage.Lineup, Stage.Timeout, Stage.AfterTimeout, Stage.Intermission, Stage.AfterGame)] Stage stage)
    {
        State = new([], false);
        MockState<GameStageState>(new(stage, 1, 1, 1, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, "1")));

        State.BoxTrips[0].StartBetweenJams.Should().BeTrue();
    }

    [TestCase("123", SkaterPosition.Jammer)]
    [TestCase("321", SkaterPosition.Pivot)]
    [TestCase("1", SkaterPosition.Blocker)]
    public async Task SkaterSatInBox_CorrectlyRecordsSkaterPosition(string skaterNumber, SkaterPosition expectedPosition)
    {
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));
        MockState<GameStageState>(new(Stage.Jam, 1, 1, 1, false));

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterNumber)));

        State.BoxTrips.Single().SkaterPosition.Should().Be(expectedPosition);
    }

    [Test]
    public async Task SkaterSatInBox_WhenSkaterNotOnTrack_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, false, false, "321", SkaterPosition.Pivot, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Home, "4")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenSkaterInBox_MarksBoxTripAsCompleted([Values] bool afterStarPass)
    {
        State = new([new(2, 6, 20, false, false, "123", SkaterPosition.Jammer, null, false, [], 0, 0, 0)], afterStarPass);
        MockState<GameStageState>(new(Stage.Jam, 2, 8, 22, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.Should().Be(new BoxTrip(2, 6, 20, false, false, "123", SkaterPosition.Jammer, 2, afterStarPass, [], 0, 0, 1234));
    }

    [Test]
    public async Task SkaterReleasedFromBox_AfterPeriodChange_MarksDurationCorrectly()
    {
        State = new([new(1, 14, 14, false, false, "123", SkaterPosition.Jammer, null, false, [], 0, 0, 0)], false);
        MockState<GameStageState>(new(Stage.Jam, 2, 1, 15, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.DurationInJams.Should().Be(1);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenSkaterNotInBox_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, false, false, "321", SkaterPosition.Pivot, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, false, false, "123", SkaterPosition.Jammer, null, false, [], 0, 0, 0)], false);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 8, 22, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamStarted_SetsLastStartTick_AndSetsTicksPassedAtLastStart_ForAllRunningTrips()
    {
        State = new(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 5000, 3000, 8000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        );

        await Subject.Handle(new JamStarted(12345));

        State.Should().Be(new BoxTripsState(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 12345, 10000, 10000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 12345, 8000, 8000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        ));
    }

    [Test]
    public async Task JamEnded_UpdatesTicksPassed()
    {
        State = new(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 5000, 3000, 8000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        );

        await Subject.Handle(new JamEnded(15000));

        State.Should().Be(new BoxTripsState(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 15000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 5000, 3000, 13000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        ));
    }

    [Test]
    public async Task Tick_WhenInJam_UpdatesTicksPassedOnAllRunningTrips()
    {
        State = new(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 5000, 3000, 8000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        );
        MockState<GameStageState>(new(Stage.Jam, 1, 1, 1, false));

        await ((ITickReceiver)Subject).TickAsync(15000);

        State.Should().Be(new BoxTripsState(
            [
                new(1, 1, 1, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 15000),
                new(1, 1, 1, false, false, "2", SkaterPosition.Blocker, null, false, [], 5000, 3000, 13000),
                new(1, 1, 1, false, false, "3", SkaterPosition.Blocker, 0, false, [], 0, 0, 5000),
            ],
            false
        ));
    }

    [Test]
    public async Task SkaterSubstitutedInBox_AddsSubstitutionToTrip()
    {
        State = new(
            [
                new(1, 5, 5, false, false, "1", SkaterPosition.Blocker, 1, false, [], 0, 0, 0),
                new(2, 6, 20, false, false, "1", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
                new(2, 6, 20, false, false, "2", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
            ],
            false
        );
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, "1", "3")));

        State.Should().Be(new BoxTripsState(
            [
                new(1, 5, 5, false, false, "1", SkaterPosition.Blocker, 1, false, [], 0, 0, 0),
                new(2, 6, 20, false, false, "1", SkaterPosition.Blocker, null, false, [new("3", 21)], 0, 0, 10000),
                new(2, 6, 20, false, false, "2", SkaterPosition.Blocker, null, false, [], 0, 0, 10000),
            ],
            false
        ));
    }

    [Test]
    public async Task JamStarted_ClearsStarPassFlag()
    {
        State = new([], true);

        await Subject.Handle(new JamStarted(0));

        State.HasStarPassInJam.Should().BeFalse();
    }

    [Test]
    public async Task StarPassMarked_WhenTeamMatches_SetsStarPassAccordingly([Values] bool starPass)
    {
        State = new([], !starPass);

        await Subject.Handle(new StarPassMarked(0, new(TeamSide.Home, starPass)));

        State.HasStarPassInJam.Should().Be(starPass);
    }

    [Test]
    public async Task StarPassMarked_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([], false);

        await Subject.Handle(new StarPassMarked(0, new(TeamSide.Away, true)));

        State.HasStarPassInJam.Should().BeFalse();
    }

    [Test]
    public void State_ShouldSerializeCorrectly()
    {
        var state = new BoxTripsState(
            [new(1, 2, 3, true, false, "123", SkaterPosition.Blocker, 3, true, [new("321", 4)], 1234, 4321, 5678)],
            true);

        var serialized = System.Text.Json.JsonSerializer.Serialize(state, Program.JsonSerializerOptions);

        var deserialized = System.Text.Json.Nodes.JsonNode.Parse(serialized)!;

        var boxTrip = deserialized["boxTrips"]!.AsArray()[0]!;
        boxTrip["period"]!.AsValue().GetValue<int>().Should().Be(1);
        boxTrip["jam"]!.AsValue().GetValue<int>().Should().Be(2);
        boxTrip["totalJamStart"]!.AsValue().GetValue<int>().Should().Be(3);
        boxTrip["startAfterStarPass"]!.AsValue().GetValue<bool>().Should().BeTrue();
        boxTrip["startBetweenJams"]!.AsValue().GetValue<bool>().Should().BeFalse();
        boxTrip["skaterNumber"]!.AsValue().GetValue<string>().Should().Be("123");
        boxTrip["skaterPosition"]!.AsValue().GetValue<string>().Should().Be("Blocker");
        boxTrip["durationInJams"]!.AsValue().GetValue<int>().Should().Be(3);
        boxTrip["endAfterStarPass"]!.AsValue().GetValue<bool>().Should().BeTrue();

        var substitution = boxTrip["substitutions"]!.AsArray()[0]!;
        substitution["newNumber"]!.AsValue().GetValue<string>().Should().Be("321");
        substitution["totalJamNumber"]!.AsValue().GetValue<int>().Should().Be(4);

        boxTrip["lastStartTick"]!.AsValue().GetValue<int>().Should().Be(1234);
        boxTrip["ticksPassedAtLastStart"]!.AsValue().GetValue<int>().Should().Be(4321);
        boxTrip["ticksPassed"]!.AsValue().GetValue<int>().Should().Be(5678);
        boxTrip["secondsPassed"]!.AsValue().GetValue<int>().Should().Be(5);
    }
}
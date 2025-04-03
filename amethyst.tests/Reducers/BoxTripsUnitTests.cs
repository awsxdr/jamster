using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class BoxTripsUnitTests : ReducerUnitTest<HomeBoxTrips, BoxTripsState>
{
    [Test]
    public async Task SkaterSatInBox_WhenNotAlreadyInBox_AddsBoxTrip()
    {
        State = new([]);
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1234, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.Should().Be(new BoxTrip(2, 6, 20, "123", SkaterPosition.Jammer, null, [], 1234, 0, 0, 0));
    }

    [Test]
    public async Task SkaterSatInBox_WhenPreviouslyReleasedFromBox_AddsBoxTrip()
    {
        State = new([new(2, 5, 19, "123", SkaterPosition.Blocker, 0, [], 0, 0, 30000, 30)]);
        MockState<GameStageState>(new(Stage.Jam, 20, 2, 6, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(60000, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().HaveCount(2);
    }

    [Test]
    public async Task SkaterSatInBox_WhenAlreadyInBox_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, "123", SkaterPosition.Jammer, null, [], 0, 0, 0, 0)]);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenAnotherSkaterInBox_DoesNotImpactExistingSkater()
    {
        State = new([new(2, 6, 20, "321", SkaterPosition.Pivot, null, [], 0, 0, 0, 0)]);
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
        State = new([new(2, 6, 20, "321", SkaterPosition.Pivot, null, [], 0, 0, 0, 0)]);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [TestCase("123", SkaterPosition.Jammer)]
    [TestCase("321", SkaterPosition.Pivot)]
    [TestCase("12", SkaterPosition.Blocker)]
    public async Task SkaterSatInBox_CorrectlyRecordsSkaterPosition(string skaterNumber, SkaterPosition expectedPosition)
    {

    }

    [Test]
    public async Task SkaterSatInBox_WhenSkaterNotOnTrack_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, "321", SkaterPosition.Pivot, null, [], 0, 0, 0, 0)]);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterSatInBox(1000, new(TeamSide.Home, "4")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenSkaterInBox_MarksBoxTripAsCompleted()
    {
        State = new([new(2, 6, 20, "123", SkaterPosition.Jammer, null, [], 0, 0, 0, 0)]);
        MockState<GameStageState>(new(Stage.Jam, 2, 8, 22, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.Should().Be(new BoxTrip(2, 6, 20, "123", SkaterPosition.Jammer, 2, [], 0, 0, 1234, 1));
    }

    [Test]
    public async Task SkaterReleasedFromBox_AfterPeriodChange_MarksDurationCorrectly()
    {
        State = new([new(1, 14, 14, "123", SkaterPosition.Jammer, null, [], 0, 0, 0, 0)]);
        MockState<GameStageState>(new(Stage.Jam, 2, 1, 15, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, "123")));

        State.BoxTrips.Should().ContainSingle()
            .Which.DurationInJams.Should().Be(1);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenSkaterNotInBox_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, "321", SkaterPosition.Pivot, null, [], 0, 0, 0, 0)]);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 6, 20, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([new(2, 6, 20, "123", SkaterPosition.Jammer, null, [], 0, 0, 0, 0)]);
        var originalState = State;
        MockState<GameStageState>(new(Stage.Jam, 2, 8, 22, false));
        MockKeyedState<JamLineupState>(nameof(TeamSide.Home), new("123", "321", ["1", "2", "3"]));

        await Subject.Handle(new SkaterReleasedFromBox(1234, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task JamStarted_SetsLastStartTick_AndSetsTicksPassedAtLastStart_ForAllRunningTrips()
    {
        State = new([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 5000, 3000, 8000, 8),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]);

        await Subject.Handle(new JamStarted(12345));

        State.Should().Be(new BoxTripsState([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 12345, 10000, 10000, 10),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 12345, 8000, 8000, 8),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]));
    }

    [Test]
    public async Task JamEnded_UpdatesTicksPassed()
    {
        State = new([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 5000, 3000, 8000, 8),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]);

        await Subject.Handle(new JamEnded(15000));

        State.Should().Be(new BoxTripsState([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 0, 0, 15000, 15),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 5000, 3000, 13000, 13),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]));
    }

    [Test]
    public async Task Tick_WhenInJam_UpdatesTicksPassedOnAllRunningTrips()
    {
        State = new([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 5000, 3000, 8000, 8),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 1, 1, false));

        await ((ITickReceiver)Subject).TickAsync(15000);

        State.Should().Be(new BoxTripsState([
            new(1, 1, 1, "1", SkaterPosition.Blocker, null, [], 0, 0, 15000, 15),
            new(1, 1, 1, "2", SkaterPosition.Blocker, null, [], 5000, 3000, 13000, 13),
            new(1, 1, 1, "3", SkaterPosition.Blocker, 0, [], 0, 0, 5000, 5),
        ]));
    }

    [Test]
    public async Task SkaterSubstitutedInBox_AddsSubstitutionToTrip()
    {
        State = new([
            new(1, 5, 5, "1", SkaterPosition.Blocker, 1, [], 0, 0, 0, 0),
            new(2, 6, 20, "1", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
            new(2, 6, 20, "2", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
        ]);
        MockState<GameStageState>(new(Stage.Lineup, 2, 6, 20, false));

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, "1", "3")));

        State.Should().Be(new BoxTripsState([
            new(1, 5, 5, "1", SkaterPosition.Blocker, 1, [], 0, 0, 0, 0),
            new(2, 6, 20, "1", SkaterPosition.Blocker, null, [new("3", 21)], 0, 0, 10000, 10),
            new(2, 6, 20, "2", SkaterPosition.Blocker, null, [], 0, 0, 10000, 10),
        ]));
    }
}
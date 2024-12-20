using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TripScoreUnitTests : ReducerUnitTest<HomeTripScore, TripScoreState>
{
    [TestCase(1, 3, 0, 10000, TeamSide.Home, 3, 10000)]
    [TestCase(1, 3, 8000, 10000, TeamSide.Home, 4, 10000)]
    [TestCase(1, 3, 8000, 10000, TeamSide.Away, 1, 8000)]
    [TestCase(2, -3, 8000, 10000, TeamSide.Home, 0, 10000)]
    [TestCase(2, 3, 8000, 10000, TeamSide.Home, 4, 10000)]
    [TestCase(null, 0, 0, 1000, TeamSide.Home, 0, 1000)]
    public async Task ScoreModifiedRelative_WhenJamRunning_SetsTripScoreAsExpected(int? startScore, int value, long lastUpdateTick, long eventTick, TeamSide teamSide, int expectedScore, long expectedTick)
    {
        State = new(startScore, lastUpdateTick);
        MockState(new JamClockState(true, 0, 0, 0));

        await Subject.Handle(new ScoreModifiedRelative(eventTick, new(teamSide, value)));

        State.LastChangeTick.Should().Be(expectedTick);
        State.Score.Should().Be(expectedScore);
    }

    [Test]
    public async Task ScoreModifiedRelative_WhenJamNotRunning_DoesNotResetTripScoreBeforeAdjusting()
    {
        State = new(3, 0);
        MockState(new JamClockState(false, 0, 0, 0));

        await Subject.Handle(new ScoreModifiedRelative(TripScore.TripScoreResetTimeInTicks + Domain.Tick.FromSeconds(2), new(TeamSide.Home, 1)));

        State.Score.Should().Be(4);
    }

    [Test]
    public async Task ScoreSet_SetsTripScoreToNull()
    {
        State = new(3, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        await Subject.Handle(new ScoreSet(1000, new(TeamSide.Home, 20)));

        State.Score.Should().Be(null);
        State.LastChangeTick.Should().Be(1000);
    }

    [Test]
    public async Task ScoreSet_IgnoresEventsForOtherTeam()
    {
        State = new(3, 123);
        MockState(new JamClockState(true, 0, 0, 0));

        await Subject.Handle(new ScoreSet(1000, new(TeamSide.Away, 20)));

        State.Score.Should().Be(3);
        State.LastChangeTick.Should().Be(123);
    }

    [Test]
    public async Task JamEnded_ClearsTripScore()
    {
        State = new(3, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        await Subject.Handle(new JamEnded(1000));

        State.Score.Should().Be(null);
        State.LastChangeTick.Should().Be(1000);
    }

    [Test]
    public async Task LastTripDeleted_WhenTeamMatches_ClearsTripScore()
    {
        State = new(2, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        await Subject.Handle(new LastTripDeleted(0, new(TeamSide.Home)));

        State.Score.Should().Be(null);
    }

    [Test]
    public async Task LastTripDeleted_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new(2, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        var originalState = State;

        await Subject.Handle(new LastTripDeleted(1000, new(TeamSide.Away)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task Tick_WhenJamRunning_ClearsTripScoreAfterSetTime()
    {
        State = new(3, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        await Tick(TripScore.TripScoreResetTimeInTicks - 1);

        State.Score.Should().Be(3);
        State.LastChangeTick.Should().Be(0);

        await Tick(TripScore.TripScoreResetTimeInTicks);

        State.Score.Should().Be(null);
        State.LastChangeTick.Should().Be(TripScore.TripScoreResetTimeInTicks);
    }

    [Test]
    public async Task Tick_WhenJamNotRunning_DoesNotClearTripScore()
    {
        var checkTick = TripScore.TripScoreResetTimeInTicks + Domain.Tick.FromSeconds(2);

        State = new(3, 0);
        MockState(new JamClockState(false, 0, checkTick, checkTick.Seconds));

        await Tick(checkTick);

        State.Score.Should().Be(3);
        State.LastChangeTick.Should().Be(0);
    }

    [Test]
    public async Task Tick_WhenJamRunning_AndTripScoreClear_DoesNotChangeState()
    {
        State = new(null, 0);
        MockState(new JamClockState(true, 0, 0, 0));

        await Tick(TripScore.TripScoreResetTimeInTicks + 1);

        State.LastChangeTick.Should().Be(0);
    }
}
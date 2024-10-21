using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class PassScoreUnitTests : ReducerUnitTest<HomePassScore, PassScoreState>
{
    [TestCase(1, 3, 0, 10000, TeamSide.Home, 3, 10000)]
    [TestCase(1, 3, 8000, 10000, TeamSide.Home, 4, 10000)]
    [TestCase(1, 3, 8000, 10000, TeamSide.Away, 1, 8000)]
    [TestCase(2, -3, 8000, 10000, TeamSide.Home, 0, 10000)]
    [TestCase(2, 3, 8000, 10000, TeamSide.Home, 4, 10000)]
    public async Task ScoreModifiedRelative_SetsPassScoreAsExpected(int startScore, int value, long lastUpdateTick, long eventTick, TeamSide teamSide, int expectedScore, long expectedTick)
    {
        State = new(startScore, lastUpdateTick);

        await Subject.Handle(new ScoreModifiedRelative(eventTick, new(teamSide, value)));

        State.LastChangeTick.Should().Be(expectedTick);
        State.Score.Should().Be(expectedScore);
    }

    [Test]
    public async Task ScoreSet_SetsPassScoreTo0()
    {
        State = new(3, 0);

        await Subject.Handle(new ScoreSet(1000, new(TeamSide.Home, 20)));

        State.Score.Should().Be(0);
        State.LastChangeTick.Should().Be(1000);
    }

    [Test]
    public async Task ScoreSet_IgnoresEventsForOtherTeam()
    {
        State = new(3, 123);

        await Subject.Handle(new ScoreSet(1000, new(TeamSide.Away, 20)));

        State.Score.Should().Be(3);
        State.LastChangeTick.Should().Be(123);
    }

    [Test]
    public async Task Tick_ClearsJamScoreAfterSetTime()
    {
        State = new(3, 0);

        await Tick(PassScore.PassScoreResetTimeInTicks - 1);

        State.Score.Should().Be(3);
        State.LastChangeTick.Should().Be(0);

        await Tick(PassScore.PassScoreResetTimeInTicks);

        State.Score.Should().Be(0);
        State.LastChangeTick.Should().Be(PassScore.PassScoreResetTimeInTicks);
    }

    [Test]
    public async Task Tick_WhenPassScoreIs0_DoesNotUpdateState()
    {
        State = new(0, 123);

        await Tick(PassScore.PassScoreResetTimeInTicks + State.LastChangeTick);

        State.LastChangeTick.Should().Be(123);
    }
}
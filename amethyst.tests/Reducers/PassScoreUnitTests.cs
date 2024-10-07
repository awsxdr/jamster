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
}
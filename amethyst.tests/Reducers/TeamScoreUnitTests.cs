using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamScoreUnitTests : ReducerUnitTest<HomeTeamScore, TeamScoreState>
{
    [TestCase(10, TeamSide.Home, 5, 15)]
    [TestCase(10, TeamSide.Home, -5, 5)]
    [TestCase(10, TeamSide.Away, 5, 10)]
    [TestCase(5, TeamSide.Home, -10, 0)]
    public async Task ScoreModifiedRelative_UpdatesScoreAsExpected(int startScore, TeamSide teamSide, int modifyValue, int expectedScore)
    {
        State = new(startScore);

        await Subject.Handle(new ScoreModifiedRelative(0, new ScoreModifiedRelativeBody(teamSide, modifyValue)));

        State.Score.Should().Be(expectedScore);
    }

    [TestCase(TeamSide.Home, 5, 5)]
    [TestCase(TeamSide.Home, -5, 0)]
    [TestCase(TeamSide.Away, 5, 10)]
    public async Task ScoreSet_UpdatesScoreAsExpected(TeamSide teamSide, int value, int expectedScore)
    {
        State = new(10);

        await Subject.Handle(new ScoreSet(0, new(teamSide, value)));

        State.Score.Should().Be(expectedScore);
    }
}
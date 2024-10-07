using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamScoreUnitTests : ReducerUnitTest<HomeTeamScore, TeamScoreState>
{
    [TestCase(10, Team.Home, 5, 15)]
    [TestCase(10, Team.Home, -5, 5)]
    [TestCase(10, Team.Away, 5, 10)]
    [TestCase(5, Team.Home, -10, 0)]
    public async Task ScoreModifiedRelative_UpdatesScoreAsExpected(int startScore, Team team, int modifyValue, int expectedScore)
    {
        State = new(startScore);

        await Subject.Handle(new ScoreModifiedRelative(0, new ScoreModifiedRelativeBody(team, modifyValue)));

        State.Score.Should().Be(expectedScore);
    }

    [TestCase(Team.Home, 5, 5)]
    [TestCase(Team.Home, -5, 0)]
    [TestCase(Team.Away, 5, 10)]
    public async Task ScoreSet_UpdatesScoreAsExpected(Team team, int value, int expectedScore)
    {
        State = new(10);

        await Subject.Handle(new ScoreSet(0, new(team, value)));

        State.Score.Should().Be(expectedScore);
    }
}
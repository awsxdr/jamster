using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamScoreUnitTests : ReducerUnitTest<HomeTeamScore, TeamScoreState>
{
    [TestCase(10, TeamSide.Home, 5, 15, 11)]
    [TestCase(10, TeamSide.Home, -5, 5, 1)]
    [TestCase(10, TeamSide.Away, 5, 10, 6)]
    [TestCase(5, TeamSide.Home, -10, 0, 0)]
    public async Task ScoreModifiedRelative_UpdatesScoreAsExpected(int startScore, TeamSide teamSide, int modifyValue, int expectedScore, int expectedJamScore)
    {
        State = new(startScore, 6);

        await Subject.Handle(new ScoreModifiedRelative(0, new ScoreModifiedRelativeBody(teamSide, modifyValue)));

        State.Should().Be(new TeamScoreState(expectedScore, expectedJamScore));
    }

    [TestCase(TeamSide.Home, 5, 5, 0)]
    [TestCase(TeamSide.Home, -5, 0, 0)]
    [TestCase(TeamSide.Away, 5, 10, 3)]
    [TestCase(TeamSide.Home, 8, 8, 1)]
    [TestCase(TeamSide.Home, 12, 12, 5)]
    public async Task ScoreSet_UpdatesScoreAsExpected(TeamSide teamSide, int value, int expectedScore, int expectedJamScore)
    {
        State = new(10, 3);

        await Subject.Handle(new ScoreSet(0, new(teamSide, value)));

        State.Should().Be(new TeamScoreState(expectedScore, expectedJamScore));
    }
}
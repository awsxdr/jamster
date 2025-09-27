using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

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

    [TestCase(10, 5, TeamSide.Home, 3, 7, 2)]
    [TestCase(3, 3, TeamSide.Home, 4, 0, 0)]
    [TestCase(10, 5, TeamSide.Home, null, 10, 5)]
    [TestCase(10, 5, TeamSide.Away, 4, 10, 5)]
    [TestCase(10, 2, TeamSide.Home, 4, 6, 0)]
    public async Task LastTripDeleted_WhenTeamMatches_RemovesPointsFromTrip(int totalScore, int jamScore, TeamSide teamSide, int? tripScore, int expectedTotalScore, int expectedJamScore)
    {
        State = new(totalScore, jamScore);
        MockKeyedState("Home", new TripScoreState(tripScore, 1, 0));

        await Subject.Handle(new LastTripDeleted(0, new(teamSide)));

        State.Should().Be(new TeamScoreState(expectedTotalScore, expectedJamScore));
    }
}
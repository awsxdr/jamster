using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamDetailsUnitTests : ReducerUnitTest<HomeTeamDetails, TeamDetailsState>
{
    [Test]
    public async Task TeamSet_UpdatesTeam()
    {
        var team = new Team(
            Guid.NewGuid(),
            new()
            {
                ["test"] = "Test Team",
            },
            new()
            {
                ["test"] = new DisplayColor(Color.Black, Color.White),
            },
            Enumerable.Range(1, 15).Select(i => new Skater(i.ToString(), $"Skater {i}", "test/test", SkaterRole.Skater)).ToList());

        State = new(new Team(Guid.NewGuid(), [], [], []));

        await Subject.Handle(new TeamSet(0, new(team)));

        State.Team.Should().Be(team);
    }
}
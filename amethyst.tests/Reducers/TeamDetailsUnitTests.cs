using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamDetailsUnitTests : ReducerUnitTest<HomeTeamDetails, TeamDetailsState>
{
    [Test]
    public async Task TeamSet_UpdatesTeam()
    {
        var updateTime = DateTimeOffset.UtcNow;

        var team = new Team(
            Guid.NewGuid(),
            new()
            {
                ["test"] = "Test Team",
            },
            new()
            {
                ["White"] = new() { ["test"] = new DisplayColor(Color.Black, Color.White) }
            },
            Enumerable.Range(1, 15).Select(i => new Skater(i.ToString(), $"Skater {i}", "test/test", SkaterRole.Skater)).ToList(),
            updateTime
            );

        State = new(new Team(Guid.NewGuid(), [], [], [], DateTimeOffset.MinValue));

        await Subject.Handle(new TeamSet(0, new(team)));

        State.Team.Should().Be(team with { LastUpdateTime = updateTime });
    }
}
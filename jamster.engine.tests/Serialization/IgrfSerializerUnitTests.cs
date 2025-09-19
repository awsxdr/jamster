using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using jamster.Serialization;
using jamster.Services;
using jamster.engine.tests.GameGeneration;
using FluentAssertions;

namespace jamster.engine.tests.Serialization;

public class IgrfSerializerUnitTests : UnitTest<IgrfSerializer>
{
    [Test]
    public void Serialize_CorrectlySerializesTeams()
    {
        var homeTeam = ToGameTeam(TeamGenerator.GenerateRandom().DomainTeam);
        var awayTeam = ToGameTeam(TeamGenerator.GenerateRandom().DomainTeam);

        MockState(new RulesState(Rules.DefaultRules));
        MockState(new GameSummaryState(GameProgress.Finished, new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), new([0, 0], 0), [0, 0]));
        MockKeyedState(TeamSide.Home, new TeamDetailsState(homeTeam));
        MockKeyedState(TeamSide.Away, new TeamDetailsState(awayTeam));

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.Teams.HomeTeam.Should().Be(
            new StatsBookTeam(
                NameOrEmpty(homeTeam, "league"),
                NameOrEmpty(homeTeam, "team"),
                NameOrEmpty(homeTeam, "color"),
                homeTeam.Roster.Select(s => new StatsBookSkater(s.Number, s.Name, s.IsSkating)).ToArray()));

        result.Teams.AwayTeam.Should().Be(
            new StatsBookTeam(
                NameOrEmpty(awayTeam, "league"),
                NameOrEmpty(awayTeam, "team"),
                NameOrEmpty(awayTeam, "color"),
                awayTeam.Roster.Select(s => new StatsBookSkater(s.Number, s.Name, s.IsSkating)).ToArray()));

        return;

        string NameOrEmpty(GameTeam team, string nameKey) =>
            team.Names.TryGetValue(nameKey, out var name) ? name : string.Empty;

        GameTeam ToGameTeam(Team team) => new(
            team.Names,
            team.Colors.Values.First(),
            team.Roster.Select(s => new GameSkater(s.Number, s.Name, true)).ToList());
    }

    [Test]
    public void Serialize_CorrectRecordsTotals()
    {
        MockState(new RulesState(Rules.DefaultRules));
        MockState(new GameSummaryState(GameProgress.Finished, new([10, 20], 30), new([15, 35], 50), new([10, 7], 17), new([8, 5], 13), [20, 10]));
        MockKeyedState(TeamSide.Home, new TeamDetailsState(new([], new(Color.White, Color.Black), [])));
        MockKeyedState(TeamSide.Away, new TeamDetailsState(new([], new(Color.White, Color.Black), [])));

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.GameSummary.Period1Summary.Should().Be(new PeriodSummary(10, 10, 8, 15));
        result.GameSummary.Period2Summary.Should().Be(new PeriodSummary(7, 20, 5, 35));
    }

    private void MockState<TState>(TState state) where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetState<TState>()).Returns(state);

    private void MockKeyedState<TKey, TState>(TKey key, TState state) where TKey : notnull where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetKeyedState<TState>(key.ToString() ?? "")).Returns(state);
}
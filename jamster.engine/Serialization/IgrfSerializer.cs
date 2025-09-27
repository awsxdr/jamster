using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Serialization;

public interface IIgrfSerializer
{
    Igrf Serialize(IGameStateStore stateStore);
}

[Singleton]
public class IgrfSerializer : IIgrfSerializer
{
    public Igrf Serialize(IGameStateStore stateStore)
    {
        var homeTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Home));
        var awayTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Away));

        return new(
            GetGameLocation(),
            GetGameDetails(),
            new(GetTeam(homeTeam), GetTeam(awayTeam)),
            GetGameSummary(stateStore)
        );
    }

    private static GameLocation GetGameLocation() =>
        new("", "", "");

    private static GameDetails GetGameDetails() =>
        new("", "", "", DateTime.UtcNow);

    private static StatsBookTeam GetTeam(TeamDetailsState team) =>
        new(
            team.Team.Names.GetValueOrDefault("league", ""),
            team.Team.Names.GetValueOrDefault("team", ""),
            team.Team.Names.GetValueOrDefault("color", ""),
            team.Team.Roster.Select(skater => new StatsBookSkater(skater.Number, skater.Name, skater.IsSkating)).ToArray()
        );

    private static GameSummary GetGameSummary(IGameStateStore stateStore)
    {
        var gameSummaryState = stateStore.GetState<GameSummaryState>();
        var rules = stateStore.GetState<RulesState>().Rules;

        return new(
            new(
                gameSummaryState.HomePenalties.PeriodTotals[0],
                gameSummaryState.HomeScore.PeriodTotals[0],
                gameSummaryState.AwayPenalties.PeriodTotals[0],
                gameSummaryState.AwayScore.PeriodTotals[0]),
            rules.PeriodRules.PeriodCount >= 2
                ? new(
                    gameSummaryState.HomePenalties.PeriodTotals[1],
                    gameSummaryState.HomeScore.PeriodTotals[1],
                    gameSummaryState.AwayPenalties.PeriodTotals[1],
                    gameSummaryState.AwayScore.PeriodTotals[1])
                : new(0, 0, 0, 0)
        );
    }
}
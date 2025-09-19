using jamster.DataStores;
using jamster.Domain;
using jamster.Reducers;
using jamster.Services;

namespace jamster.Serialization;

public interface IGameSerializer
{
    StatsBook Serialize(GameInfo game);
}

[Singleton]
public class GameSerializer(
        IGameContextFactory contextFactory,
        IIgrfSerializer igrfSerializer,
        IScoreSheetSerializer scoreSheetSerializer,
        IPenaltySheetSerializer penaltySheetSerializer,
        ILineupSheetSerializer lineupSheetSerializer
    ) : IGameSerializer
{
    public StatsBook Serialize(GameInfo game)
    {
        var context = contextFactory.GetGame(game);

        return new StatsBook(
            igrfSerializer.Serialize(context.StateStore),
            scoreSheetSerializer.Serialize(context.StateStore),
            penaltySheetSerializer.Serialize(context.StateStore),
            lineupSheetSerializer.Serialize(context.StateStore)
        );
    }
}

public sealed class TeamSheetsDoNotMatchException : Exception;
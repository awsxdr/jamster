using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Reducers;
using amethyst.Services;

namespace amethyst.Serialization;

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
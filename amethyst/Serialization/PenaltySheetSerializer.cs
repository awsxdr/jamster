using amethyst.Domain;
using amethyst.Reducers;
using amethyst.Services;

namespace amethyst.Serialization;

public interface IPenaltySheetSerializer
{
    PenaltySheetCollection Serialize(IGameStateStore stateStore);
}

[Singleton]
public class PenaltySheetSerializer : IPenaltySheetSerializer
{
    public PenaltySheetCollection Serialize(IGameStateStore stateStore)
    {
        var homePenalties = stateStore.GetKeyedState<PenaltySheetState>(nameof(TeamSide.Home));
        var awayPenalties = stateStore.GetKeyedState<PenaltySheetState>(nameof(TeamSide.Away));

        return new(
            new("", GetPenaltySheetLines(homePenalties, 1), GetPenaltySheetLines(awayPenalties, 1)),
            new("", GetPenaltySheetLines(homePenalties, 2), GetPenaltySheetLines(awayPenalties, 2))
        );
    }

    private static PenaltySheetLine[] GetPenaltySheetLines(PenaltySheetState penaltySheet, int period) =>
        penaltySheet.Lines.Select(line => new PenaltySheetLine(
                line.Penalties.Count(p => p.Period < period),
                line.SkaterNumber,
                line.Penalties.Where(p => p.Period == period).Select(p => new Penalty(p.Jam, p.Code)).ToArray(),
                line.ExpulsionPenalty?.Map(p => new Penalty(p.Jam, p.Code))))
            .ToArray();
}
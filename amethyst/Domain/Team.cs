namespace amethyst.Domain;

public sealed record Team(
    Guid Id,
    Dictionary<string, string> Names,
    Dictionary<string, TeamColor> Colors,
    List<Skater> Roster,
    DateTimeOffset LastUpdateTime
)
{
    public Team() : this(Guid.NewGuid(), [], [], [], DateTimeOffset.MinValue)
    {
    }
}


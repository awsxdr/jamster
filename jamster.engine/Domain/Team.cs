namespace jamster.Domain;

public sealed record Team(
    Guid Id,
    Dictionary<string, string> Names,
    Dictionary<string, TeamColor> Colors,
    Skater[] Roster,
    DateTimeOffset LastUpdateTime
)
{
    public Team() : this(Guid.NewGuid(), [], [], [], DateTimeOffset.MinValue)
    {
    }
}


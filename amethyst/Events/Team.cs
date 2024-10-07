using amethyst.DataStores;
using amethyst.Services;

namespace amethyst.Events;

public sealed class TeamSet(Guid7 id, TeamSetBody body) : Event<TeamSetBody>(id, body);
public sealed record TeamSetBody(Team Team);
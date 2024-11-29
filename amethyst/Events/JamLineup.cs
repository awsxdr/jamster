using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class SkaterOnTrack(Guid7 id, SkaterOnTrackBody body) : Event<SkaterOnTrackBody>(id, body);

public sealed record SkaterOnTrackBody(TeamSide Side, string? SkaterNumber, SkaterPosition Position) : TeamEventBody(Side);

public enum SkaterPosition
{
    Jammer,
    Pivot,
}


using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class SkaterOnTrack(Guid7 id, SkaterOnTrackBody body) : Event<SkaterOnTrackBody>(id, body);

public sealed record SkaterOnTrackBody(TeamSide TeamSide, string? SkaterNumber, SkaterPosition Position) : TeamEventBody(TeamSide);

public enum SkaterPosition
{
    Jammer,
    Pivot,
}


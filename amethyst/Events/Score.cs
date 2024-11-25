using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public abstract record TeamEventBody(TeamSide TeamSide);

public sealed class ScoreModifiedRelative(Guid7 id, ScoreModifiedRelativeBody body) : Event<ScoreModifiedRelativeBody>(id, body);
public sealed record ScoreModifiedRelativeBody(TeamSide TeamSide, int Value) : TeamEventBody(TeamSide);

public sealed class ScoreSet(Guid7 id, ScoreSetBody body) : Event<ScoreSetBody>(id, body);
public sealed record ScoreSetBody(TeamSide TeamSide, int Value) : TeamEventBody(TeamSide);
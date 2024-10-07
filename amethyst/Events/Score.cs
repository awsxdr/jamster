using amethyst.Reducers;
using amethyst.Services;

namespace amethyst.Events;

public abstract record TeamEventBody(Team Team);

public sealed class ScoreModifiedRelative(Guid7 id, ScoreModifiedRelativeBody body) : Event<ScoreModifiedRelativeBody>(id, body);
public sealed record ScoreModifiedRelativeBody(Team Team, int Value) : TeamEventBody(Team);

public sealed class ScoreSet(Guid7 id, ScoreSetBody body) : Event<ScoreSetBody>(id, body);
public sealed record ScoreSetBody(Team Team, int Value) : TeamEventBody(Team);

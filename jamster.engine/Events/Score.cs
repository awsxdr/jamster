using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class ScoreModifiedRelative(Guid7 id, ScoreModifiedRelativeBody body) : Event<ScoreModifiedRelativeBody>(id, body);
public sealed record ScoreModifiedRelativeBody(TeamSide TeamSide, int Value) : TeamEventBody(TeamSide);

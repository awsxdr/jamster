using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class RulesetSet(Guid7 id, RulesetSetBody body) : Event<RulesetSetBody>(id, body);
public sealed record RulesetSetBody(Ruleset Rules);
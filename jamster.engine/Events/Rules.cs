using jamster.Domain;
using jamster.Services;

namespace jamster.Events;

public sealed class RulesetSet(Guid7 id, RulesetSetBody body) : Event<RulesetSetBody>(id, body);
public sealed record RulesetSetBody(Ruleset Rules);
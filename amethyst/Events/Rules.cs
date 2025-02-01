using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class RulesetSet(Guid7 id, RulesetSetBody body) : Event<RulesetSetBody>(id, body);
public sealed record RulesetSetBody(Ruleset Rules);
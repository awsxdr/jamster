using amethyst.Services;

namespace amethyst.Events;

public class ConfigurationSet(Guid7 id, ConfigurationSetBody body) : Event<ConfigurationSetBody>(id, body);
public record ConfigurationSetBody(object Configuration, string ConfigurationTypeName);
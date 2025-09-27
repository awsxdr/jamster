using jamster.engine.Services;

namespace jamster.engine.Events;

public class ConfigurationSet(Guid7 id, ConfigurationSetBody body) : Event<ConfigurationSetBody>(id, body);
public record ConfigurationSetBody(object Configuration, string ConfigurationTypeName);
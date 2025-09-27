namespace jamster.engine.Domain;

public record User(string Name, Dictionary<string, object> Configurations);
namespace jamster.Domain;

public record User(string Name, Dictionary<string, object> Configurations);
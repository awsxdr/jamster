using System.Text.Json;
using System.Text.Json.Nodes;

namespace jamster.engine.Configurations;

public interface IConfigurationFactory
{
    Type ConfigurationType { get; }
    object GetDefaultValue();
    Result<object> ParseConfiguration(string json);
    Result<object> ParseConfiguration(JsonObject json);
}

public interface IConfigurationFactory<out TConfiguration> : IConfigurationFactory
    where TConfiguration : class
{
    Type IConfigurationFactory.ConfigurationType => typeof(TConfiguration);
    object IConfigurationFactory.GetDefaultValue() => GetDefaultValue();

    Result<object> IConfigurationFactory.ParseConfiguration(string json) =>
        JsonSerializer.Deserialize<TConfiguration>(json, Program.JsonSerializerOptions)
            ?.Map(Result.Succeed<object>) 
            ?? Result<object>.Fail<CannotParseConfigurationError>();

    Result<object> IConfigurationFactory.ParseConfiguration(JsonObject json) =>
        json.Deserialize<TConfiguration>(Program.JsonSerializerOptions)
            ?.Map(Result.Succeed<object>)
            ?? Result<object>.Fail<CannotParseConfigurationError>();

    new TConfiguration GetDefaultValue();
}

public sealed class CannotParseConfigurationError : ResultError;

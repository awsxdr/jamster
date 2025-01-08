using System.Text.Json;
using System.Text.Json.Serialization;
using amethyst.Services;
using Func;

namespace amethyst.DataStores;

public interface IConfigurationDataStore : IDataStore
{
    Result<TConfiguration> GetConfiguration<TConfiguration>() where TConfiguration : class;
    Result<object> GetConfiguration(Type configurationType);
    Result SetConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : class;
    Result SetConfiguration(object configuration, Type configurationType);
}

public class ConfigurationDataStore(ConnectionFactory connectionFactory, IDefaultConfigurationFactory defaultConfigurationFactory) 
    : DataStore<ConfigurationDataItem, string>("configurations", 1, i => i.ConfigurationTypeName, connectionFactory)
    , IConfigurationDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = GetSerializerOptions();

    public Result<TConfiguration> GetConfiguration<TConfiguration>() where TConfiguration : class =>
        Get(typeof(TConfiguration).Name).Then(MapConfiguration<TConfiguration>)
        .Else<TConfiguration>(_ => defaultConfigurationFactory.GetDefaultConfiguration<TConfiguration>());

    public Result<object> GetConfiguration(Type configurationType) =>
        Get(configurationType.Name).Then(MapConfiguration)
            .Else<object>(_ => defaultConfigurationFactory.GetDefaultConfiguration(configurationType));

    public Result SetConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : class =>
        SetConfiguration(configuration, typeof(TConfiguration));

    public Result SetConfiguration(object configuration, Type configurationType)
    {
        if (!defaultConfigurationFactory.IsKnownConfigurationType(configurationType))
            return Result.Fail<ConfigurationTypeNotKnownError>();

        Upsert(new ConfigurationDataItem
        {
            ConfigurationTypeName = configurationType.Name,
            ConfigurationJson = JsonSerializer.Serialize(configuration, SerializerOptions)
        });

        return Result.Succeed();
    }

    protected override void ApplyUpgrade(int version)
    {
    }

    private static Result<TConfiguration> MapConfiguration<TConfiguration>(ConfigurationDataItem item) where TConfiguration : class =>
        item.ConfigurationTypeName == typeof(TConfiguration).Name
        ? JsonSerializer.Deserialize<TConfiguration>(item.ConfigurationJson)?.Map(Result.Succeed) ?? Result<TConfiguration>.Fail<ConfigurationJsonInvalidError>()
        : Result<TConfiguration>.Fail<ConfigurationTypesDoNotMatchError>();

    private Result<object> MapConfiguration(ConfigurationDataItem item) =>
        defaultConfigurationFactory.GetKnownConfigurationTypeForKey(item.ConfigurationTypeName)
            .Then(configurationType =>
                JsonSerializer.Deserialize(item.ConfigurationJson, configurationType, SerializerOptions)?.Map(Result.Succeed)
                ?? Result<object>.Fail<ConfigurationJsonInvalidError>());

    private static JsonSerializerOptions GetSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        return serializerOptions;
    }

    public sealed class ConfigurationJsonInvalidError : ResultError;
    public sealed class ConfigurationTypesDoNotMatchError : ResultError;
}

public class ConfigurationDataItem
{
    public string ConfigurationTypeName { get; set; } = "object";
    public string ConfigurationJson { get; set; } = "{}";
}
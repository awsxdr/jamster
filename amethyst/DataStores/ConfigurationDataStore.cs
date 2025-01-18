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

public class ConfigurationDataStore 
    : DataStore
    , IConfigurationDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = GetSerializerOptions();
    private readonly IDataTable<ConfigurationDataItem, string> _configurationTable;
    private readonly IDefaultConfigurationFactory _defaultConfigurationFactory;

    public ConfigurationDataStore(ConnectionFactory connectionFactory, IDataTableFactory dataTableFactory, IDefaultConfigurationFactory defaultConfigurationFactory)
        : base("configurations", 1, connectionFactory, dataTableFactory)
    {
        _defaultConfigurationFactory = defaultConfigurationFactory;
        _configurationTable = GetTable<ConfigurationDataItem, string>(c => c.ConfigurationTypeName);
    }

    public Result<TConfiguration> GetConfiguration<TConfiguration>() where TConfiguration : class =>
        _configurationTable.Get(typeof(TConfiguration).Name).Then(MapConfiguration<TConfiguration>)
        .Else<TConfiguration>(_ => _defaultConfigurationFactory.GetDefaultConfiguration<TConfiguration>());

    public Result<object> GetConfiguration(Type configurationType) =>
        _configurationTable.Get(configurationType.Name).Then(MapConfiguration)
            .Else<object>(_ => _defaultConfigurationFactory.GetDefaultConfiguration(configurationType));

    public Result SetConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : class =>
        SetConfiguration(configuration, typeof(TConfiguration));

    public Result SetConfiguration(object configuration, Type configurationType)
    {
        if (!_defaultConfigurationFactory.IsKnownConfigurationType(configurationType))
            return Result.Fail<ConfigurationTypeNotKnownError>();

        _configurationTable.Upsert(new ConfigurationDataItem
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
        _defaultConfigurationFactory.GetKnownConfigurationTypeForKey(item.ConfigurationTypeName)
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
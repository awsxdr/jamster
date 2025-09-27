using jamster.engine.Configurations;

namespace jamster.engine.Services;

public interface IDefaultConfigurationFactory
{
    bool IsKnownConfigurationType<TConfiguration>() where TConfiguration : class;
    bool IsKnownConfigurationType(Type configurationType);
    Result<Type> GetKnownConfigurationTypeForKey(string key);
    Result<TConfiguration> GetDefaultConfiguration<TConfiguration>() where TConfiguration : class;
    Result<object> GetDefaultConfiguration(Type configurationType);
}

public class DefaultConfigurationFactory : IDefaultConfigurationFactory
{
    private readonly Dictionary<Type, IConfigurationFactory> _configurationFactories;

    public DefaultConfigurationFactory(IEnumerable<IConfigurationFactory> configurationFactories)
    {
        _configurationFactories = configurationFactories
            .ToDictionary(x => x.ConfigurationType, x => x);
    }

    public bool IsKnownConfigurationType<TConfiguration>() where TConfiguration : class =>
        IsKnownConfigurationType(typeof(TConfiguration));

    public bool IsKnownConfigurationType(Type configurationType) =>
        _configurationFactories.ContainsKey(configurationType);

    public Result<Type> GetKnownConfigurationTypeForKey(string key) =>
        _configurationFactories.Keys.SingleOrDefault(k => k.Name == key)?.Map(Result.Succeed) 
        ?? Result<Type>.Fail<ConfigurationKeyNotKnownError>();

    public Result<TConfiguration> GetDefaultConfiguration<TConfiguration>() where TConfiguration : class =>
        IsKnownConfigurationType<TConfiguration>()
            ? Result.Succeed((TConfiguration) _configurationFactories[typeof(TConfiguration)].GetDefaultValue())
            : Result<TConfiguration>.Fail<ConfigurationTypeNotKnownError>();

    public Result<object> GetDefaultConfiguration(Type configurationType) =>
        IsKnownConfigurationType(configurationType)
            ? Result.Succeed(_configurationFactories[configurationType].GetDefaultValue())
            : Result<object>.Fail<ConfigurationTypeNotKnownError>();


}

public sealed class ConfigurationTypeNotKnownError : ResultError;

public sealed class ConfigurationKeyNotKnownError : ResultError;

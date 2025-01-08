namespace amethyst.Configurations;

public interface IConfigurationFactory
{
    Type ConfigurationType { get; }
    object GetDefaultValue();
}

public interface IConfigurationFactory<out TConfiguration> : IConfigurationFactory
    where TConfiguration : class
{
    object IConfigurationFactory.GetDefaultValue() => GetDefaultValue();
    Type IConfigurationFactory.ConfigurationType => typeof(TConfiguration);

    new TConfiguration GetDefaultValue();
}

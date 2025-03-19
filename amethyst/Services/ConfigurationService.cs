using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;

namespace amethyst.Services;

public interface IConfigurationService
{
    event AsyncEventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

    Result<TConfiguration> GetConfiguration<TConfiguration>() where TConfiguration : class;
    Result<object> GetConfiguration(Type configurationType);
    Task<Result<TConfiguration>> GetConfigurationForGame<TConfiguration>(Guid gameId) where TConfiguration : class;
    Task<Result<object>> GetConfigurationForGame(Guid gameId, Type configurationType);

    Task<Result> SetConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : class;
    Task<Result> SetConfiguration(object configuration, Type configurationType);
    Task<Result> SetConfigurationForGame<TConfiguration>(Guid gameId, TConfiguration configuration) where TConfiguration : class;
    Task<Result> SetConfigurationForGame(Guid gameId, object configuration, Type configurationType);

    public sealed class ConfigurationChangedEventArgs(string key, object value) : EventArgs
    {
        public string Key { get; } = key;
        public object Value { get; } = value;
    }
}

[Singleton]
public class ConfigurationService(
    IGameDiscoveryService gameDiscoveryService, 
    IGameContextFactory gameContextFactory,
    IConfigurationDataStore dataStore,
    IEventBus eventBus) 
    : IConfigurationService
{
    public event AsyncEventHandler<IConfigurationService.ConfigurationChangedEventArgs>? ConfigurationChanged;

    public Result<TConfiguration> GetConfiguration<TConfiguration>() where TConfiguration : class =>
        dataStore.GetConfiguration<TConfiguration>();

    public Result<object> GetConfiguration(Type configurationType) =>
        dataStore.GetConfiguration(configurationType);

    public Task<Result<TConfiguration>> GetConfigurationForGame<TConfiguration>(Guid gameId) where TConfiguration : class =>
        GetConfigurationForGame(gameId, typeof(TConfiguration)).ThenMap(x => (TConfiguration)x);

    public Task<Result<object>> GetConfigurationForGame(Guid gameId, Type configurationType) =>
        gameDiscoveryService.GetExistingGame(gameId)
            .ThenMap(gameContextFactory.GetGame)
            .Then(context =>
                context.StateStore.GetState<ConfigurationState>().Configurations.TryGetValue(configurationType, out var value)
                ? Result.Succeed(value)
                : GetConfiguration(configurationType));

    public Task<Result> SetConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : class =>
        dataStore.SetConfiguration(configuration)
            .OnSuccess(() =>
                ConfigurationChanged.InvokeHandlersAsync(this, new(typeof(TConfiguration).Name, configuration))
            );

    public Task<Result> SetConfiguration(object configuration, Type configurationType) =>
        dataStore.SetConfiguration(configuration, configurationType)
            .OnSuccess(() =>
                ConfigurationChanged.InvokeHandlersAsync(this, new(configurationType.Name, configuration))
            );

    public Task<Result> SetConfigurationForGame<TConfiguration>(Guid gameId, TConfiguration configuration) where TConfiguration : class =>
        SetConfigurationForGame(gameId, configuration, typeof(TConfiguration));

    public Task<Result> SetConfigurationForGame(Guid gameId, object configuration, Type configurationType) =>
        gameDiscoveryService.GetExistingGame(gameId)
            .Then(async gameInfo =>
            {
                await eventBus.AddEventAtCurrentTick(gameInfo, new ConfigurationSet(Guid7.Empty, new(configuration, configurationType.Name)));

                return Result.Succeed();
            });
}
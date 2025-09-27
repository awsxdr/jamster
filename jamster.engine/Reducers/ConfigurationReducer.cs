using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public class ConfigurationReducer(ReducerGameContext context, IDefaultConfigurationFactory defaultConfigurationFactory, ILogger<ConfigurationReducer> logger) 
    : Reducer<ConfigurationState>(context)
    , IHandlesEvent<ConfigurationSet>
{
    protected override ConfigurationState DefaultState => new(new Dictionary<Type, object>());

    public IEnumerable<Event> Handle(ConfigurationSet @event)
    {
        logger.LogInformation("Setting {configurationType} configuration for game {gameId}", @event.Body.ConfigurationTypeName, Context.GameInfo.Id);

        defaultConfigurationFactory.GetKnownConfigurationTypeForKey(@event.Body.ConfigurationTypeName)
            .Then(type =>
            {
                var configurations = GetState().Configurations.ToDictionary();
                configurations[type] = @event.Body.Configuration;
                SetState(new(configurations));

                return Result.Succeed();
            })
            .OnError<ResultError>(f => logger.LogWarning("Unable to set configuration. Error: {error}", f.GetType().Name));

        return [];
    }
}

public sealed record ConfigurationState(IReadOnlyDictionary<Type, object> Configurations);
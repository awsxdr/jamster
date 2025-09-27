using System.Collections.Immutable;
using System.Reflection;

using Autofac;

using jamster.engine;
using jamster.engine.DataStores;
using jamster.engine.Reducers;
using jamster.engine.Services;

using NUnit.Framework;

namespace jamster.ui.tests.MockEngine;

[TestFixture]
public abstract class MockedEngineTest : FullEngineTest
{
    protected IGameStateStore StateStore { get; private set; }

    protected long Tick { get; set; }

    protected override void Setup()
    {
        base.Setup();

        var reducerTypes = DependencyInjection.GetReducerTypes();
        var reducers = reducerTypes.Select(r => Mocker.Create(r) as IReducer).OfType<IReducer>().ToImmutableArray();

        StateStore = Create<GameStateStore>();
        StateStore.LoadDefaultStates(reducers);

        GetMock<ISystemTime>()
            .Setup(mock => mock.GetTick())
            .Returns(Tick);
    }

    protected override void RegisterAdditionalDependencies(ContainerBuilder container)
    {
        base.RegisterAdditionalDependencies(container);

        var types =
            DependencyInjection.GetServiceTypes()
                .Where(service => service.GetCustomAttribute<DoNotRegisterAttribute>() is null)
                .Concat(DependencyInjection.GetReducerTypes())
                .Concat([
                    // Data store types
                    typeof(DataTableFactory),
                    typeof(SystemStateDataStore),
                    typeof(SystemStateStore),
                    typeof(TeamsDataStore),
                    typeof(GameDataStoreFactory),
                    typeof(GameDataStore),
                    typeof(ConfigurationDataStore),
                    typeof(UserDataStore),
                ]);

        var interfaces = types
            .SelectMany(type => type.GetInterfaces())
            .Distinct()
            .Where(i => i.Namespace?.StartsWith("jamster.") ?? false);

        foreach (var @interface in interfaces)
            container.Register(_ => GetMock(@interface).Object).As(@interface);

        container.RegisterType<DefaultConfigurationFactory>().AsSelf().AsImplementedInterfaces().SingleInstance();
        //container.RegisterConfigurations();

        container.RegisterType<EventConverter>().AsImplementedInterfaces().SingleInstance();

        container.Register(_ => GetMock<ISystemTime>().Object).As<ISystemTime>().SingleInstance();

        container.RegisterType<GameStateStore>().As<IGameStateStore>().SingleInstance();
        container.RegisterType<ConnectedClientsService>().As<IConnectedClientsService>().SingleInstance();
    }
}
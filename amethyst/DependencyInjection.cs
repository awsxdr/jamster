using System.Reflection;
using amethyst.Configurations;
using amethyst.DataStores;
using amethyst.Hubs;
using amethyst.Reducers;
using amethyst.Services;
using Autofac;
using SQLite;

namespace amethyst;

internal static class DependencyInjection
{
    internal static void RegisterServices(this ContainerBuilder builder)
    {
        var serviceTypes =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == $"{nameof(amethyst)}.{nameof(Services)}")
                .Where(t => t is { IsAbstract: false, IsGenericType: false, IsNested: false })
                .Where(t => !t.IsAssignableTo<MulticastDelegate>())
                .ToArray();

        foreach (var type in serviceTypes)
        {
            var singleton = type.GetCustomAttribute<SingletonAttribute>() is not null;

            var registration = builder.RegisterType(type).AsImplementedInterfaces();

            if (singleton)
                registration.SingleInstance();
        }
    }

    internal static void RegisterReducers(this ContainerBuilder builder)
    {
        var reducerTypes =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && t.IsAssignableTo<IReducer>())
                .ToArray();

        foreach (var reducerType in reducerTypes)
        {
            builder.RegisterType(reducerType).AsSelf();
            builder.Register<ReducerFactory>(context =>
            {
                var localContext = context.Resolve<IComponentContext>();
                return gameContext =>
                    (IReducer)localContext.Resolve(reducerType, new TypedParameter(typeof(ReducerGameContext), gameContext));
            });
        }
    }

    internal static void RegisterConfigurations(this ContainerBuilder builder)
    {
        var configurationFactoryTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(ass => !ass.IsDynamic)
            .SelectMany(ass => ass.GetTypes())
            .Where(t => !t.IsAbstract && t.IsAssignableTo<IConfigurationFactory>())
            .ToArray();

        builder.RegisterTypes(configurationFactoryTypes).AsImplementedInterfaces().SingleInstance();
    }

    internal static void RegisterDataStores(this ContainerBuilder builder)
    {
        builder.RegisterType<DataTableFactory>().As<IDataTableFactory>().SingleInstance();

        builder.RegisterType<SystemStateDataStore>().As<ISystemStateDataStore>().SingleInstance();
        builder.RegisterType<SystemStateStore>().As<ISystemStateStore>().SingleInstance();
        builder.RegisterType<TeamsDataStore>().As<ITeamsDataStore>().SingleInstance();
        builder.RegisterInstance<ConnectionFactory>((connectionString, flags) => new SQLiteConnection(connectionString, flags));
        builder.Register<GameDataStore.Factory>(context =>
        {
            var cachedContext = context.Resolve<IComponentContext>();
            return path => cachedContext.Resolve<Func<string, IGameDataStore>>()(path);
        });
        builder.RegisterType<GameDataStoreFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GameDataStore>().As<IGameDataStore>().ExternallyOwned().InstancePerDependency();
        builder.RegisterType<ConfigurationDataStore>().As<IConfigurationDataStore>().SingleInstance();
        builder.RegisterType<UserDataStore>().As<IUserDataStore>().SingleInstance();
    }

    internal static void RegisterHubNotifiers(this ContainerBuilder builder)
    {
        var hubTypes =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && t.IsAssignableTo<INotifier>())
                .ToArray();

        foreach (var hubType in hubTypes)
        {
            builder.RegisterType(hubType).AsSelf().As<INotifier>().SingleInstance();
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute;
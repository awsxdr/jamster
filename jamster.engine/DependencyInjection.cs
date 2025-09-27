using System.Reflection;

using Autofac;

using jamster.engine.Configurations;
using jamster.engine.DataStores;
using jamster.engine.Hubs;
using jamster.engine.Reducers;
using jamster.engine.Services;

using SQLite;

namespace jamster.engine;

public static class DependencyInjection
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        var serviceTypes =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => 
                    (t.Namespace?.StartsWith($"{typeof(Program).Namespace}.{nameof(Services)}") ?? false)
                    || (t.Namespace?.StartsWith($"{typeof(Program).Namespace}.{nameof(Serialization)}") ?? false)
                )
                .Where(t => t is { IsAbstract: false, IsGenericType: false, IsNested: false, IsClass: true })
                .Where(t => !t.IsAssignableTo<MulticastDelegate>())
                .ToArray();

        foreach (var type in serviceTypes)
        {
            if (type.GetCustomAttribute<DoNotRegisterAttribute>() is not null) continue;

            var singleton = type.GetCustomAttribute<SingletonAttribute>() is not null;

            var registration = builder.RegisterType(type).AsImplementedInterfaces();

            if (singleton)
                registration.SingleInstance();
        }
    }

    public static void RegisterReducers(this ContainerBuilder builder)
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

    public static void RegisterConfigurations(this ContainerBuilder builder)
    {
        var configurationFactoryTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(ass => !ass.IsDynamic)
            .SelectMany(ass => ass.GetTypes())
            .Where(t => !t.IsAbstract && t.IsAssignableTo<IConfigurationFactory>())
            .ToArray();

        builder.RegisterTypes(configurationFactoryTypes).AsImplementedInterfaces().SingleInstance();
    }

    public static void RegisterDataStores(this ContainerBuilder builder)
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

    public static void RegisterHubNotifiers(this ContainerBuilder builder)
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

[AttributeUsage(AttributeTargets.Class)]
public sealed class DoNotRegisterAttribute : Attribute;
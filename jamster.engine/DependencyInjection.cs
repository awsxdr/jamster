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
    public static ICollection<Type> GetServiceTypes() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t =>
                (t.Namespace?.StartsWith($"{typeof(Program).Namespace}.{nameof(Services)}") ?? false)
                || (t.Namespace?.StartsWith($"{typeof(Program).Namespace}.{nameof(Serialization)}") ?? false)
            )
            .Where(t => t is { IsAbstract: false, IsGenericType: false, IsNested: false, IsClass: true })
            .Where(t => !t.IsAssignableTo<MulticastDelegate>())
            .ToArray();

    public static void RegisterServices(this ContainerBuilder builder, CommandLineOptions options)
    {
        var serviceTypes = GetServiceTypes();

        foreach (var type in serviceTypes)
        {
            if (!ShouldRegister(type, options))
                continue;

            var singleton = type.GetCustomAttribute<SingletonAttribute>() is not null;

            var registration = builder.RegisterType(type).AsImplementedInterfaces();

            if (singleton)
                registration.SingleInstance();
        }
    }

    public static ICollection<Type> GetCarolinaCompatibilityTypes() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace?.StartsWith($"{typeof(Program).Namespace}.{nameof(Carolina)}") ?? false)
            .Where(t => t is { IsAbstract: false, IsGenericType: false, IsNested: false, IsClass: true })
            .Where(t => !t.IsAssignableTo<MulticastDelegate>())
            .ToArray();

    public static void RegisterCarolinaCompatibilityLayer(this ContainerBuilder builder, CommandLineOptions options)
    {
        var carolinaTypes = GetCarolinaCompatibilityTypes();

        foreach (var type in carolinaTypes)
        {
            if (!ShouldRegister(type, options))
                continue;

            var singleton = type.GetCustomAttribute<SingletonAttribute>() is not null;

            var registration = builder.RegisterType(type).AsImplementedInterfaces();

            if (singleton)
                registration.SingleInstance();
        }
    }

    public static ICollection<Type> GetReducerTypes() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && t.IsAssignableTo<IReducer>())
            .ToArray();

    public static void RegisterReducers(this ContainerBuilder builder, CommandLineOptions options)
    {
        var reducerTypes = GetReducerTypes();

        foreach (var reducerType in reducerTypes)
        {
            if (!ShouldRegister(reducerType, options))
                continue;

            builder.RegisterType(reducerType).AsSelf();
            builder.Register<ReducerFactory>(context =>
            {
                var localContext = context.Resolve<IComponentContext>();
                return gameContext =>
                    (IReducer)localContext.Resolve(reducerType, new TypedParameter(typeof(ReducerGameContext), gameContext));
            });
        }
    }

    public static ICollection<Type> GetConfigurationFactoryTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(ass => !ass.IsDynamic)
            .SelectMany(ass => ass.GetTypes())
            .Where(t => !t.IsAbstract && t.IsAssignableTo<IConfigurationFactory>())
            .DistinctBy(t => t.FullName)
            .ToArray();

    public static void RegisterConfigurations(this ContainerBuilder builder)
    {
        var configurationFactoryTypes = GetConfigurationFactoryTypes().ToArray();

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

    public static void RegisterHubNotifiers(this ContainerBuilder builder, CommandLineOptions options)
    {
        var hubTypes =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && t.IsAssignableTo<INotifier>())
                .ToArray();

        foreach (var hubType in hubTypes)
        {
            if (!ShouldRegister(hubType, options))
                continue;

            builder.RegisterType(hubType).AsSelf().As<INotifier>().SingleInstance();
        }
    }

    private static bool ShouldRegister(Type type, CommandLineOptions options)
    {
        if (type.GetCustomAttribute<DoNotRegisterAttribute>() is not null) 
            return false;

        var predicateRegister = type.GetCustomAttribute<RegisterOnPredicateAttribute>();

        if (predicateRegister is not null)
        {
            if (!predicateRegister.ShouldRegister(options))
                return false;
        }

        var optionRegister = type.GetCustomAttribute<RegisterOnOptionAttribute>();

        if (optionRegister is not null)
        {
            if (!optionRegister.ShouldRegister(options))
                return false;
        }

        return true;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DoNotRegisterAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterOnPredicateAttribute(Type type, string predicateName) : Attribute
{
    public bool ShouldRegister(CommandLineOptions options) =>
        type.GetMethod(predicateName)?.Invoke(null, [options]) as bool? ??
        throw new ArgumentException("Given method cannot be found or does not return a bool");
}
public sealed class RegisterOnPredicateAttribute<T>(string predicateName) : RegisterOnPredicateAttribute(typeof(T), predicateName);

[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterOnOptionAttribute(string optionName) : Attribute
{
    public bool ShouldRegister(CommandLineOptions options) =>
        typeof(CommandLineOptions).GetProperty(optionName)?.GetValue(options) as bool? ??
        throw new ArgumentException("Given option cannot be found or does not return a bool");
}
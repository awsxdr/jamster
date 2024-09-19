using System.Collections.Immutable;
using System.Text.Json.Serialization;
using amethyst;
using amethyst.DataStores;
using amethyst.Extensions;
using amethyst.Reducers;
using amethyst.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using SQLite;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(container =>
{
    container.RegisterType<TeamsDataStore>().As<ITeamsDataStore>().SingleInstance();
    container.RegisterInstance<ConnectionFactory>((connectionString, flags) => new SQLiteConnection(connectionString, flags));
    container.Register<GameStoreFactory>(context =>
    {
        var cachedContext = context.Resolve<IComponentContext>();
        return path => cachedContext.Resolve<Func<string, IGameDataStore>>()(path);
    });
    container.RegisterType<GameDiscoveryService>().As<IGameDiscoveryService>().SingleInstance();
    container.RegisterType<EventConverter>().As<IEventConverter>().SingleInstance();
    container.RegisterType<GameStateStore>().As<IGameStateStore>();
    container.RegisterType<GameContextFactory>().As<IGameContextFactory>().SingleInstance();
    container.RegisterType<EventBus>().As<IEventBus>().SingleInstance();
    container.RegisterType<GameDataStore>().As<IGameDataStore>().ExternallyOwned().InstancePerDependency();

    var reducerTypes = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic)
        .SelectMany(a => a.GetExportedTypes())
        .Where(t => !t.IsAbstract && t.IsAssignableTo<IReducer>())
        .ToArray();

    container.RegisterTypes(reducerTypes).AsSelf().AsImplementedInterfaces();
}));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var databasesPath = Path.Combine(RunningEnvironment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName));


//builder.Services.AddSingleton<ImmutableList<Func<IReducer>>>(services => reducerTypes.Select(type => services))

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
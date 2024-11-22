using System.Reflection;
using System.Text.Json.Serialization;
using amethyst;
using amethyst.DataStores;
using amethyst.Hubs;
using amethyst.Reducers;
using amethyst.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using SQLite;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(container =>
{
    container.RegisterType<SystemStateDataStore>().As<ISystemStateDataStore>().SingleInstance();
    container.RegisterType<SystemStateStore>().As<ISystemStateStore>().SingleInstance();
    container.RegisterType<TeamsDataStore>().As<ITeamsDataStore>().SingleInstance();
    container.RegisterInstance<ConnectionFactory>((connectionString, flags) => new SQLiteConnection(connectionString, flags));
    container.Register<GameDataStore.Factory>(context =>
    {
        var cachedContext = context.Resolve<IComponentContext>();
        return path => cachedContext.Resolve<Func<string, IGameDataStore>>()(path);
    });
    container.RegisterType<GameDataStoreFactory>().AsImplementedInterfaces().SingleInstance();
    container.RegisterType<GameDiscoveryService>().As<IGameDiscoveryService>().SingleInstance();
    container.RegisterType<EventConverter>().As<IEventConverter>().SingleInstance();
    container.RegisterType<GameStateStore>().As<IGameStateStore>();
    container.RegisterType<GameContextFactory>().As<IGameContextFactory>().SingleInstance();
    container.RegisterType<EventBus>().As<IEventBus>().SingleInstance();
    container.RegisterType<GameDataStore>().As<IGameDataStore>().ExternallyOwned().InstancePerDependency();
    container.RegisterType<GameClock>().As<IGameClock>();
    container.RegisterType<TeamStore>().As<ITeamStore>().SingleInstance();

    container.RegisterType<GameStatesNotifier>().AsSelf().SingleInstance();
    container.RegisterType<GameStoreNotifier>().AsSelf().SingleInstance();
    container.RegisterType<SystemStateNotifier>().AsSelf().SingleInstance();
    container.RegisterType<TeamsNotifier>().AsSelf().SingleInstance();

    container.RegisterType<SystemTime>().AsImplementedInterfaces().SingleInstance();

    var reducerTypes = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic)
        .SelectMany(a => a.GetExportedTypes())
        .Where(t => !t.IsAbstract && t.IsAssignableTo<IReducer>())
        .ToArray();

    foreach (var reducerType in reducerTypes)
    {
        container.RegisterType(reducerType).AsSelf();
        container.Register<ReducerFactory>(context =>
        {
            var localContext = context.Resolve<IComponentContext>();
            return gameContext =>
                (IReducer)localContext.Resolve(reducerType, new TypedParameter(typeof(ReducerGameContext), gameContext));
        });
    }

    //container.RegisterTypes(reducerTypes).As<IReducer>();
}));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "wwwroot");
});

var databasesPath = Path.Combine(RunningEnvironment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policyBuilder =>
{
    policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseExceptionHandler(options =>
{
    options.Run(context =>
    {
        var logger = context.RequestServices.GetService<ILogger<Program>>()!;

        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandler != null)
            logger.LogError(exceptionHandler.Error, "Uncaught exception while handling request");

        return Task.CompletedTask;
    });
});

app.UseAuthorization();

app.MapControllers();

//app.MapFallbackToFile("index.html");
//app.UseDefaultFiles(new DefaultFilesOptions
//{
//    DefaultFileNames = ["index.htm", "index.html"]
//});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "wwwroot")),
    RequestPath = "",

});
app.UseSpaStaticFiles();
app.UseSpa(config =>
{
    config.Options.SourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "wwwroot");
});

void MapHub<THub>(string pattern) where THub : Hub =>
    app
        .MapHub<THub>(pattern)
        .RequireCors(policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });

MapHub<GameStatesHub>("/api/hubs/game/{gameId:guid}");
_ = app.Services.GetService<GameStatesNotifier>();
MapHub<SystemStateHub>("/api/hubs/system");
_ = app.Services.GetService<SystemStateNotifier>();
MapHub<GameStoreHub>("/api/hubs/games");
_ = app.Services.GetService<GameStoreNotifier>();
MapHub<TeamsHub>("/api/hubs/teams");
_ = app.Services.GetService<TeamsNotifier>();

app.Run();

public partial class Program;
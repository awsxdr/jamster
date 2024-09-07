using System.Reflection;
using System.Text.Json.Serialization;
using amethyst;
using amethyst.DataStores;
using amethyst.Services;
using SQLite;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITeamsDataStore, TeamsDataStore>();
builder.Services.AddSingleton<ConnectionFactory>((connectionString, flags) => new SQLiteConnection(connectionString, flags));
builder.Services.AddSingleton<GameStoreFactory>(services => path => new GameDataStore(path, services.GetService<ConnectionFactory>()!, services.GetService<RunningEnvironment>()!));
builder.Services.AddSingleton<IGameDiscoveryService, GameDiscoveryService>();

var environment = new RunningEnvironment(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!);
builder.Services.AddSingleton(environment);

var databasesPath = Path.Combine(environment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDiscoveryService.GamesFolderName));


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

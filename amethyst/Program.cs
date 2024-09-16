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
builder.Services.AddSingleton<GameStoreFactory>(services => path => new GameDataStore(path, services.GetService<ConnectionFactory>()!));
builder.Services.AddSingleton<IGameDiscoveryService, GameDiscoveryService>();
builder.Services.AddSingleton<IEventConverter, EventConverter>();

var databasesPath = Path.Combine(RunningEnvironment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName));


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
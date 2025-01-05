using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using amethyst;
using amethyst.DataStores;
using amethyst.Hubs;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using DotNext.Collections.Generic;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.SignalR;

var parseCommandLineResult = Parser.Default.ParseArguments<CommandLineOptions>(SkipCommandLineParse ? [] : args);

if (parseCommandLineResult.Errors.Any() && !SkipCommandLineParse) return;

var commandLineOptions = parseCommandLineResult.Value;

var builder = WebApplication.CreateBuilder(args);

var hostUrl = GetHostUrl();

builder.WebHost.UseUrls(hostUrl);

builder.Logging
    .SetMinimumLevel(commandLineOptions.LoggingLevel)
    .AddFilter("Microsoft", LogLevel.Warning)
    .AddConsole();

builder.Configuration.AddJsonFile(Path.Combine(RunningEnvironment.RootPath, "appsettings.json"), true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(Path.Combine(RunningEnvironment.RootPath, "appsettings.development.json"), true);
}

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(container =>
{
    container.RegisterServices();
    container.RegisterDataStores();
    container.RegisterReducers();
    container.RegisterHubNotifiers();
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
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(HandleApiDescriptionConflicts);
});

builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "wwwroot");
});

var databasesPath = Path.Combine(RunningEnvironment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName));
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName, GameDataStore.ArchiveFolderName));

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

if(commandLineOptions.UseSsl)
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

app.UseSpaStaticFiles();
app.UseSpa(config =>
{
    config.Options.SourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "wwwroot");
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

if (commandLineOptions.Hostname == "0.0.0.0" || commandLineOptions.Hostname == "::")
{
    Console.WriteLine("Application starting. Use a web browser to go to one of these addresses:");

    var urls = GetLocalAddresses(commandLineOptions.Hostname == "::").Select(GetHostUrl);

    foreach (var url in urls)
    {
        Console.WriteLine($"\t{url}");
    }
}
else
{
    Console.WriteLine($"Application starting. Use a web browser to go to {hostUrl}");
}

app.Run();

IEnumerable<string> GetLocalAddresses(bool includeIpv6)
{
    var hostName = Dns.GetHostName();
    yield return hostName;
    yield return "127.0.0.1";
    yield return "localhost";

    var entries = Dns.GetHostEntry(hostName)
        .AddressList
        .Where(a =>
            a.AddressFamily == AddressFamily.InterNetwork
            || (a.AddressFamily == AddressFamily.InterNetworkV6 && includeIpv6));
    
    foreach (var entry in entries)
    {
        var encodedEntry = entry.ToString().Split('%').First();
        yield return entry.AddressFamily == AddressFamily.InterNetwork ? encodedEntry : $"[{encodedEntry}]";
    }
}

string GetHostUrl(string? hostName = null)
{
    var hostUrlBuilder = new StringBuilder();
    hostUrlBuilder.Append(commandLineOptions.UseSsl ? "https://" : "http://");
    hostUrlBuilder.Append(hostName ?? commandLineOptions.Hostname);
    hostUrlBuilder.Append(':');
    hostUrlBuilder.Append(commandLineOptions.Port);
    hostUrlBuilder.Append('/');

    return hostUrlBuilder.ToString();
}

ApiDescription HandleApiDescriptionConflicts(IEnumerable<ApiDescription> apiDescriptions)
{
    apiDescriptions = apiDescriptions.ToArray();
    var firstDescription = apiDescriptions.First();

    var combinedDescription = new ApiDescription()
    {
        ActionDescriptor = firstDescription.ActionDescriptor,
        GroupName = firstDescription.GroupName,
        HttpMethod = firstDescription.HttpMethod,
        RelativePath = firstDescription.RelativePath,
    };

    combinedDescription.ParameterDescriptions.AddAll(apiDescriptions.SelectMany(d => d.ParameterDescriptions));
    combinedDescription.SupportedRequestFormats.AddAll(apiDescriptions.SelectMany(d => d.SupportedRequestFormats));
    combinedDescription.SupportedResponseTypes.AddAll(
        apiDescriptions.SelectMany(d => d.SupportedResponseTypes)
            .GroupBy(r => r.StatusCode)
            .Select(g =>
            {
                var firstResponse = g.First();

                var responseType = new ApiResponseType
                {
                    IsDefaultResponse = firstResponse.IsDefaultResponse,
                    ModelMetadata = firstResponse.ModelMetadata,
                    StatusCode = g.Key,
                    Type = firstResponse.Type,
                };

                responseType.ApiResponseFormats.AddAll(g.SelectMany(r => r.ApiResponseFormats));

                return responseType;
            })
    );

    foreach (var description in apiDescriptions.Reverse())
    {
        foreach (var property in description.Properties)
        {
            combinedDescription.Properties[property.Key] = property.Value;
        }
    }

    return combinedDescription;

}

public partial class Program
{
    public static bool SkipCommandLineParse { get; set; } = false;
}

public sealed class CommandLineOptions
{
    [Option('p', "port", Required = false, Default = (ushort)8000, HelpText = "Set the host to expose the application on. Value must be between 1 and 65535.")]
    public ushort Port { get; set; }

    [Option('h', "hostname", Required = false, Default = "::", HelpText = "Set the IP address or host name to use to host the application.")]
    public string Hostname { get; set; } = string.Empty;

    [Option('s', "ssl", Required = false, Default = false, HelpText = "Set to 'true' to use secure communications; otherwise 'false'.")]
    public bool UseSsl { get; set; }

    [Option('l', "log", Required = false, Default = LogLevel.Warning, HelpText = "Set the logging level. Options are 'Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical', and 'None'.")]
    public LogLevel LoggingLevel { get; set; }
}

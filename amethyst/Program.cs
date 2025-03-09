using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using amethyst;
using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Hubs;
using amethyst.Services;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using DotNext.Collections.Generic;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.SignalR;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using LogLevel = NLog.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

var parseCommandLineResult = Parser.Default.ParseArguments<CommandLineOptions>(SkipCommandLineParse ? [] : args);

if (parseCommandLineResult.Errors.Any() && !SkipCommandLineParse) return;

var commandLineOptions = parseCommandLineResult.Value;

var builder = WebApplication.CreateBuilder(args);

RunningEnvironment.IsDevelopment = builder.Environment.IsDevelopment();

var logger = LogManager.Setup().LoadConfiguration(GetLoggingConfiguration).GetCurrentClassLogger();

LogManager.Configuration.Variables["rootPath"] = RunningEnvironment.RootPath;

var hostUrl = GetHostUrl();

builder.WebHost.UseUrls(hostUrl);

builder.Configuration.AddJsonFile(Path.Combine(RunningEnvironment.RootPath, "appsettings.json"), true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(Path.Combine(RunningEnvironment.RootPath, "appsettings.development.json"), true);
}

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(container =>
{
    container.RegisterServices();
    container.RegisterConfigurations();
    container.RegisterDataStores();
    container.RegisterReducers();
    container.RegisterHubNotifiers();
}));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Clear();
        options.JsonSerializerOptions.Converters.AddAll(JsonSerializerOptions.Converters);
    });

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions = JsonSerializerOptions;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(HandleApiDescriptionConflicts);
});

builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = Path.Combine(RunningEnvironment.RootPath, "wwwroot");
});

var databasesPath = Path.Combine(RunningEnvironment.RootPath, "db");
Directory.CreateDirectory(databasesPath);
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName));
Directory.CreateDirectory(Path.Combine(databasesPath, GameDataStore.GamesFolderName, GameDataStore.ArchiveFolderName));

builder.Logging.ClearProviders();
builder.Host.UseNLog();

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
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandler != null)
            logger.Error(exceptionHandler.Error, "Uncaught exception while handling request");

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

foreach (var notifier in app.Services.GetService<IEnumerable<INotifier>>() ?? [])
{
    MapNotifier(notifier);
}

if (commandLineOptions.Hostname is "0.0.0.0" or "::")
{
    Console.WriteLine("Application starting. Use a web browser to go to one of these addresses:");

    var urls = GetLocalAddresses(commandLineOptions.Hostname == "::").Select(x => $"{GetHostUrl(x.Address)}{(x.IsLocalOnly ? " (This machine only)" : "")}");

    foreach (var url in urls)
    {
        Console.WriteLine($"\t{url}");
    }
}
else
{
    Console.WriteLine($"Application starting. Use a web browser to go to {hostUrl}");
}

if(!SkipFirstRunSetup)
    await app.Services.GetService<IFirstRunConfigurator>()!.PerformFirstRunTasksIfRequired();

try
{
    app.Run();
}
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AccessDenied)
{
    logger.Error(ex);
    WriteConsoleError("Insufficient privileges to bind to the given address or port");
}
catch (IOException ex) when (ex.InnerException is AddressInUseException)
{
    logger.Error(ex);
    WriteConsoleError("Requested address is already in use");
}
catch (Exception ex)
{
    logger.Error(ex);
    WriteConsoleError("Unexpected error running the application. Check logs for more details.");
}
finally
{
    LogManager.Shutdown();
}

return;

void GetLoggingConfiguration(ISetupLoadConfigurationBuilder config)
{
    config.Configuration.AddTarget("file", new FileTarget
    {
        FileName = Layout.FromString($@"{RunningEnvironment.RootPath}\logs\amethyst-${{shortdate}}.log"),
        Layout = Layout.FromString("${longdate}|${event-properties:item=EventId:whenEmpty=0}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}"),
    });

    config.Configuration.AddTarget("console", new ColoredConsoleTarget
    {
        Layout = Layout.FromString("${level:uppercase=true} ${message} ${exception:format=tostring)"),
        WordHighlightingRules =
        {
            new("FATAL", ConsoleOutputColor.White, ConsoleOutputColor.DarkRed),
            new("ERROR", ConsoleOutputColor.Red, ConsoleOutputColor.Black),
            new("WARN", ConsoleOutputColor.Yellow, ConsoleOutputColor.Black),
            new("INFO", ConsoleOutputColor.Green, ConsoleOutputColor.Black),
            new("DEBUG", ConsoleOutputColor.Blue, ConsoleOutputColor.Black),
            new("TRACE", ConsoleOutputColor.Gray, ConsoleOutputColor.Black),
        }
    });

    config.Configuration.AddRule(LogLevel.Error, LogLevel.Fatal, config.Configuration.FindTargetByName("console"));
    config.Configuration.AddRule(MapLogLevel(commandLineOptions.LoggingLevel), LogLevel.Fatal, config.Configuration.FindTargetByName("console"), "amethyst.*");

    config.Configuration.AddRule(MapLogLevel(commandLineOptions.FileLoggingLevel), LogLevel.Fatal, config.Configuration.FindTargetByName("file"));
}

LogLevel MapLogLevel(MicrosoftLogLevel logLevel) => LogLevel.FromOrdinal((int)logLevel);

MethodInfo GetGenericMapHubMethod() => 
    typeof(Program)
        .GetMethod(nameof(MapHub), BindingFlags.Static | BindingFlags.NonPublic)!;

void MapNotifier(INotifier notifier)
{
    GetGenericMapHubMethod().MakeGenericMethod(notifier.HubType).Invoke(null, [app, notifier.HubAddress]);
}

void WriteConsoleError(string error)
{
    Console.WriteLine();
    Console.BackgroundColor = ConsoleColor.DarkRed;
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"ERROR: {error}");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine();
}

IEnumerable<(string Address, bool IsLocalOnly)> GetLocalAddresses(bool includeIpv6)
{
    var hostName = Dns.GetHostName();
    yield return (hostName, false);
    yield return ("127.0.0.1", true);
    yield return ("localhost", true);

    var entries = Dns.GetHostEntry(hostName)
        .AddressList
        .Where(a =>
            a.AddressFamily == AddressFamily.InterNetwork
            || (a.AddressFamily == AddressFamily.InterNetworkV6 && includeIpv6))
        .OrderBy(x => x.AddressFamily);
    
    foreach (var entry in entries)
    {
        var encodedEntry = entry.ToString().Split('%').First();
        yield return (entry.AddressFamily == AddressFamily.InterNetwork ? encodedEntry : $"[{encodedEntry}]", false);
    }
}

string GetHostUrl(string? hostName = null)
{
    var hostUrlBuilder = new StringBuilder();
    hostUrlBuilder.Append(commandLineOptions.UseSsl ? "https://" : "http://");
    hostUrlBuilder.Append(hostName ?? commandLineOptions.Hostname);
    if (commandLineOptions.Port != 80)
    {
        hostUrlBuilder.Append(':');
        hostUrlBuilder.Append(commandLineOptions.Port);
    }
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
    public static bool SkipCommandLineParse { get; set; }
    public static bool SkipFirstRunSetup { get; set; }
    public static JsonSerializerOptions JsonSerializerOptions { get; } = GetSerializerOptions();

    private static void MapHub<THub>(WebApplication app, string pattern) where THub : Hub =>
        app
            .MapHub<THub>(pattern)
            .RequireCors(policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });

    private static JsonSerializerOptions GetSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new JsonNumberEnumConverter<TimeoutPeriodClockStopBehavior>());
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        return serializerOptions;
    }
}

namespace amethyst
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public sealed class CommandLineOptions
    {
        [Option('p', "port", Required = false, Default = (ushort)8000, HelpText = "Set the host to expose the application on. Value must be between 1 and 65535.")]
        public ushort Port { get; set; }

        [Option('h', "hostname", Required = false, Default = "0.0.0.0", HelpText = "Set the IP address or host name to use to host the application.")]
        public string Hostname { get; set; } = string.Empty;

        [Option('s', "ssl", Required = false, Default = false, HelpText = "Set to 'true' to use secure communications; otherwise 'false'.")]
        public bool UseSsl { get; set; }

        [Option('l', "log", Required = false, Default = MicrosoftLogLevel.Warning, HelpText = "Set the logging level for console output. Options are 'Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical', and 'None'.")]
        public MicrosoftLogLevel LoggingLevel { get; set; }

        [Option("file-log", Required = false, Default = MicrosoftLogLevel.Warning, HelpText = "Set the logging level for file output. Options are 'Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical', and 'None'.")]
        public MicrosoftLogLevel FileLoggingLevel { get; set; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
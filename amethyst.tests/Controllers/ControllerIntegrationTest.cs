using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using amethyst.DataStores;
using amethyst.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace amethyst.tests.Controllers;

[TestFixture]
public abstract class ControllerIntegrationTest
{
    private readonly WebApplicationFactory<Program> _applicationFactory;
    protected HttpClient Client { get; private set; }
    protected TestServer Server => _applicationFactory.Server;
    protected GameDataStoreFactory? GameDataStoreFactory { get; private set; }
    private readonly string _runPath;

    protected JsonSerializerOptions SerializerOptions { get; } = Program.JsonSerializerOptions;

    protected ControllerIntegrationTest()
    {
        var systemTimeMock = new Mock<ISystemTime>();
        systemTimeMock.Setup(mock => mock.UtcNow()).Returns(new DateTimeOffset(2000, 5, 4, 3, 2, 1, TimeSpan.Zero));

        _runPath = Path.Combine(Path.GetTempPath(), $"AmethystControllerTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_runPath);
        RunningEnvironment.RootPath = _runPath;

        Program.SkipCommandLineParse = true;
        Program.SkipFirstRunSetup = true;

        _applicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logOptions =>
            {
                logOptions.AddConsole().SetMinimumLevel(LogLevel.Debug);
            });
        });
        _applicationFactory.Server.PreserveExecutionContext = true;
    }

    [OneTimeSetUp]
    public virtual void OneTimeSetup()
    {
        Client = _applicationFactory.CreateClient();
        Client.GetAsync("/").Wait();

        GameDataStoreFactory = _applicationFactory.Services.GetService(typeof(IGameDataStoreFactory)) as GameDataStoreFactory;
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Client.Dispose();
        _applicationFactory.Dispose();

        GC.Collect();
        Thread.Sleep(500);
        Directory.Delete(_runPath, true);
    }

    [SetUp]
    public virtual void Setup()
    {
    }

    [TearDown]
    public virtual void TearDown()
    {
        CleanDatabase();
    }

    protected Task<HttpResponseMessage> Get(string path) =>
        Time($"GET {path}", () => Client.GetAsync(path));

    protected async Task<TContent?> Get<TContent>(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Get(path);
        response.StatusCode.Should().Be(expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected Task<HttpResponseMessage> Post(string path, HttpContent content) =>
        Time($"POST {path}", () => Client.PostAsync(path, content));

    protected async Task<TContent?> Post<TContent>(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Post(path, JsonContent.Create(content, content.GetType()));
        response.StatusCode.Should().Be(expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected async Task Post(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Post(path, JsonContent.Create(content, content.GetType()));
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    protected Task<HttpResponseMessage> Put(string path, HttpContent content) =>
        Time($"PUT {path}", () => Client.PutAsync(path, content));

    protected async Task<HttpResponseMessage> Put(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Put(path, JsonContent.Create(content, content.GetType()));
        response.StatusCode.Should().Be(expectedStatusCode);
        return response;
    }

    protected async Task<TContent?> Put<TContent>(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Put(path, JsonContent.Create(content, content.GetType()), expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected Task<HttpResponseMessage> Delete(string path) =>
        Time($"DELETE {path}", () => Client.DeleteAsync(path));

    protected async Task Delete(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Delete(path);
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    protected static async Task<TResult> Time<TResult>(string message, Func<Task<TResult>> method)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await method();
        stopwatch.Stop();
        Console.WriteLine($"{message} ({stopwatch.ElapsedMilliseconds}ms)");

        return result;
    }

    protected static TResult Time<TResult>(string message, Func<TResult> method)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = method();
        stopwatch.Stop();
        Console.WriteLine($"{message} ({stopwatch.ElapsedMilliseconds}ms)");

        return result;
    }

    protected async Task<HubConnection> GetHubConnection(string hubPath)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(
                Client.BaseAddress + hubPath,
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                    options.Transports = HttpTransportType.ServerSentEvents;
                })
            .AddJsonProtocol(config =>
            {
                config.PayloadSerializerOptions = Program.JsonSerializerOptions;
            })
            .Build();

        await connection.StartAsync();

        return connection;
    }

    protected async Task Wait(Task task, TimeSpan? delay = null) =>
        await await Task.WhenAny(
            task,
            Task.Run(async () =>
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(4));
                throw new TimeoutException();
            }));

    protected async Task<TResult> Wait<TResult>(Task<TResult> task, TimeSpan? delay = null) =>
        await await Task.WhenAny(
            task,
            Task.Run<TResult>(async () =>
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(4));
                throw new TimeoutException();
            }));
    protected abstract void CleanDatabase();
}
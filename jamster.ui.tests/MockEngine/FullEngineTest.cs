using System.Reflection;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Moq;

using DotNext.Collections.Generic;

using jamster.engine;
using jamster.engine.Domain;
using jamster.engine.Hubs;
using jamster.engine.Reducers;

using Microsoft.AspNetCore.SignalR;

using Moq;

using NUnit.Framework;

namespace jamster.ui.tests.MockEngine;

[TestFixture]
public abstract class FullEngineTest
{
    private Lazy<AutoMock> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected virtual AutoMock Mocker => _mocker.Value;

    protected Mock<TMock> GetMock<TMock>() where TMock : class => Mocker.Mock<TMock>();
    protected Mock GetMock(Type mockedType) =>
        GetType()
                .GetMethod(nameof(GetMock), BindingFlags.NonPublic | BindingFlags.Instance, [])
                ?.MakeGenericMethod(mockedType)
                .Invoke(this, [])
            as Mock
        ?? throw new MockMethodNotFoundException();
    protected TConcrete Create<TConcrete>() where TConcrete : class => Mocker.Create<TConcrete>();

    protected int Port { get; set; } = 8778;
    protected string Url => $"http://127.0.0.1:{Port}/";

    protected IReducer[] Reducers { get; private set; }

    private CancellationTokenSource _cancellationTokenSource;

    [OneTimeSetUp]
    protected virtual void OneTimeSetup()
    {
        RunningEnvironment.RootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "temp");

        var databasePath = Path.Combine(RunningEnvironment.RootPath, "db");
        if (Directory.Exists(databasePath))
            Directory.Delete(databasePath, true);

        Directory.CreateDirectory(Path.Combine(RunningEnvironment.RootPath, "db", "games"));
    }

    [SetUp]
    protected virtual void Setup()
    {
        _mocker = new(AutoMock.GetLoose());
        _cancellationTokenSource = new();

        StartServer();
    }

    [TearDown]
    protected virtual void Teardown() =>
        _cancellationTokenSource.Cancel();

    [OneTimeTearDown]
    protected virtual void OneTimeTearDown()
    {
    }

    private void StartServer()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls(Url);

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(container =>
        {
            container.RegisterServices();
            container.RegisterConfigurations();
            container.RegisterDataStores();
            container.RegisterReducers();
            container.RegisterHubNotifiers();

            container.RegisterInstance(new KeyFrameSettings(true, 5)).AsSelf();

            RegisterAdditionalDependencies(container);
        }));

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(Program).Assembly)
            .AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Clear();
                options.JsonSerializerOptions.Converters.AddAll(Program.JsonSerializerOptions.Converters);
            });

        builder.Services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions = Program.JsonSerializerOptions;
        });

        var uiBuildPath = Path.Combine(".", "wwwroot");
        builder.Services.AddSpaStaticFiles(config =>
        {
            config.RootPath = uiBuildPath;
        });

        var app = builder.Build();
        app.UseCors(policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
        app.MapControllers();
        app.UseSpaStaticFiles();
        app.UseSpa(config =>
        {
            config.Options.SourcePath = uiBuildPath;
            config.Options.DefaultPage = "/index.html";
        });

        var notifiers = app.Services.GetService<IEnumerable<INotifier>>()?.ToArray() ?? [];

        foreach (var notifier in notifiers)
            MapNotifier(app, notifier);

        Reducers = app.Services.GetService<IEnumerable<IReducer>>()?.ToArray() ?? [];

        _ = app.RunAsync(_cancellationTokenSource.Token);
    }

    protected virtual void RegisterAdditionalDependencies(ContainerBuilder container)
    {
    }

    protected string GetUrl(string path) =>
        $"{Url}{path}";

    private static void MapHub<THub>(WebApplication app, string pattern) where THub : Hub =>
        app
            .MapHub<THub>(pattern)
            .RequireCors(policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });

    private static MethodInfo GetGenericMapHubMethod() =>
        typeof(Program)
            .GetMethod(nameof(MapHub), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void MapNotifier(IApplicationBuilder app, INotifier notifier) =>
        GetGenericMapHubMethod().MakeGenericMethod(notifier.HubType).Invoke(null, [app, notifier.HubAddress]);

    private sealed class MockMethodNotFoundException : Exception;
}
using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace amethyst.tests.EventHandling;

[TestFixture]
public abstract class EventBusIntegrationTest
{
    protected MockBehavior MockingBehavior { get; set; } = MockBehavior.Loose;

    private Lazy<AutoMocker> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected AutoMocker Mocker => _mocker.Value;

    protected IEventBus EventBus { get; private set; } = null!;
    protected GameInfo Game { get; private set; } = null!;
    protected IGameStateStore StateStore { get; private set; } = null!;

    private Tick _lastTick = 0;

    protected Mock<TMock> GetMock<TMock>() where TMock : class => Mocker.GetMock<TMock>();
    protected TConcrete Create<TConcrete>() where TConcrete : class => Mocker.CreateInstance<TConcrete>();

    [OneTimeSetUp]
    protected virtual void OneTimeSetup()
    {
    }

    [SetUp]
    protected virtual void Setup()
    {
        _lastTick = 0;
        _mocker = new(() => new AutoMocker(MockingBehavior));

        Game = new GameInfo(Guid.NewGuid(), "Integration test game");
        StateStore = Mocker.CreateInstance<GameStateStore>();
        var context = new GameContext(Game, [], StateStore);

        RegisterLogger(typeof(Services.EventBus));

        Mocker.Use(context);

        var reducerTypes = GetReducerTypes().ToArray();

        var reducerFactories =
            reducerTypes
                .Select(t => (ReducerFactory)(_ => (IReducer) Mocker.Get(t)))
                .ToArray();

        Mocker.Use<IEnumerable<ReducerFactory>>(reducerFactories);

        Mocker.Use<GameStateStoreFactory>(() => StateStore);
        Mocker.Use<Func<IEnumerable<ITickReceiver>, IGameClock>>(_ => Mocker.GetMock<IGameClock>().Object);
        Mocker.Use<GameStoreFactory>(_ => Mocker.GetMock<IGameDataStore>().Object);

        GetMock<IGameDataStore>()
            .Setup(mock => mock.AddEvent(It.IsAny<Event>()))
            .Returns((Event @event) => @event.Id);

        Mocker.Use<IGameContextFactory>(Mocker.CreateInstance<GameContextFactory>());
        EventBus = Mocker.CreateInstance<Services.EventBus>();
        Mocker.Use(EventBus);

        foreach (var reducer in reducerTypes)
        {
            RegisterLogger(reducer);
            Mocker.Use(reducer, Mocker.CreateInstance(reducer));
        }

        Mocker.CreateInstance<GameContextFactory>().GetGame(Game);
    }

    [TearDown]
    protected virtual void Teardown()
    {
    }

    [OneTimeTearDown]
    protected virtual void OneTimeTeardown()
    {
    }

    protected async Task AddEvents(params Event[] events)
    {
        foreach (var @event in events)
        {
            if (@event is ValidateStateFakeEvent validate)
            {
                Tick(@event.Tick);
                validate.ValidateStates(StateStore);
            }
            else
            {
                await EventBus.AddEvent(Game, @event);
            }
        }
    }

    protected TState GetState<TState>() where TState : class =>
        StateStore.GetState<TState>();

    protected void Tick(Func<Tick, Tick> tick) =>
        Tick(tick(_lastTick));

    protected void Tick(Tick tick)
    {
        var tickReceivers =
            GetReducerTypes()
                .Where(t => t.IsAssignableTo(typeof(ITickReceiver)))
                .Select(Mocker.Get)
                .Cast<ITickReceiver>()
                .ToArray();

        foreach (var receiver in tickReceivers)
        {
            receiver.Tick(tick, tick - _lastTick);
        }

        _lastTick = tick;
    }

    private static IEnumerable<Type> GetReducerTypes() =>
        typeof(IReducer).Assembly.GetExportedTypes()
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(IReducer)));

    private void RegisterLogger(Type type)
    {
        var createLoggerMethod =
            typeof(LoggerFactoryExtensions).GetMethods()
                .Single(method => method is {IsGenericMethod: true, Name: nameof(LoggerFactoryExtensions.CreateLogger)});

        Mocker.Use(
            typeof(ILogger<>).MakeGenericType(type),
            createLoggerMethod.MakeGenericMethod(type).Invoke(null, [
            LoggerFactory
                .Create(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Debug);
                })])!);
    }
}
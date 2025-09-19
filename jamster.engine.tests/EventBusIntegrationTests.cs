using System.Collections.Immutable;
using jamster.DataStores;
using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using jamster.Services;
using jamster.engine.tests.EventHandling;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Func;
using Moq;

namespace jamster.engine.tests;

[TestFixture]
public class EventBusIntegrationTests
{
    private AutoMock _mocker;
    private ImmutableList<ReducerFactory> _reducerFactories;
    private IGameStateStore _stateStore;
    private List<Event> _events;

    private Lazy<IEventBus> _subject;
    private IEventBus Subject => _subject.Value;
    private Tick _tick = 0;

    [SetUp]
    public void Setup()
    {
        _events = new List<Event>();
        _mocker = AutoMock.GetLoose(builder =>
        {
            builder.Register(_ => _reducerFactories).As<IEnumerable<ReducerFactory>>().SingleInstance();
            builder.RegisterType<GameStateStore>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GameContextFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<GameClock>().As<IGameClock>().SingleInstance();
            builder.RegisterType<KeyFrameService>().As<IKeyFrameService>();
            builder.RegisterInstance(new KeyFrameSettings(true, 1));
        });

        _mocker.Mock<IGameDataStore>()
            .Setup(mock => mock.GetEvents())
            .Returns(() => _events);

        _mocker.Mock<IGameDataStore>()
            .Setup(mock => mock.AddEvent(It.IsAny<Event>()))
            .Callback((Event @event) => _events.Add(@event))
            .Returns((Event @event) => @event.Id);

        _mocker.Mock<IGameDataStore>()
            .Setup(mock => mock.DeleteEvent(It.IsAny<Guid>()))
            .Callback((Guid eventId) => _events.RemoveAll(e => e.Id == eventId));

        _mocker.Mock<IGameDataStore>()
            .Setup(mock => mock.GetEvent(It.IsAny<Guid>()))
            .Returns((Guid eventId) => _events.SingleOrDefault(e => e.Id == eventId)?.Map(Result.Succeed) ?? Result<Event>.Fail<GameDataStore.EventNotFoundError>());

        _mocker.Mock<IGameDataStoreFactory>()
            .Setup(mock => mock.GetDataStore(It.IsAny<string>()))
            .ReturnsAsync(() => _mocker.Mock<IGameDataStore>().Object);

        _mocker.Mock<ISystemTime>()
            .Setup(mock => mock.GetTick())
            .Returns(() => _tick);

        _stateStore = _mocker.Create<IGameStateStore>();
        _reducerFactories = [];

        _subject = new Lazy<IEventBus>(() => _mocker.Create<IEventBus>());
    }

    [TearDown]
    public void Teardown() =>
        _mocker.Dispose();

    [Test]
    public async Task SingleEventInEmptyGame_SetsExpectedState()
    {
        _reducerFactories = typeof(Reducer<>).Assembly.GetExportedTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IReducer)))
            .Select(type => (ReducerFactory)(_ => (IReducer)_mocker.Create(type)))
            .ToImmutableList();

        await Subject.AddEvent(new(Guid.NewGuid(), "test"), new JamStarted(10000));

        var jamClockState = _stateStore.GetState<JamClockState>();

        jamClockState.IsRunning.Should().BeTrue();
    }

    [Test]
    public async Task EventBus_HandlesMultipleSimultaneousRequests()
    {
        _reducerFactories = [_ => _mocker.Create<ComplexStateTestReducer>(), ctx => new MockPeriodClock(ctx)];

        var gameInfo = new GameInfo(Guid.NewGuid(), "test");
        const int testCount = 10000;

        var eventCreationTasks =
            Enumerable.Range(0, testCount)
                .AsParallel()
                .Select(i => Subject.AddEvent(gameInfo, new TestEvent(i)))
                .ToArray();

        await Task.WhenAll(eventCreationTasks);
        
        _stateStore.GetState<ComplexStateTestReducerState>().Count.Should().Be(testCount);
    }

    [Test]
    public async Task RemoveEvent_RestoresStateAsExpected()
    {
        _reducerFactories = typeof(Reducer<>).Assembly.GetExportedTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IReducer)))
            .Select(type => (ReducerFactory)(_ => (IReducer)_mocker.Create(type)))
            .ToImmutableList();

        var game = new GameInfo(Guid.NewGuid(), "Test game");
        var homeTeam = TestGameEventsSource.HomeTeam;
        var awayTeam = TestGameEventsSource.AwayTeam;

        await AddEvent(new TeamSet(0_000, new(TeamSide.Home, homeTeam)));
        await AddEvent(new TeamSet(0_000, new(TeamSide.Away, awayTeam)));
        await AddEvent(new SkaterOnTrack(10_000, new(TeamSide.Home, homeTeam.Roster[0].Number, SkaterPosition.Jammer)));
        await AddEvent(new SkaterOnTrack(10_500, new(TeamSide.Home, homeTeam.Roster[1].Number, SkaterPosition.Pivot)));
        await AddEvent(new SkaterOnTrack(11_000, new(TeamSide.Away, awayTeam.Roster[0].Number, SkaterPosition.Jammer)));
        await AddEvent(new SkaterOnTrack(11_500, new(TeamSide.Away, awayTeam.Roster[1].Number, SkaterPosition.Pivot)));
        await AddEvent(new JamStarted(40_000));
        await AddEvent(new LeadMarked(50_000, new(TeamSide.Home, true)));
        await AddEvent(new ScoreModifiedRelative(60_000, new(TeamSide.Home, 4)));
        await AddEvent(new InitialTripCompleted(61_000, new(TeamSide.Away, true)));
        await AddEvent(new CallMarked(70_000, new(TeamSide.Home, true)));
        await AddEvent(new ScoreModifiedRelative(72_000, new(TeamSide.Home, 2)));
        await AddEvent(new TimeoutStarted(97_000));
        await AddEvent(new TimeoutTypeSet(98000, new(TimeoutType.Official, null)));

        var gameStageState = _stateStore.GetState<GameStageState>();

        _tick = 110_000;
        var jamStartedEvent = await AddEvent(new JamStarted(110_000));
        Thread.Sleep(100);

        _tick = 111_000;
        var jamEndedEvent = await AddEvent(new JamEnded(111_000));
        Thread.Sleep(100);

        await Subject.RemoveEvent(game, jamEndedEvent.Id);
        Thread.Sleep(100);
        await Subject.RemoveEvent(game, jamStartedEvent.Id);
        Thread.Sleep(100);

        _tick = 112_000;
        var gameStageStateAfterUndo = _stateStore.GetState<GameStageState>();

        gameStageStateAfterUndo.Should().BeEquivalentTo(gameStageState);

        return;

        Task<Event> AddEvent<TEvent>(TEvent @event) where TEvent : Event =>
            Subject.AddEvent(game, @event);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class ComplexStateTestReducer(ReducerGameContext context) 
        : Reducer<ComplexStateTestReducerState>(context)
        , IHandlesEvent<TestEvent>
        , IHandlesEvent<TestIncremented>
    {
        protected override ComplexStateTestReducerState DefaultState => new(0);

        public IEnumerable<Event> Handle(TestEvent @event) =>
            [new TestIncremented(@event.Tick, new(GetState().Count + 1))];

        public IEnumerable<Event> Handle(TestIncremented @event)
        {
            SetState(new(@event.Body.Value));

            return [];
        }
    }

    private sealed record ComplexStateTestReducerState(int Count);

    private sealed class TestEvent(Guid7 id) : Event(id);

    private sealed class TestIncremented(Guid7 id, TestIncrementedBody body) : Event<TestIncrementedBody>(id, body);
    private sealed record TestIncrementedBody(int Value);

    private sealed class MockPeriodClock(ReducerGameContext context) : Reducer<PeriodClockState>(context)
    {
        protected override PeriodClockState DefaultState => new (false, false, 0, 0, 0);
    }
}
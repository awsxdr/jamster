using System.Collections.Immutable;
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;

namespace amethyst.tests;

[TestFixture]
public class EventBusIntegrationTests
{
    private AutoMock _mocker;
    private ImmutableList<ReducerFactory> _reducerFactories;
    private IGameStateStore _stateStore;

    private Lazy<IEventBus> _subject;
    private IEventBus Subject => _subject.Value;

    [SetUp]
    public void Setup()
    {
        _mocker = AutoMock.GetLoose(builder =>
        {
            builder.Register(_ => _reducerFactories).As<IEnumerable<ReducerFactory>>().SingleInstance();
            builder.RegisterType<GameStateStore>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GameContextFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventBus>().As<IEventBus>().SingleInstance();
        });

        _stateStore = _mocker.Create<IGameStateStore>();
        _reducerFactories = [];

        _subject = new Lazy<IEventBus>(() => _mocker.Create<IEventBus>());
    }

    [TearDown]
    public void Teardown()
    {
        _mocker.Dispose();
    }

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
        _reducerFactories = [_ => _mocker.Create<ComplexStateTestReducer>()];

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

    private sealed class ComplexStateTestReducer(GameContext context, IEventBus eventBus) 
        : Reducer<ComplexStateTestReducerState>(context)
        , IHandlesEventAsync<TestEvent>
        , IHandlesEvent<TestIncremented>
    {
        protected override ComplexStateTestReducerState DefaultState => new(0);

        public async Task HandleAsync(TestEvent @event)
        {
            await eventBus.AddEvent(Context.GameInfo, new TestIncremented(@event.Tick, new(GetState().Count + 1)));
        }

        public void Handle(TestIncremented @event)
        {
            SetState(new(@event.Body.Value));
        }
    }

    private sealed record ComplexStateTestReducerState(int Count);

    private sealed class TestEvent(Guid7 id) : Event(id);

    private sealed class TestIncremented(Guid7 id, TestIncrementedBody body) : Event<TestIncrementedBody>(id, body);
    private sealed record TestIncrementedBody(int Value);
}
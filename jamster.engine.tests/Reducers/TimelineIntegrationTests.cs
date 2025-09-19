using jamster.DataStores;
using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using jamster.Services;
using Autofac;
using FluentAssertions;
using Moq;

namespace jamster.engine.tests.Reducers;

public class TimelineIntegrationTests : IntegrationTest<Timeline>
{
    private Tick Tick { get; set; } = 0;

    private readonly List<Event> _events = new();
    private GameInfo _game;

    protected override void ConfigureDependencies(ContainerBuilder builder)
    {
        builder.RegisterInstance(new KeyFrameSettings(false, 0));

        builder.RegisterServices();
        builder.RegisterConfigurations();
        builder.RegisterReducers();

        var systemTimeMock = new Mock<ISystemTime>();
        systemTimeMock
            .Setup(mock => mock.GetTick())
            .Returns(() => Tick);
        builder.RegisterInstance(systemTimeMock.Object).As<ISystemTime>();

        //builder.RegisterInstance(() => Resolve<IGameStateStore>()).As<GameStateStoreFactory>();
    }

    protected override void Setup()
    {
        base.Setup();

        Tick = 0;

        _events.Clear();
        _game = new();

        GetMock<IGameDataStore>()
            .Setup(mock => mock.GetEvents())
            .Returns(_events);

        GetMock<IGameDataStore>()
            .Setup(mock => mock.AddEvent(It.IsAny<Event>()))
            .Callback((Event @event) => _events.Add(@event));

        GetMock<IGameDataStoreFactory>()
            .Setup(mock => mock.GetDataStore(It.IsAny<string>()))
            .ReturnsAsync(() => GetMock<IGameDataStore>().Object);

        Resolve<IGameContextFactory>().GetGame(new GameInfo());
    }

    [Test]
    public async Task Timeline_UpdatesWithLatestState()
    {
        var eventBus = Resolve<IEventBus>();

        Tick = Tick.FromSeconds(30);

        await eventBus.AddEvent(_game, new JamStarted(0));
        ValidateStages([Stage.BeforeGame, Stage.Jam]);

        Tick = Tick.FromSeconds(110);
        
        await eventBus.AddEvent(_game, new CallMarked(100_000, new(TeamSide.Home, true)));
        ValidateStages([Stage.BeforeGame, Stage.Jam, Stage.Lineup]);

    }

    private void ValidateStages(IEnumerable<Stage> stages)
    {
        var gameStateStore = Resolve<IGameContextFactory>().GetGame(_game).StateStore;

        var timeline = gameStateStore.GetState<TimelineState>();

        ((Stage[])[..timeline.PreviousStages.Select(s => s.Stage), timeline.CurrentStage]).Should().BeEquivalentTo(stages);
    }
}
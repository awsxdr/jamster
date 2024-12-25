using System.Collections.Immutable;
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Func;
using Moq;

namespace amethyst.tests.Services;

public class EventBusUnitTests : UnitTest<EventBus>
{
    private GameInfo _game;
    private Guid _persistedEventId;

    protected override void Setup()
    {
        base.Setup();

        _game = new GameInfo(Guid.NewGuid(), "Test Game");

        GetMock<IGameContextFactory>()
            .Setup(mock => mock.GetGame(_game))
            .Returns(() => new GameContext(
                _game,
                [],
                GetMock<IGameStateStore>().Object,
                GetMock<IGameClock>().Object));

        GetMock<IGameDataStoreFactory>()
            .Setup(mock => mock.GetDataStore($"TestGame_{_game.Id}"))
            .ReturnsAsync(() => GetMock<IGameDataStore>().Object);

        _persistedEventId = Guid.NewGuid();
        GetMock<IGameDataStore>()
            .Setup(mock => mock.AddEvent(It.IsAny<Event>()))
            .Returns((Event @event) => @event.Id == Guid.Empty ? _persistedEventId : @event.Id);
    }

    [Test]
    public async Task AddEvent_AppliesEventToStateStore()
    {
        var @event = new TestEvent(Guid.Empty, new TestEventBody {Value = "This is a test"});

        await Subject.AddEvent(_game, @event);

        GetMock<IGameStateStore>()
            .Verify(mock => mock.ApplyEvents(It.Is<IImmutableList<IReducer>>(l => !l.Any()), It.Is<Event[]>(e => e.Single().Equals(@event))), Times.Once);
    }

    [Test]
    public async Task AddEvent_PersistsEventToDatabase()
    {
        var @event = new TestEvent(Guid.Empty, new TestEventBody { Value = "This is a test" });

        await Subject.AddEvent(_game, @event);

        GetMock<IGameDataStore>()
            .Verify(mock => mock.AddEvent(@event), Times.Once);
    }

    [Test]
    public async Task AddEvent_SetsEventIdToPersistedId()
    {
        var @event = new TestEvent(Guid.Empty, new TestEventBody { Value = "This is a test" });

        var result = await Subject.AddEvent(_game, @event);

        result.Id.Should().Be(_persistedEventId);
    }

    [TestCase(1750, 1000, 2000)]
    [TestCase(1250, 1000, 1000)]
    [TestCase(1500, 1000, 1000)]
    [TestCase(1501, 1000, 2000)]
    [TestCase(1234, 1234, 1234)]
    public async Task AddEvent_WithPeriodClockAlignedEvent_AlignsTickToPeriodClockSeconds(long currentTick, long periodStartTick, long expectedTick)
    {
        GetMock<ISystemTime>()
            .Setup(mock => mock.GetTick())
            .Returns(currentTick);

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<PeriodClockState>())
            .Returns(new PeriodClockState(true, false, periodStartTick, 0, currentTick - periodStartTick, 0));

        var @event = new TestAlignedEvent(Guid7.FromTick(currentTick));

        var result = await Subject.AddEvent(_game, @event);

        result.Tick.Should().Be(expectedTick);
    }

    [Test]
    public async Task RemoveEvent_WhenEventExists_DeletesEventFromDatabase()
    {
        var eventId = Guid.NewGuid();

        GetMock<IGameDataStore>()
            .Setup(mock => mock.GetEvent(eventId))
            .Returns(Result.Succeed<Event>(new TestAlignedEvent(eventId)));

        var result = await Subject.RemoveEvent(_game, eventId);

        result.Should().BeSuccess();

        GetMock<IGameDataStore>()
            .Verify(mock => mock.DeleteEvent(eventId), Times.Once);
    }

    [Test]
    public async Task RemoveEvent_WhenEventExists_ReloadsGame()
    {
        var eventId = Guid.NewGuid();

        GetMock<IGameDataStore>()
            .Setup(mock => mock.GetEvent(eventId))
            .Returns(Result.Succeed<Event>(new TestAlignedEvent(eventId)));

        await Subject.RemoveEvent(_game, eventId);

        GetMock<IGameContextFactory>()
            .Verify(mock => mock.ReloadGame(_game), Times.Once);
    }

    [Test]
    public async Task RemoveEvent_WhenEventDoesNotExist_ReturnsFailure()
    {
        var eventId = Guid.NewGuid();

        GetMock<IGameDataStore>()
            .Setup(mock => mock.GetEvent(eventId))
            .Returns(Result<Event>.Fail<GameDataStore.EventNotFoundError>());

        var result = await Subject.RemoveEvent(_game, eventId);

        result.Should().BeFailure();
    }

    [Test]
    public async Task RemoveEvent_WhenExceptionThrown_ReturnsEventDeletionFailedError()
    {
        GetMock<IGameDataStore>()
            .Setup(mock => mock.GetEvent(It.IsAny<Guid>()))
            .Throws<Exception>();

        var result = await Subject.RemoveEvent(_game, Guid.NewGuid());

        result.Should().BeFailure<EventBus.EventDeletionFailedError>();
    }
}
using System.Collections.Immutable;
using amethyst.DataStores;
using amethyst.Events;
using amethyst.Reducers;
using amethyst.Services;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace amethyst.tests;

[TestFixture]
public class EventBusIntegrationTests
{
    [Test]
    public async Task SingleEventInEmptyGame_SetsExpectedState()
    {
        var mocker = new AutoMocker(MockBehavior.Loose);

        var reducerFactories = typeof(Reducer<>).Assembly.GetExportedTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IReducer)))
            .Select(type => (ReducerFactory)(_ => (IReducer)mocker.CreateInstance(type)))
            .ToImmutableList();

        mocker.Use<IEnumerable<ReducerFactory>>(reducerFactories);

        var dataStoreMock = mocker.GetMock<IGameDataStore>();
        mocker.Use((GameStoreFactory)(_ => dataStoreMock.Object));

        var stateStore = mocker.CreateInstance<GameStateStore>();
        mocker.Use<IGameStateStore>(stateStore);

        mocker.Use<GameStateStoreFactory>(() => mocker.Get<IGameStateStore>());

        var gameContextFactory = mocker.CreateInstance<GameContextFactory>();
        mocker.Use<IGameContextFactory>(gameContextFactory);

        var subject = mocker.CreateInstance<Services.EventBus>();

        await subject.AddEvent(new(Guid.NewGuid(), "test"), new JamStarted(10000));

        var jamClockState = stateStore.GetState<JamClockState>();

        jamClockState.IsRunning.Should().BeTrue();
    }
}
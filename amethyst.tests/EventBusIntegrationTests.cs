namespace amethyst.tests;

using System.Collections.Immutable;
using amethyst.Reducers;
using Castle.Core.Logging;
using DataStores;
using Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Services;

[TestFixture]
public class EventBusIntegrationTests
{
    [Test]
    public async Task Test()
    {
        var mocker = new AutoMocker(MockBehavior.Loose);

        var reducers = typeof(Reducer<>).Assembly.GetExportedTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IReducer)))
            .Select(type => (IReducer)mocker.CreateInstance(type))
            .ToImmutableList();

        mocker.Use<IImmutableList<IReducer>>(reducers);

        var stateStore = mocker.CreateInstance<GameStateStore>();
        mocker.Use(stateStore);

        var stateStoreFactoryMock = mocker.GetMock<IGameStateStoreFactory>();
        stateStoreFactoryMock.Setup(m => m.GetGame(It.IsAny<GameInfo>())).Returns(stateStore);
        mocker.Use(stateStoreFactoryMock.Object);

        var dataStoreMock = mocker.GetMock<IGameDataStore>();
        mocker.Use((GameStoreFactory)(_ => dataStoreMock.Object));

        var subject = mocker.CreateInstance<EventBus>();

        await subject.AddEvent(new(Guid.NewGuid(), "test"), new JamStarted(10000));

        var jamClockState = stateStore.GetState<JamClockState>();

        jamClockState.IsRunning.Should().BeTrue();
    }
}
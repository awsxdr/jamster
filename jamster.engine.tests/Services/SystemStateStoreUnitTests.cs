﻿using FluentAssertions;
using Func;

using jamster.engine.DataStores;
using jamster.engine.Services;

using Moq;

namespace jamster.engine.tests.Services;

public class SystemStateStoreUnitTests : UnitTest<SystemStateStore>
{
    protected override void Setup()
    {
        base.Setup();

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(It.IsAny<Guid>()))
            .ReturnsAsync(Result<GameInfo>.Fail<GameFileNotFoundForIdError>());
    }

    [Test]
    public async Task SetCurrentGame_WhenGameExists_InvokesCurrentGameChangedEvent()
    {
        Guid? passedGameId = null;
        var eventInvoked = false;

        Subject.CurrentGameChanged += (s, e) =>
        {
            eventInvoked = true;
            passedGameId = e.Value;

            return Task.CompletedTask;
        };

        var gameId = Guid.NewGuid();

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result.Succeed(new GameInfo(gameId, "Test Game")));

        GetMock<ISystemStateDataStore>()
            .Setup(mock => mock.SetCurrentGame(gameId))
            .Returns(Result.Succeed());

        await Subject.SetCurrentGame(gameId);

        eventInvoked.Should().BeTrue();
        passedGameId.Should().Be(gameId);
    }

    [Test]
    public async Task SetCurrentGame_WhenGameDoesNotExist_ReturnsNotFoundError()
    {
        var result = await Subject.SetCurrentGame(Guid.NewGuid());

        result.Should().BeFailure<GameFileNotFoundForIdError>();
    }

    [Test]
    public async Task SetCurrentGame_WhenGameExists_ReturnsSuccess()
    {
        var gameId = Guid.NewGuid();
        var gameInfo = new GameInfo(gameId, "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result.Succeed(gameInfo));

        var result = await Subject.SetCurrentGame(gameId);
        result.Should().BeSuccess<GameInfo>().Which.Value.Should().Be(gameInfo);
    }
}
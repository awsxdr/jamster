using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;
using FluentAssertions;
using Func;
using Moq;

namespace amethyst.tests.Services;

public class SystemStateStoreUnitTests : UnitTest<SystemStateStore>
{
    protected override void Setup()
    {
        base.Setup();

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(It.IsAny<Guid>()))
            .Returns(Result<GameInfo>.Fail<GameFileNotFoundForIdError>());
    }

    [Test]
    public void SetCurrentGame_WhenGameExists_InvokesCurrentGameChangedEvent()
    {
        Guid? passedGameId = null;
        var eventInvoked = false;

        Subject.CurrentGameChanged += (s, e) =>
        {
            eventInvoked = true;
            passedGameId = e.Value;
        };

        var gameId = Guid.NewGuid();

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .Returns(Result.Succeed(new GameInfo(gameId, "Test Game")));

        Subject.SetCurrentGame(gameId);

        eventInvoked.Should().BeTrue();
        passedGameId.Should().Be(gameId);
    }

    [Test]
    public void SetCurrentGame_WhenGameDoesNotExist_ReturnsNotFoundError()
    {
        var result = Subject.SetCurrentGame(Guid.NewGuid());

        result.Should().BeFailure<GameFileNotFoundForIdError>();
    }

    [Test]
    public void SetCurrentGame_WhenGameExists_ReturnsSuccess()
    {
        var gameId = Guid.NewGuid();
        var gameInfo = new GameInfo(gameId, "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .Returns(Result.Succeed(gameInfo));

        var result = Subject.SetCurrentGame(gameId);
        result.Should().BeSuccess<GameInfo>().Which.Value.Should().Be(gameInfo);
    }
}
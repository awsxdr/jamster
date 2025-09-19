using jamster.DataStores;
using jamster.Events;
using jamster.Reducers;
using jamster.Services;
using FluentAssertions;
using Func;
using Moq;

namespace jamster.engine.tests.Services;

public class ConfigurationServiceUnitTests : UnitTest<ConfigurationService>
{
    [Test]
    public void GetConfiguration_WithGenericParameter_RetrievesConfigurationFromDataStore()
    {
        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.GetConfiguration<TestConfiguration>())
            .Returns(Result.Succeed(new TestConfiguration { Test = "Hello" }));

        var result = Subject.GetConfiguration<TestConfiguration>();

        result.Should().BeSuccess<TestConfiguration>()
            .Which.Value.Test.Should().Be("Hello");
    }

    [Test]
    public void GetConfiguration_WithTypeArgument_RetrievesConfigurationFromDataStore()
    {
        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.GetConfiguration(typeof(TestConfiguration)))
            .Returns(Result.Succeed<object>(new TestConfiguration { Test = "Hello" }));

        var result = Subject.GetConfiguration(typeof(TestConfiguration));

        result.Should().BeSuccess<object>()
            .Which.Value.Should().BeAssignableTo<TestConfiguration>()
            .Which.Test.Should().Be("Hello");
    }

    [Test]
    public async Task GetConfigurationForGame_WhenGameExists_AndConfigurationNotSetForGame_RetrievesConfigurationFromDataStore([Values] bool useGenericMethod)
    {
        var gameId = Guid.NewGuid();
        var gameInfo = new GameInfo(gameId, "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result.Succeed(gameInfo));

        GetMock<IGameContextFactory>()
            .Setup(mock => mock.GetGame(gameInfo))
            .Returns(() => new GameContext(gameInfo, [], GetMock<IGameStateStore>().Object, GetMock<IGameClock>().Object, GetMock<IKeyFrameService>().Object));

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<ConfigurationState>())
            .Returns(new ConfigurationState(new Dictionary<Type, object>()));

        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.GetConfiguration(typeof(TestConfiguration)))
            .Returns(Result.Succeed<object>(new TestConfiguration { Test = "Hello" }));

        if (useGenericMethod)
        {
            var result = await Subject.GetConfigurationForGame<TestConfiguration>(gameId);

            result.Should().BeSuccess<TestConfiguration>()
                .Which.Value.Test.Should().Be("Hello");
        }
        else
        {
            var result = await Subject.GetConfigurationForGame(gameId, typeof(TestConfiguration));

            result.Should().BeSuccess<object>()
                .Which.Value.Should().BeAssignableTo<TestConfiguration>()
                .Which.Test.Should().Be("Hello");
        }
    }

    [Test]
    public async Task GetConfigurationForGame_WhenGameExists_AndConfigurationSetForGame_RetrievesConfigurationFromStateStore([Values] bool useGenericMethod)
    {
        var gameId = Guid.NewGuid();
        var gameInfo = new GameInfo(gameId, "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result.Succeed(gameInfo));

        GetMock<IGameContextFactory>()
            .Setup(mock => mock.GetGame(gameInfo))
            .Returns(() => new GameContext(gameInfo, [], GetMock<IGameStateStore>().Object, GetMock<IGameClock>().Object, GetMock<IKeyFrameService>().Object));

        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetState<ConfigurationState>())
            .Returns(new ConfigurationState(new Dictionary<Type, object>
            {
                [typeof(TestConfiguration)] = new TestConfiguration { Test = "State" },
            }));

        if (useGenericMethod)
        {
            var result = await Subject.GetConfigurationForGame<TestConfiguration>(gameId);

            result.Should().BeSuccess<TestConfiguration>()
                .Which.Value.Test.Should().Be("State");
        }
        else
        {
            var result = await Subject.GetConfigurationForGame(gameId, typeof(TestConfiguration));

            result.Should().BeSuccess<object>()
                .Which.Value.Should().BeAssignableTo<TestConfiguration>()
                .Which.Test.Should().Be("State");
        }
    }

    [Test]
    public async Task GetConfigurationForGame_WhenGameDoesNotExist_ReturnsFailure([Values] bool useGenericMethod)
    {
        var gameId = Guid.NewGuid();

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result<GameInfo>.Fail<GameFileNotFoundForIdError>());

        var result = useGenericMethod
            ? (Result)await Subject.GetConfigurationForGame<TestConfiguration>(gameId)
            : await Subject.GetConfigurationForGame(gameId, typeof(TestConfiguration));

        result.Should().BeFailure<GameFileNotFoundForIdError>();
    }

    [Test]
    public async Task SetConfiguration_WithGenericParameter_SetsConfigurationInDataStore()
    {
        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.SetConfiguration(It.Is<TestConfiguration>(c => c.Test == "Hello")))
            .Returns(Result.Succeed);

        var result = await Subject.SetConfiguration(new TestConfiguration { Test = "Hello" });

        result.Should().BeSuccess();

        GetMock<IConfigurationDataStore>()
            .Verify(mock => mock.SetConfiguration(It.Is<TestConfiguration>(c => c.Test == "Hello")), Times.Once);
    }

    [Test]
    public async Task SetConfiguration_WithTypeArgument_SetsConfigurationInDataStore()
    {
        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.SetConfiguration(It.Is<TestConfiguration>(c => c.Test == "Hello"), typeof(TestConfiguration)))
            .Returns(Result.Succeed);

        var result = await Subject.SetConfiguration(new TestConfiguration { Test = "Hello" }, typeof(TestConfiguration));

        result.Should().BeSuccess();
        
        GetMock<IConfigurationDataStore>()
            .Verify(mock => mock.SetConfiguration(It.Is<TestConfiguration>(c => c.Test == "Hello"), typeof(TestConfiguration)), Times.Once);
    }

    [Test]
    public async Task SetConfigurationForGame_WhenGameExists_AddsConfigurationSetEventToEventBus([Values] bool useGenericMethod)
    {
        var gameId = Guid.NewGuid();
        var gameInfo = new GameInfo(gameId, "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(gameId))
            .ReturnsAsync(Result.Succeed(gameInfo));

        GetMock<IGameContextFactory>()
            .Setup(mock => mock.GetGame(gameInfo))
            .Returns(() => new GameContext(gameInfo, [], GetMock<IGameStateStore>().Object, GetMock<IGameClock>().Object, GetMock<IKeyFrameService>().Object));

        var configuration = new TestConfiguration { Test = "State" };

        var result = useGenericMethod
            ? await Subject.SetConfigurationForGame(gameId, configuration)
            : await Subject.SetConfigurationForGame(gameId, configuration, typeof(TestConfiguration));

        result.Should().BeSuccess();

        GetMock<IEventBus>()
            .Verify(
                mock => mock.AddEventAtCurrentTick(gameInfo, It.Is<ConfigurationSet>(e => e.Body == new ConfigurationSetBody(configuration, nameof(TestConfiguration)))),
                Times.Once);
    }

    [Test]
    public async Task SetConfiguration_RaisesConfigurationChangedEvent([Values] bool useGenericMethod)
    {
        var configuration = new TestConfiguration { Test = "State" };

        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.SetConfiguration(configuration))
            .Returns(Result.Succeed());

        GetMock<IConfigurationDataStore>()
            .Setup(mock => mock.SetConfiguration(configuration, typeof(TestConfiguration)))
            .Returns(Result.Succeed());

        IConfigurationService.ConfigurationChangedEventArgs? passedArgs = null;
        Subject.ConfigurationChanged += (_, e) =>
        {
            passedArgs = e;
            return Task.CompletedTask;
        };

        var result = useGenericMethod
            ? await Subject.SetConfiguration(configuration)
            : await Subject.SetConfiguration(configuration, typeof(TestConfiguration));

        result.Should().BeSuccess();

        passedArgs.Should().NotBeNull();
        passedArgs!.Key.Should().Be(nameof(TestConfiguration));
        passedArgs!.Value.Should().Be(configuration);
    }

    private class TestConfiguration
    {
        public string Test { get; set; } = "Test";
    }
}
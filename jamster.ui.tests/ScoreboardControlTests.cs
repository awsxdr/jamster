using System.Collections.Immutable;

using FluentAssertions;

using Func;

using jamster.engine.Configurations;
using jamster.engine.DataStores;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;
using jamster.ui.tests.MockEngine;

using Moq;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace jamster.ui.tests;

public class ScoreboardControlTests : MockedEngineTest
{
    private IWebDriver _driver;
    private GameInfo _game;

    protected override void Setup()
    {
        base.Setup();

        _game = new GameInfo(Guid.NewGuid(), "Test Game");

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetGames())
            .ReturnsAsync([_game]);

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GetExistingGame(_game.Id))
            .ReturnsAsync(Result.Succeed(_game));

        GetMock<IGameDiscoveryService>()
            .Setup(mock => mock.GameExists(_game.Id))
            .Returns(true);

        GetMock<IGameContextFactory>()
            .Setup(mock => mock.GetGame(_game))
            .Returns(() => new GameContext(
                _game,
                Reducers.ToImmutableList(),
                StateStore,
                GetMock<IGameClock>().Object,
                GetMock<IKeyFrameService>().Object
            ));

        GetMock<ISystemStateStore>()
            .Setup(mock => mock.GetCurrentGame())
            .ReturnsAsync(Result.Succeed(_game));

        GetMock<IEventBus>()
            .Setup(mock => mock.AddEventAtCurrentTick(_game, It.IsAny<Event>()))
            .ReturnsAsync((GameInfo _, Event e) => e);

        GetMock<IConfigurationService>()
            .Setup(mock => mock.GetConfiguration(typeof(ControlPanelViewConfiguration)))
            .Returns(Result.Succeed<object>(new ControlPanelViewConfiguration(true, true, true, true, true, false, true, false, DisplaySide.Both)));

        GetMock<IConfigurationService>()
            .Setup(mock => mock.GetConfiguration(typeof(InputControls)))
            .Returns(Result.Succeed<object>(new InputControls(
                new(null, null, null, null),
                new(null, null, null, null, null, null, null, null),
                new(null, null, null, null, null, null, null, null),
                new(null, null, null, null, null),
                new(null, null, null, null, null))));

        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    protected override void Teardown()
    {
        base.Teardown();

        _driver.Dispose();
    }

    [Test]
    public async Task StartButton_BehavesAsExpected()
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetStateByName(nameof(GameStageState)))
            .Returns(Result.Succeed<object>(new GameStageState(Stage.BeforeGame, 1, 1, 1, false)));

        await _driver.Navigate().GoToUrlAsync(GetUrl("sbo"));

        SeleniumHelpers.RetryOnStale(() =>
        {
            try
            {
                var connectionLostAlert = _driver.FindElement(By.Id("ConnectionLostAlert"));

                connectionLostAlert.WaitForNotVisible();
            }
            catch (NoSuchElementException)
            {
                // Element isn't present
            }
        });

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Displayed.Should().BeTrue();
            startButton.Enabled.Should().BeTrue();

            startButton.Click();
        });

        GetMock<IEventBus>()
            .WaitVerify(mock => mock.AddEventAtCurrentTick(_game, It.IsAny<JamStarted>()), Times.Once);
        GetMock<IEventBus>().Invocations.Clear();

        StateStore.SetState(new GameStageState(Stage.Jam, 1, 1, 1, false));
        StateStore.SetState(new JamClockState(true, 0, 1500, true, false));
        StateStore.SetState(new PeriodClockState(true, false, true, 0, 0, 1500));

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Enabled.Should().BeFalse();
        });

        StateStore.SetState(new GameStageState(Stage.Lineup, 1, 1, 1, false));
        StateStore.SetState(new JamClockState(false, 0, 90_000, true, false));
        StateStore.SetState(new PeriodClockState(true, false, true, 0, 0, 95_000));

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Enabled.Should().BeTrue();
        });

        StateStore.SetState(new GameStageState(Stage.Timeout, 1, 1, 1, false));
        StateStore.SetState(new TimeoutClockState(true, 100_000, 0, TimeoutClockStopReason.None, 5_000));
        StateStore.SetState(new PeriodClockState(false, false, true, 0, 0, 100_000));

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Displayed.Should().BeTrue();
            startButton.Enabled.Should().BeTrue();

            startButton.Click();
        });

        GetMock<IEventBus>()
            .WaitVerify(mock => mock.AddEventAtCurrentTick(_game, It.IsAny<JamStarted>()), Times.Once);
        GetMock<IEventBus>().Invocations.Clear();

        StateStore.SetState(new GameStageState(Stage.AfterGame, 2, 22, 45, false));
        StateStore.SetState(new PeriodClockState(false, true, true, 100_000, 100_000, 30 * 2 * 60 * 1000));

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Displayed.Should().BeTrue();
            startButton.Enabled.Should().BeTrue();

            startButton.Click();
        });

        GetMock<IEventBus>()
            .WaitVerify(mock => mock.AddEventAtCurrentTick(_game, It.IsAny<JamStarted>()), Times.Once);
        GetMock<IEventBus>().Invocations.Clear();

        StateStore.SetState(new GameStageState(Stage.AfterGame, 2, 22, 45, true));
        StateStore.SetState(new PeriodClockState(false, true, true, 100_000, 100_000, 30 * 2 * 60 * 1000));

        Thread.Sleep(TimeSpan.FromSeconds(.1));

        SeleniumHelpers.RetryOnStale(() =>
        {
            var startButton = _driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));

            startButton.Displayed.Should().BeTrue();
            startButton.Enabled.Should().BeFalse();
        });
    }
}
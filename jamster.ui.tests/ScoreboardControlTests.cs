using System.Collections.Immutable;

using FluentAssertions;

using Func;

using jamster.engine.Configurations;
using jamster.engine.DataStores;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;
using jamster.ui.tests.Interactors;
using jamster.ui.tests.MockEngine;

using Moq;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        wait.Until(driver => driver.FindElement(By.Id("ConnectionLostAlert")));

        var scoreboardOperatorInteractor = new ScoreboardOperatorInteractor(_driver);

        scoreboardOperatorInteractor.ClickStart();
        VerifyEvent<JamStarted>();

        StateStore.SetState(new GameStageState(Stage.Jam, 1, 1, 1, false));
        StateStore.SetState(new JamClockState(true, 0, 1500, true, false));
        StateStore.SetState(new PeriodClockState(true, false, true, 0, 0, 1500));
        scoreboardOperatorInteractor.ValidateStartEnabled(false);

        StateStore.SetState(new GameStageState(Stage.Lineup, 1, 1, 1, false));
        StateStore.SetState(new JamClockState(false, 0, 90_000, true, false));
        StateStore.SetState(new PeriodClockState(true, false, true, 0, 0, 95_000));
        scoreboardOperatorInteractor.ValidateStartEnabled(true);

        StateStore.SetState(new GameStageState(Stage.Timeout, 1, 1, 1, false));
        StateStore.SetState(new TimeoutClockState(true, 100_000, 0, TimeoutClockStopReason.None, 5_000));
        StateStore.SetState(new PeriodClockState(false, false, true, 0, 0, 100_000));
        scoreboardOperatorInteractor.ClickStart();

        VerifyEvent<JamStarted>();
        StateStore.SetState(new GameStageState(Stage.AfterGame, 2, 22, 45, false));
        StateStore.SetState(new PeriodClockState(false, true, true, 100_000, 100_000, 30 * 2 * 60 * 1000));

        scoreboardOperatorInteractor.ClickStart();
        VerifyEvent<JamStarted>();

        StateStore.SetState(new GameStageState(Stage.AfterGame, 2, 22, 45, true));
        StateStore.SetState(new PeriodClockState(false, true, true, 100_000, 100_000, 30 * 2 * 60 * 1000));
        scoreboardOperatorInteractor.ValidateStartEnabled(false);
    }

    [Test]
    public async Task StopButton_BehavesAsExpected()
    {
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetStateByName(nameof(GameStageState)))
            .Returns(Result.Succeed<object>(new GameStageState(Stage.BeforeGame, 1, 1, 1, false)));

        await _driver.Navigate().GoToUrlAsync(GetUrl("sbo"));
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        wait.Until(driver => driver.FindElement(By.Id("ConnectionLostAlert")));

        var scoreboardOperatorInteractor = new ScoreboardOperatorInteractor(_driver);

        scoreboardOperatorInteractor.ValidateStopEnabled(true);
        scoreboardOperatorInteractor.ClickStop();
        VerifyEvent<IntermissionEnded>();

        StateStore.SetState(new GameStageState(Stage.Lineup, 1, 1, 1, false));
        scoreboardOperatorInteractor.ValidateStopEnabled(false);

        StateStore.SetState(new GameStageState(Stage.Jam, 1, 1, 1, false));
        scoreboardOperatorInteractor.ValidateStopEnabled(true);
        scoreboardOperatorInteractor.ClickStop();
        VerifyEvent<JamEnded>();

        StateStore.SetState(new GameStageState(Stage.Timeout, 1, 1, 1, false));
        scoreboardOperatorInteractor.ValidateStopEnabled(true);
        scoreboardOperatorInteractor.ClickStop();
        VerifyEvent<TimeoutEnded>();

        StateStore.SetState(new GameStageState(Stage.AfterTimeout, 1, 1, 1, false));
        scoreboardOperatorInteractor.ValidateStopEnabled(false);

        StateStore.SetState(new GameStageState(Stage.Intermission, 1, 1, 1, false));
        scoreboardOperatorInteractor.ValidateStopEnabled(true);
        scoreboardOperatorInteractor.ClickStop();
        VerifyEvent<PeriodFinalized>();

        StateStore.SetState(new GameStageState(Stage.Intermission, 1, 1, 1, true));
        scoreboardOperatorInteractor.ValidateStopEnabled(true);
        scoreboardOperatorInteractor.ClickStop();
        VerifyEvent<IntermissionEnded>();

        StateStore.SetState(new GameStageState(Stage.AfterGame, 2, 1, 1, true));
        scoreboardOperatorInteractor.ValidateStopEnabled(false);
    }

    private void VerifyEvent<TEvent>() where TEvent : Event
    {
        GetMock<IEventBus>()
            .WaitVerify(mock => mock.AddEventAtCurrentTick(_game, It.IsAny<TEvent>()), Times.Once);
        GetMock<IEventBus>().Invocations.Clear();
    }
}
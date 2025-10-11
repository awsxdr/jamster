using FluentAssertions;

using jamster.engine.Configurations;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.tests.EventHandling;
using jamster.engine.tests.GameGeneration;
using jamster.ui.tests.Interactors;
using jamster.ui.tests.MockEngine;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace jamster.ui.tests;

[TestFixture]
public class FullGameTest : FullEngineTest
{
    private readonly List<WebDriver> _drivers = new();

    protected override void OneTimeTearDown()
    {
        base.OneTimeTearDown();

        foreach (var driver in _drivers)
            driver.Dispose();
    }

    [Test]
    public void FullGameSimulation()
    {
        var game = GameGenerator.GenerateRandom();

        var sboDriver = CreateDriver();

        sboDriver.Navigate().GoToUrl(GetUrl("teams"));

        CreateTeam(game.HomeTeam, sboDriver);
        CreateTeam(game.AwayTeam, sboDriver);

        sboDriver.Navigate().GoToUrl(GetUrl("games"));

        var gameName = CreateGame(game, sboDriver);

        sboDriver.Navigate().GoToUrl(GetUrl("sbo"));
        sboDriver.Manage().Window.Position = new(0, 0);
        sboDriver.Manage().Window.Size = new(1920 / 2, 1080);

        var gameEvents = new GameSimulator(game).SimulateGame().Where(e => e is not IFakeEvent).ToArray();
        //gameEvents = gameEvents.TakeWhile(e => e is not CallMarked).ToArray();
        using var gameClock = new GameClock();

        var sboInteractor = new ScoreboardOperatorInteractor(sboDriver);
        sboInteractor.ClickGameSelect();
        sboInteractor.SelectGame(gameName);

        var pltTasks = new[] { TeamSide.Home, TeamSide.Away }.Select((side, i) =>
        {
            var penaltyLineupDriver = CreateDriver();
            penaltyLineupDriver.Manage().Window.Position = new(1920 / 2, 1080 / 2 * i);
            penaltyLineupDriver.Manage().Window.Size = new(1920 / 2, 1080 / 2);
            penaltyLineupDriver.Navigate().GoToUrl(GetUrl("plt"));

            var penaltyLineupInteractor = new PenaltyLineupInteractor(penaltyLineupDriver);
            var penaltyDialogInteractor = new PenaltyDialogInteractor(penaltyLineupDriver);
            penaltyLineupInteractor.ClickGameSelect();
            penaltyLineupInteractor.SelectGame(gameName);

            return PltGame(gameEvents, gameClock, side, penaltyLineupInteractor, penaltyDialogInteractor);
        }).ToArray();

        var sboTask = SboGame(gameEvents, gameClock, sboInteractor);

        var startTick = gameEvents.First(e => e is not TeamSet).Tick - Tick.FromSeconds(5);

        gameClock.Start(startTick);

        Task.WaitAll([sboTask, ..pltTasks]);
    }

    private Task SboGame(IEnumerable<Event> events, IReminderSetter reminderSetter, ScoreboardOperatorInteractor interactor) => Task.Run(async () =>
    {
        var jamNumber = 0;

        foreach (var @event in events)
        {
            Console.WriteLine(@event.GetType().Name);
            await reminderSetter.WaitForTick(@event.Tick);

            switch (@event)
            {
                case JamStarted:
                    ++jamNumber;
                    interactor.ClickStart();
                    break;

                case JamEnded:
                case IntermissionEnded:
                case TimeoutEnded:
                case PeriodFinalized:
                    interactor.ClickStop();
                    break;

                case TimeoutStarted:
                    interactor.ClickNewTimeout();
                    break;

                case LeadMarked leadMarked:
                    interactor.SetLead(leadMarked.Body.TeamSide);
                    break;

                case LostMarked lostMarked:
                    interactor.SetLost(lostMarked.Body.TeamSide);
                    break;

                case CallMarked callMarked:
                    interactor.SetCall(callMarked.Body.TeamSide);
                    break;

                case StarPassMarked starPassMarked:
                    interactor.SetStarPass(starPassMarked.Body.TeamSide);
                    break;

                case InitialTripCompleted initialTripCompleted:
                    interactor.SetInitialTrip(initialTripCompleted.Body.TeamSide);
                    break;

                case SkaterOnTrack { Body.Position: SkaterPosition.Jammer or SkaterPosition.Pivot } skaterOnTrack when jamNumber % 2 == 1:
                    interactor.LineupSkater(skaterOnTrack.Body.TeamSide, skaterOnTrack.Body.Position, skaterOnTrack.Body.SkaterNumber);
                    break;

                case ScoreModifiedRelative { Body.Value: >= 0 } scoreModifiedRelative:
                    interactor.SetTripScore(scoreModifiedRelative.Body.TeamSide, scoreModifiedRelative.Body.Value);
                    break;

                case TimeoutTypeSet timeoutTypeSet:
                    interactor.SetTimeoutType(timeoutTypeSet.Body.Type, timeoutTypeSet.Body.TeamSide);
                    break;
            }
        }
    });

    private Task PltGame(
        IEnumerable<Event> events,
        IReminderSetter reminderSetter,
        TeamSide teamSide,
        PenaltyLineupInteractor interactor,
        PenaltyDialogInteractor penaltyDialogInteractor
    ) => 
        Task.Run(async () =>
        {
            var jamNumber = 0;

            var penaltyCounts = new Dictionary<string, int>();

            interactor.ClickViewMenu();
            interactor.ClickViewMenuTeam(teamSide == TeamSide.Home ? DisplaySide.Home : DisplaySide.Away);

            foreach (var @event in events)
            {
                Console.WriteLine(@event.GetType().Name);
                await reminderSetter.WaitForTick(@event.Tick);

                if (@event is not CallMarked && @event.HasBody && @event.GetBodyObject() is TeamEventBody teamBody && teamBody.TeamSide != teamSide)
                    continue;

                switch (@event)
                {
                    case JamStarted:
                        ++jamNumber;
                        break;

                    case SkaterOnTrack { Body.Position: SkaterPosition.Blocker } skaterOnTrack:
                        interactor.TryGoToNextJam();
                        interactor.AddSkaterToJam(skaterOnTrack.Body.SkaterNumber, skaterOnTrack.Body.Position);
                        break;

                    case SkaterOnTrack skaterOnTrack when jamNumber % 2 == 0:
                        interactor.TryGoToNextJam();
                        interactor.AddSkaterToJam(skaterOnTrack.Body.SkaterNumber, skaterOnTrack.Body.Position);
                        break;

                    case PenaltyAssessed penaltyAssessed:
                        penaltyCounts.TryAdd(penaltyAssessed.Body.SkaterNumber, 0);
                        var penaltyNumber = ++penaltyCounts[penaltyAssessed.Body.SkaterNumber];
                        interactor.ClickPenalty(penaltyAssessed.Body.SkaterNumber, penaltyNumber);
                        penaltyDialogInteractor.ClickPenalty(penaltyAssessed.Body.PenaltyCode);
                        break;

                    case SkaterSatInBox skaterSatInBox:
                        interactor.ClickBoxButton(skaterSatInBox.Body.SkaterNumber);
                        break;

                    case SkaterReleasedFromBox skaterReleasedFromBox:
                        interactor.ClickBoxButton(skaterReleasedFromBox.Body.SkaterNumber);
                        break;
                }
            }
        });

    private void CreateTeam(SimulatorTeam team, IWebDriver driver)
    {
        var teamPageInteractor = new TeamsPageInteractor(driver);
        teamPageInteractor.OpenAddTeamDialog();

        var addTeamDialogInteractor = new AddTeamDialogInteractor(driver);
        addTeamDialogInteractor.SetTeamName(team.DomainTeam.Names["league"]);
        addTeamDialogInteractor.ValidateTeamName(team.DomainTeam.Names["league"]);
        addTeamDialogInteractor.SetKitColor(team.DomainTeam.Names["color"]);
        addTeamDialogInteractor.ValidateKitName(team.DomainTeam.Names["color"]);

        addTeamDialogInteractor.ClickCreate();

        var teamDetails = teamPageInteractor.GetTeam(team.DomainTeam.Names["league"]);
        teamDetails.TeamName.Should().Be(team.DomainTeam.Names["league"]);

        teamPageInteractor.ClickTeam(team.DomainTeam.Names["league"]);

        EnterTeamDetails(team, driver);

        driver.Navigate().Back();

        teamDetails = teamPageInteractor.GetTeam(team.DomainTeam.Names["league"]);
        teamDetails.LeagueName.Should().Be($"{team.DomainTeam.Names["league"]} (League)");
        teamDetails.TeamName.Should().Be($"{team.DomainTeam.Names["league"]}");
    }

    private void EnterTeamDetails(SimulatorTeam team, IWebDriver driver)
    {
        var teamDetailsInteractor = new TeamDetailsPageInteractor(driver);
        teamDetailsInteractor.ValidateTeamName(team.DomainTeam.Names["league"]);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        teamDetailsInteractor.SetLeagueName(team.DomainTeam.Names["league"] + " (League)");
        teamDetailsInteractor.ValidateLeagueName(team.DomainTeam.Names["league"] + " (League)");
        teamDetailsInteractor.SetScoreboardName(team.DomainTeam.Names["league"] + " (Scoreboard)");
        teamDetailsInteractor.ValidateScoreboardName(team.DomainTeam.Names["league"] + " (Scoreboard)");
        teamDetailsInteractor.SetOverlayName(team.DomainTeam.Names["league"] + " (Overlay)");
        teamDetailsInteractor.ValidateOverlayName(team.DomainTeam.Names["league"] + " (Overlay)");

        teamDetailsInteractor.ValidateColorPresent(team.DomainTeam.Names["color"]);

        var randomizedRoster = team.Roster.Shuffle().ToArray();
        var manualEntryRoster = randomizedRoster[0..(randomizedRoster.Length / 2)];
        var pasteEntryRoster = randomizedRoster.Skip(randomizedRoster.Length / 2).Take((randomizedRoster.Length - randomizedRoster.Length / 2) / 2).ToArray();
        var reversedPasteEntryRoster = randomizedRoster[(manualEntryRoster.Length + pasteEntryRoster.Length)..];

        (manualEntryRoster.Length + pasteEntryRoster.Length + reversedPasteEntryRoster.Length).Should().Be(team.Roster.Length);

        foreach (var skater in manualEntryRoster)
            teamDetailsInteractor.AddSkaterToRoster(skater.Number, skater.Name);

        teamDetailsInteractor.PasteRoster(pasteEntryRoster);
        teamDetailsInteractor.ValidateRosterLength(manualEntryRoster.Length + pasteEntryRoster.Length);

        teamDetailsInteractor.PasteRoster(reversedPasteEntryRoster);
        teamDetailsInteractor.ValidateRosterLength(team.Roster.Length);

        var tableData = teamDetailsInteractor.GetRoster();
        var sortedRoster = team.Roster.OrderBy(s => s.Number).ToArray();
        tableData.Select(s => (s.Number, s.Name)).Should().BeEquivalentTo(sortedRoster.Select(s => (s.Number, s.Name)));
    }

    private string CreateGame(SimulatorGame game, IWebDriver driver)
    {
        var gamesPageInteractor = new GamesPageInteractor(driver);

        gamesPageInteractor.OpenAddGameDialog();

        var newGameDialogInteractor = new NewGameDialogInteractor(driver);

        newGameDialogInteractor.ClickHomeTeamSelect();
        newGameDialogInteractor.SelectHomeTeam($"{game.HomeTeam.DomainTeam.Names["league"]} ({game.HomeTeam.DomainTeam.Names["league"]} (League))");

        newGameDialogInteractor.ClickAwayTeamSelect();
        newGameDialogInteractor.SelectAwayTeam($"{game.AwayTeam.DomainTeam.Names["league"]} ({game.AwayTeam.DomainTeam.Names["league"]} (League))");

        var gameName = newGameDialogInteractor.GetGameName();

        newGameDialogInteractor.ClickCreateButton();

        var createdGame = gamesPageInteractor.GetGame(gameName);
        createdGame.Name.Should().Be(gameName);
        createdGame.HomeTeam.Should().Be(game.HomeTeam.DomainTeam.Names["league"]);
        createdGame.AwayTeam.Should().Be(game.AwayTeam.DomainTeam.Names["league"]);
        createdGame.Status.Should().Be("Upcoming");

        return gameName;
    }

    private IWebDriver CreateDriver()
    {
        var driver = new ChromeDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        _drivers.Add(driver);

        return driver;
    }
}

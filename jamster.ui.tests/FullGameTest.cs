using FluentAssertions;

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
    private IWebDriver _driver;

    protected override void OneTimeSetup()
    {
        base.OneTimeSetup();

        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    protected override void OneTimeTearDown()
    {
        base.OneTimeTearDown();

        _driver.Dispose();
    }

    [Test]
    public void FullGameSimulation()
    {
        var game = GameGenerator.GenerateRandom();

        _driver.Navigate().GoToUrl(GetUrl("teams"));

        CreateTeam(game.HomeTeam);
        CreateTeam(game.AwayTeam);

        _driver.Navigate().GoToUrl(GetUrl("games"));

        var gameName = CreateGame(game);

        _driver.Navigate().GoToUrl(GetUrl("sbo"));

        var gameEvents = new GameSimulator(game).SimulateGame().Where(e => e is not IFakeEvent).ToArray();
        using var gameClock = new GameClock();

        var sboInteractor = new ScoreboardOperatorInteractor(_driver);
        sboInteractor.ClickGameSelect();
        sboInteractor.SelectGame(gameName);

        var sboTask = SboGame(gameEvents, gameClock);

        var startTick = gameEvents.First(e => e is not TeamSet).Tick - Tick.FromSeconds(5);

        gameClock.Start(startTick);

        Task.WaitAll(sboTask);
    }

    private Task SboGame(IEnumerable<Event> events, IReminderSetter reminderSetter) => Task.Run(async () =>
    {
        var interactor = new ScoreboardOperatorInteractor(_driver);

        foreach (var @event in events)
        {
            Console.WriteLine(@event.GetType().Name);
            await reminderSetter.WaitForTick(@event.Tick);

            switch (@event)
            {
                case JamStarted:
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

                case SkaterOnTrack { Body.Position: SkaterPosition.Jammer or SkaterPosition.Pivot } skaterOnTrack:
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

    private void CreateTeam(SimulatorTeam team)
    {
        var teamPageInteractor = new TeamsPageInteractor(_driver);
        teamPageInteractor.OpenAddTeamDialog();

        var addTeamDialogInteractor = new AddTeamDialogInteractor(_driver);
        addTeamDialogInteractor.SetTeamName(team.DomainTeam.Names["league"]);
        addTeamDialogInteractor.SetKitColor(team.DomainTeam.Names["color"]);

        addTeamDialogInteractor.ClickCreate();

        var teamDetails = teamPageInteractor.GetTeam(team.DomainTeam.Names["league"]);
        teamDetails.TeamName.Should().Be(team.DomainTeam.Names["league"]);

        teamPageInteractor.ClickTeam(team.DomainTeam.Names["league"]);

        EnterTeamDetails(team);

        _driver.Navigate().Back();

        teamDetails = teamPageInteractor.GetTeam(team.DomainTeam.Names["league"]);
        teamDetails.LeagueName.Should().Be($"{team.DomainTeam.Names["league"]} (League)");
        teamDetails.TeamName.Should().Be($"{team.DomainTeam.Names["league"]}");
    }

    private void EnterTeamDetails(SimulatorTeam team)
    {
        var teamDetailsInteractor = new TeamDetailsPageInteractor(_driver);
        teamDetailsInteractor.GetTeamName().Should().Be(team.DomainTeam.Names["league"]);

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        teamDetailsInteractor.SetLeagueName(team.DomainTeam.Names["league"] + " (League)");
        teamDetailsInteractor.SetScoreboardName(team.DomainTeam.Names["league"] + " (Scoreboard)");
        teamDetailsInteractor.SetOverlayName(team.DomainTeam.Names["league"] + " (Overlay)");

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

    private string CreateGame(SimulatorGame game)
    {
        var gamesPageInteractor = new GamesPageInteractor(_driver);

        gamesPageInteractor.OpenAddGameDialog();

        var newGameDialogInteractor = new NewGameDialogInteractor(_driver);

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
}

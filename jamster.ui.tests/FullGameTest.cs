using System.Diagnostics;

using FluentAssertions;

using Func;

using jamster.engine.Domain;
using jamster.engine.Extensions;
using jamster.engine.tests.GameGeneration;
using jamster.ui.tests.MockEngine;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using TextCopy;

using static jamster.ui.tests.SeleniumHelpers;

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
    public void Test()
    {
        try
        {
            var game = GameGenerator.GenerateRandom();

            _driver.Navigate().GoToUrl(GetUrl("teams"));

            CreateTeam(game.HomeTeam);
            CreateTeam(game.AwayTeam);

            _driver.Navigate().GoToUrl(GetUrl("games"));
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
        catch (AssertionException)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            throw;
        }
    }

    private void CreateTeam(SimulatorTeam team)
    {
        var teamPageInteractor = new TeamsPageInteractor(_driver);
        teamPageInteractor.OpenAddTeamDialog();

        var addTeamDialogInteractor = new AddTeamDialogInteractor(_driver);
        addTeamDialogInteractor.SetTeamName(team.DomainTeam.Names["league"]);
        addTeamDialogInteractor.SetKitColor(team.DomainTeam.Names["color"]);

        Thread.Sleep(TimeSpan.FromSeconds(.5));

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
        var teamNameInput = _driver.FindElement(By.Id("TeamNames.TeamName"));
        teamNameInput.WaitForVisible();
        teamNameInput.Displayed.Should().BeTrue();
        teamNameInput.GetAttribute("value").Should().Be(team.DomainTeam.Names["league"]);

        var leagueNameInput = _driver.FindElement(By.Id("TeamNames.LeagueName"));
        leagueNameInput.Displayed.Should().BeTrue();
        leagueNameInput.GetAttribute("value").Should().BeEmpty();
        leagueNameInput.SendKeys(team.DomainTeam.Names["league"] + " (League)");

        var scoreboardNameInput = _driver.FindElement(By.Id("TeamNames.ScoreboardName"));
        scoreboardNameInput.Displayed.Should().BeTrue();
        scoreboardNameInput.GetAttribute("value").Should().BeEmpty();
        scoreboardNameInput.SendKeys(team.DomainTeam.Names["league"] + " (Scoreboard)");

        var overlayNameInput = _driver.FindElement(By.Id("TeamNames.OverlayName"));
        overlayNameInput.Displayed.Should().BeTrue();
        overlayNameInput.GetAttribute("value").Should().BeEmpty();
        overlayNameInput.SendKeys(team.DomainTeam.Names["league"] + " (Overlay)");

        var colorsTableRow = _driver.FindElement(By.Id("TeamColors.ColorsTable.0"));
        colorsTableRow.Displayed.Should().BeTrue();
        var colorsTableRowItems = colorsTableRow.FindElements(By.TagName("div"));
        colorsTableRowItems.Should().ContainSingle(i => i.Text == team.DomainTeam.Names["color"]);

        var randomizedRoster = team.Roster.Shuffle().ToArray();
        var manualEntryRoster = randomizedRoster[0..(randomizedRoster.Length / 2)];
        var pasteEntryRoster = randomizedRoster.Skip(randomizedRoster.Length / 2).Take((randomizedRoster.Length - randomizedRoster.Length / 2) / 2).ToArray();
        var reversedPasteEntryRoster = randomizedRoster[(manualEntryRoster.Length + pasteEntryRoster.Length)..];

        (manualEntryRoster.Length + pasteEntryRoster.Length + reversedPasteEntryRoster.Length).Should().Be(team.Roster.Length);

        var skaterNumberInput = _driver.FindElement(By.Id("RosterInput.Number"));
        skaterNumberInput.Displayed.Should().BeTrue();

        var skaterNameInput = _driver.FindElement(By.Id("RosterInput.Name"));
        skaterNameInput.Displayed.Should().BeTrue();

        var addSkaterButton = _driver.FindElement(By.Id("RosterInput.AddSkaterButton"));
        addSkaterButton.Displayed.Should().BeTrue();

        foreach (var skater in manualEntryRoster)
        {
            Console.WriteLine($"Manually entering skater with number {skater.Number} and name {skater.Name}");

            skaterNumberInput.GetAttribute("value").Should().BeEmpty();
            skaterNameInput.GetAttribute("value").Should().BeEmpty();

            skaterNumberInput.SendKeys(skater.Number);
            skaterNameInput.SendKeys(skater.Name);

            addSkaterButton.Click();
        }

        PasteRoster(pasteEntryRoster, skaterNumberInput);
        PasteRoster(reversedPasteEntryRoster, skaterNumberInput);

        var tableData =
            team.Roster.OrderBy(s => s.Number)
                .Select((_, i) => _driver.FindElement(By.Id($"RosterTable.Row.{i}")))
                .Select(e => e.FindElements(By.TagName("span")))
                .Select(e => (Number: e[0].Text, Name: e[1].Text))
                .ToArray();

        Console.WriteLine($"Retrieved skaters:\n{tableData.Select(s => $"{s.Number}\t{s.Name}").Map(string.Join, "\n")}");

        Console.WriteLine();
        Console.WriteLine($"Expected skaters:\n{team.Roster.Select(s => $"{s.Number}\t{s.Name}").Map(string.Join, "\n")}");

        if (!tableData.Zip(team.Roster.Select(s => (s.Number, s.Name)), (a, b) => a.Number == b.Number && a.Name == b.Name).All(x => x))
        {
            Thread.Sleep(30000);
        }

        tableData.Should().BeEquivalentTo(team.Roster.Select(s => (s.Number, s.Name)));
    }

    private void PasteRoster(SimulatorSkater[] roster, IWebElement skaterNumberInput)
    {
        var pasteText = roster.Select(r => $"{r.Number}\t{r.Name}").Map(string.Join, "\n");
        ClipboardService.SetText(pasteText);
        Console.WriteLine($"Pasting skater data: {pasteText}");

        skaterNumberInput.SendKeys(Keys.Control + "v");
    }
}

public class TeamsPageInteractor(IWebDriver driver)
{
    public record TeamPageTeam(string TeamName, string LeagueName, DateTime LastModified);

    public void OpenAddTeamDialog()
    {
        var addTeamButton = driver.FindElement(By.Id("TeamsManagement.AddTeamButton"));
        addTeamButton.Displayed.Should().BeTrue();
        addTeamButton.Click();
    }

    public TeamPageTeam GetTeam(string teamName) => RetryOnStale(() =>
    {
        var team = driver.FindElement(By.Id("TeamTable"))
            .FindElements(By.TagName("tr"))
            .Select(row => row.FindElements(By.TagName("td")).Select(item => item.Text).ToArray())
            .Where(row => row.Length == 4)
            .Single(row => row[1] == teamName);

        return new TeamPageTeam(team[1], team[2], DateTime.TryParse(team[3], out var modified) ? modified : DateTime.MinValue);
    });

    public TeamPageTeam[] GetTeams() => RetryOnStale(() =>
    {
        var teamTable = driver.FindElement(By.Id("TeamTable"));
        teamTable.Displayed.Should().BeTrue();

        var teams = teamTable.FindElements(By.TagName("tr"))
            .Select(row =>
            {
                var items = row.FindElements(By.TagName("td"));

                if (items.Count != 4)
                    return null;

                return new TeamPageTeam(
                    items[1].Text,
                    items[2].Text,
                    DateTime.TryParse(items[3].Text, out var modified) ? modified : DateTime.MinValue
                );
            })
            .Where(item => item != null)
            .Cast<TeamPageTeam>()
            .ToArray();

        return teams;
    });

    public void ClickTeam(string teamName)
    {
        var teamTable = driver.FindElement(By.Id("TeamTable"));
        teamTable.Displayed.Should().BeTrue();

        var team = teamTable.FindElements(By.TagName("tr"))
            .Select(row => row.FindElements(By.TagName("td")))
            .Where(row => row.Count == 4)
            .FirstOrDefault(row => row[1].Text == teamName)
            ?[1];

        team.Should().NotBeNull();

        team!.Displayed.Should().BeTrue();

        team.Click();
    }
}

public class AddTeamDialogInteractor(IWebDriver driver)
{
    public void SetTeamName(string teamName)
    {
        var teamNameInput = driver.FindElement(By.Id("NewTeamDialog.TeamName"));
        teamNameInput.WaitForVisible();

        teamNameInput.Displayed.Should().BeTrue();
        teamNameInput.SendKeys(teamName);
    }

    public void SetKitColor(string color)
    {
        var colorInput = driver.FindElement(By.Id("NewTeamDialog.KitColor"));
        colorInput.Displayed.Should().BeTrue();
        colorInput.SendKeys(color);
    }

    public void ClickCreate()
    {
        var createButton = driver.FindElement(By.Id("NewTeamDialog.CreateButton"));
        createButton.Displayed.Should().BeTrue();
        createButton.Click();
    }
}
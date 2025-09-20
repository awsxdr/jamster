using System.Diagnostics;

using FluentAssertions;

using Func;

using jamster.Domain;
using jamster.engine.tests.GameGeneration;
using jamster.Extensions;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using TextCopy;

namespace jamster.ui.tests;

[TestFixture]
public class FullGameTest
{
    private IWebDriver _driver;
    private Process _engineProcess;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        Directory.Delete(Path.Combine(".", "engine", "db"), true);

        _engineProcess = 
            Process.Start(new ProcessStartInfo(Path.Combine(".", "engine", "jamster.engine.exe"))
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            }) ?? throw new Exception("Failed to start engine process");

        _engineProcess.OutputDataReceived += (_, e) => Console.WriteLine(e.Data);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _engineProcess.Kill();
        _engineProcess.Dispose();
        _driver.Dispose();
    }

    [Test]
    public void Test()
    {
        try
        {
            var game = GameGenerator.GenerateRandom();

            _driver.Navigate().GoToUrl("http://localhost:8000/teams");

            CreateTeam(game.HomeTeam);
            CreateTeam(game.AwayTeam);
        }
        catch (AssertionException)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            throw;
        }
    }

    [Test]
    public void PasteIntoTeam_WhenContainsNumberToGoAtStart_PastesSuccessfully()
    {
        try
        {
            _driver.Navigate().GoToUrl("http://localhost:8000/teams");

            var skaters = Enumerable.Range(1, 10).Select(i => new Skater(i.ToString(), $"Test {i}")).ToArray();

            CreateTeam(new SimulatorTeam(
                new(
                    Guid.NewGuid(),
                    new() { ["league"] = "Test League", ["color"] = "Red" },
                    new() { ["Red"] = new(Color.FromRgb(255, 0, 0), Color.White) },
                    skaters,
                    DateTimeOffset.Now),
                skaters.Select(s => new SimulatorSkater(s, SkaterPosition.Blocker, 1.0f, 0.0f)).ToArray()));

            var teamTable = _driver.FindElement(By.Id("TeamTable"));
            teamTable.Displayed.Should().BeTrue();
            var teamElement =
                teamTable.FindElements(By.TagName("a")).Should()
                    .ContainSingle(e => e.Text == "Test League")
                    .Subject;
            teamElement.Click();

            var skaterNumberInput = _driver.FindElement(By.Id("RosterInput.Number"));
            skaterNumberInput.Displayed.Should().BeTrue();

            PasteRoster(
                Enumerable.Range(0, 3).Select(i => new Skater($"{i}0", $"Test 2{i}")).Select(s => new SimulatorSkater(s, SkaterPosition.Blocker, 1.0f, 0.0f)).ToArray(),
                skaterNumberInput
            );

            var tableData =
                Enumerable.Range(0, 13)
                    .Select(i => _driver.FindElement(By.Id($"RosterTable.Row.{i}")))
                    .Select(e => e.FindElements(By.TagName("span")))
                    .Select(e => (Number: e[0].Text, Name: e[1].Text))
                    .ToArray();

            Console.WriteLine($"Retrieved skaters:\n{tableData.Select(s => $"{s.Number}\t{s.Name}").Map(string.Join, "\n")}");

            Console.WriteLine();
            //Console.WriteLine($"Expected skaters:\n{team.Roster.Select(s => $"{s.Number}\t{s.Name}").Map(string.Join, "\n")}");

            //tableData.Should().BeEquivalentTo(team.Roster.Select(s => (s.Number, s.Name)));

            Thread.Sleep(5);
        }
        catch (AssertionException)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            throw;
        }
    }

    private void CreateTeam(SimulatorTeam team)
    {
        var addTeamButton = _driver.FindElement(By.Id("TeamsManagement.AddTeamButton"));
        addTeamButton.Displayed.Should().BeTrue();
        addTeamButton.Click();

        var teamNameInput = _driver.FindElement(By.Id("NewTeamDialog.TeamName"));
        teamNameInput.WaitForVisible();
        teamNameInput.Displayed.Should().BeTrue();
        teamNameInput.SendKeys(team.DomainTeam.Names["league"]);

        var colorInput = _driver.FindElement(By.Id("NewTeamDialog.KitColor"));
        colorInput.Displayed.Should().BeTrue();
        colorInput.SendKeys(team.DomainTeam.Names["color"]);

        var createButton = _driver.FindElement(By.Id("NewTeamDialog.CreateButton"));
        createButton.Displayed.Should().BeTrue();
        createButton.Click();

        var teamTable = _driver.FindElement(By.Id("TeamTable"));
        teamTable.Displayed.Should().BeTrue();
        var teamElement =
            teamTable.FindElements(By.TagName("a")).Should()
                .ContainSingle(e => e.Text == team.DomainTeam.Names["league"])
                .Subject;
        teamElement.Displayed.Should().BeTrue();

        teamElement.Click();

        EnterTeamDetails(team);

        _driver.Navigate().Back();
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

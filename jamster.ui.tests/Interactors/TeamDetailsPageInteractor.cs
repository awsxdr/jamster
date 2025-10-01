using Func;

using jamster.engine.tests.GameGeneration;

using OpenQA.Selenium;

using TextCopy;

namespace jamster.ui.tests.Interactors;

public class TeamDetailsPageInteractor(IWebDriver driver) : Interactor(driver)
{
    public string? GetTeamName() =>
        Wait.Until(driver =>
            {
                var teamNameInput = driver.FindElement(By.Id("TeamNames.TeamName"));
                return (teamNameInput.Displayed, teamNameInput);
            },
            teamNameInput => teamNameInput.GetAttribute("value"));

    public void SetLeagueName(string name) =>
        Wait.Until(driver =>
            {
                var leagueNameInput = driver.FindElement(By.Id("TeamNames.LeagueName"));
                return (leagueNameInput.Displayed && leagueNameInput.GetAttribute("value") == "", leagueNameInput);
            },
            leagueNameInput =>
            {
                leagueNameInput.SendKeys(name);
                leagueNameInput.SendKeys(Keys.Tab);
            });

    public void SetScoreboardName(string name) =>
        Wait.Until(driver =>
            {
                var scoreboardNameInput = driver.FindElement(By.Id("TeamNames.ScoreboardName"));
                return (scoreboardNameInput.Displayed && scoreboardNameInput.GetAttribute("value") == "", scoreboardNameInput);
            },
            scoreboardNameInput =>
            {
                scoreboardNameInput.SendKeys(name);
                scoreboardNameInput.SendKeys(Keys.Tab);
            });

    public void SetOverlayName(string name) =>
        Wait.Until(driver =>
            {
                var overlayNameInput = driver.FindElement(By.Id("TeamNames.OverlayName"));
                return (overlayNameInput.Displayed && overlayNameInput.GetAttribute("value") == "", overlayNameInput);
            },
            overlayNameInput =>
            {
                overlayNameInput.SendKeys(name);
                overlayNameInput.SendKeys(Keys.Tab);
            });

    public void ValidateColorPresent(string color) =>
        Wait.Until(driver =>
            {
                var rowItem = driver.FindElement(By.XPath($"//*[@id=\"TeamColors.ColorsTable.0\"]//span[text()=\"{color}\"]"));
                return rowItem.Displayed;
            });

    public void AddSkaterToRoster(string number, string name) =>
        Wait.Until(driver =>
            {
                var skaterNumberInput = driver.FindElement(By.Id("RosterInput.Number"));
                var skaterNameInput = driver.FindElement(By.Id("RosterInput.Name"));
                var addSkaterButton = driver.FindElement(By.Id("RosterInput.AddSkaterButton"));

                return (
                    skaterNumberInput.Displayed && skaterNameInput.Displayed && addSkaterButton.Displayed && skaterNumberInput.GetAttribute("value") == "" && skaterNameInput.GetAttribute("value") == "",
                    (skaterNumberInput, skaterNameInput, addSkaterButton)
                );
            },
            elements =>
            {
                var (skaterNumberInput, skaterNameInput, addSkaterButton) = elements;

                skaterNumberInput.SendKeys(number);
                skaterNameInput.SendKeys(name);
                addSkaterButton.Click();
            });

    public void PasteRoster(SimulatorSkater[] roster) =>
        Wait.Until(driver =>
            {
                var skaterNumberInput = driver.FindElement(By.Id("RosterInput.Number"));
                return (
                    skaterNumberInput.Displayed && skaterNumberInput.GetAttribute("value") == "",
                    skaterNumberInput
                );
            },
            skaterNumberInput =>
            {
                var pasteText = roster.Select(r => $"{r.Number}\t{r.Name}").Map(string.Join, "\n");
                ClipboardService.SetText(pasteText);

                skaterNumberInput.SendKeys(Keys.Control + "v");
            }
        );

    public void ValidateRosterLength(int expectedLength) =>
        Wait.Until(driver => driver.FindElements(By.XPath("//*[@id=\"RosterTable\"]//div[starts-with(@id,\"RosterTable.Row.\")]")).Count == expectedLength);

    public RosterItem[] GetRoster() =>
        Wait.Until(driver =>
        {
            var rowCount = driver.FindElements(By.XPath("//*[@id=\"RosterTable\"]//div[starts-with(@id,\"RosterTable.Row.\")]")).Count;

            var result = new List<RosterItem>();

            foreach (var row in Enumerable.Range(1, rowCount))
            {
                var spans = driver.FindElements(By.XPath($"//*[@id=\"RosterTable\"]//div[starts-with(@id,\"RosterTable.Row.\")][{row}]//span"));
                result.Add(new()
                {
                    Number = spans.Count > 0 ? spans[0].Text : "",
                    Name = spans.Count > 1 ? spans[1].Text : "",
                });
            }

            return result.ToArray();
        });

    public class RosterItem
    {
        public required string Number { get; init; }
        public required string Name { get; init; }
    }
}
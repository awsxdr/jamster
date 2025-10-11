using jamster.engine.Configurations;
using jamster.engine.Domain;

using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class PenaltyLineupInteractor(IWebDriver driver) : Interactor(driver)
{
    public void ClickGameSelect() =>
        Wait.Until(driver =>
            {
                var gameSelect = driver.FindElement(By.Id("PenaltyLineup.GameSelectMenu"));
                return (gameSelect.Displayed, gameSelect);
            },
            gameSelect => gameSelect.Click());

    public void SelectGame(string gameName) =>
        Wait.Until(driver =>
            {
                var option = driver.FindElement(By.XPath($"//div[@role=\"option\"]/span[text()=\"{gameName}\"]"));
                return (option.Displayed, option);
            },
            option => option.Click());

    public void ClickViewMenu() =>
        Wait.Until(driver =>
            {
                var viewMenuButton = driver.FindElement(By.Id("PenaltyLineup.ViewMenu"));
                return (viewMenuButton.Displayed, viewMenuButton);
            },
            viewMenuButton => viewMenuButton.Click());

    public void ClickViewMenuTeam(DisplaySide side) =>
        Wait.Until(driver =>
            {
                var sideButton = driver.FindElement(By.Id($"PenaltyLineup.ViewMenu.Team.{side}"));
                return (sideButton.Displayed, sideButton);
            },
            sideButton => sideButton.Click());

    public void AddSkaterToJam(string skaterNumber, SkaterPosition position) =>
        Wait.Until(driver =>
            {
                var positionButton = driver.FindElement(By.Id($"PenaltyLineup.LineupTable.Skater{skaterNumber}.Position.{position.ToString()}"));
                return (positionButton.Displayed, positionButton);
            },
            positionButton => positionButton.Click());

    public void TryGoToNextJam() =>
        Wait.Until(driver =>
            {
                var nextJamButton = driver.FindElement(By.Id("PenaltyLineup.NextJamButton"));
                return (nextJamButton.Displayed, nextJamButton);
            },
            nextJamButton =>
            {
                if (nextJamButton.Enabled)
                    nextJamButton.Click();
            });

    public void ClickPenalty(string skaterNumber, int penaltyNumber) =>
        Wait.Until(driver =>
            {
                var penalty = driver.FindElement(By.Id($"PenaltyLineup.PenaltyRow.{skaterNumber}.{penaltyNumber}"));
                return (penalty.Displayed, penalty);
            },
            penalty => penalty.Click());

    public void ClickBoxButton(string skaterNumber) =>
        Wait.Until(driver =>
            {
                var boxButton = driver.FindElement(By.Id($"PenaltyLineup.LineupTable.Skater{skaterNumber}.InBox"));
                return (boxButton.Displayed, boxButton);
            },
            boxButton => boxButton.Click());
}
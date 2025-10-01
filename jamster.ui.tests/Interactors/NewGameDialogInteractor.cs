using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class NewGameDialogInteractor(IWebDriver driver) : Interactor(driver)
{
    public void ClickHomeTeamSelect() =>
        Wait.Until(driver =>
            {
                var homeTeamSelect = driver.FindElement(By.Id("NewGameDialog.HomeTeamSelect"));
                return (homeTeamSelect.Displayed, homeTeamSelect);
            },
            homeTeamSelect => homeTeamSelect.Click()
        );

    public void SelectHomeTeam(string teamName) =>
        Wait.Until(driver =>
            {
                var team = driver.FindElement(By.XPath($"//*[@id=\"NewGameDialog.HomeTeamSelect.List\"]//*[text()=\"{teamName}\"]"));
                return (team.Displayed, team);
            },
            team => team.Click()
        );

    public void ClickAwayTeamSelect() =>
        Wait.Until(driver =>
            {
                var awayTeamSelect = driver.FindElement(By.Id("NewGameDialog.AwayTeamSelect"));
                return (awayTeamSelect.Displayed, awayTeamSelect);
            },
            awayTeamSelect => awayTeamSelect.Click()
        );

    public void SelectAwayTeam(string teamName) =>
        Wait.Until(driver =>
            {
                var team = driver.FindElement(By.XPath($"//*[@id=\"NewGameDialog.AwayTeamSelect.List\"]//*[text()=\"{teamName}\"]"));
                return (team.Displayed, team);
            },
            team => team.Click()
        );

    public void ClickCreateButton() =>
        Wait.Until(driver =>
            {
                var createButton = driver.FindElement(By.Id("NewGameDialog.CreateButton"));
                return (createButton.Displayed, createButton);
            },
            createButton => createButton.Click()
        );

    public string GetGameName() =>
        Wait.Until(driver =>
            {
                var gameNameInput = driver.FindElement(By.Id("NewGameDialog.GameName"));
                return (gameNameInput.Displayed, gameNameInput);
            },
            gameNameInput => gameNameInput.GetAttribute("value") ?? "");
}
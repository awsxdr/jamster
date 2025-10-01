using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class GamesPageInteractor(IWebDriver driver) : Interactor(driver)
{
    public record GamePageGame(string Name, string HomeTeam, string AwayTeam, string Status);

    public void OpenAddGameDialog() =>
        Wait.Until(driver =>
            {
                var newGameButton = driver.FindElement(By.Id("GamesManagement.NewGameButton"));
                return (newGameButton.Displayed, newGameButton);
            },
            newGameButton => newGameButton.Click()
        );

    public GamePageGame GetGame(string gameName) =>
        Wait.Until(driver =>
        {
            var game = driver.FindElements(By.XPath($"//*[@id=\"GamesManagement.GamesTable\"]/tbody/tr[./td/a[text()=\"{gameName}\"]]/td"));

            return new GamePageGame(game[1].Text, game[2].Text, game[3].Text, game[4].Text);
        });
}
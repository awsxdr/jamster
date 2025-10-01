using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class TeamsPageInteractor(IWebDriver driver) : Interactor(driver)
{
    public record TeamPageTeam(string TeamName, string LeagueName, DateTime LastModified);

    public void OpenAddTeamDialog() =>
        Wait.Until(driver =>
            {
                var addTeamButton = driver.FindElement(By.Id("TeamsManagement.AddTeamButton"));
                return (addTeamButton.Displayed, addTeamButton);
            },
            addTeamButton => addTeamButton.Click()
        );

    public TeamPageTeam GetTeam(string teamName) => 
        Wait.Until(driver =>
            {
                var team = driver.FindElements(By.XPath($"//*[@id=\"TeamTable\"]/tbody/tr[./td/a[text()=\"{teamName}\"]]/td"));

                return new TeamPageTeam(team[1].Text, team[2].Text, DateTime.TryParse(team[3].Text, out var modified) ? modified : DateTime.MinValue);
            });

    public void ClickTeam(string teamName) =>
        Wait.Until(driver =>
            {
                var team = driver.FindElement(By.XPath($"//*[@id=\"TeamTable\"]/tbody/tr/td/a[text()=\"{teamName}\"]"));
                return (team.Displayed, team);
            },
            team => team.Click()
        );
}
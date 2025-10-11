using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class AddTeamDialogInteractor(IWebDriver driver) : Interactor(driver)
{
    public void SetTeamName(string teamName) =>
        Wait.Until(driver =>
            {
                var teamNameInput = driver.FindElement(By.Id("NewTeamDialog.TeamName"));

                return (teamNameInput.Displayed, teamNameInput);
            },
            teamNameInput =>
            {
                teamNameInput.SendKeys(teamName);
                teamNameInput.SendKeys(Keys.Tab);
            });

    public void ValidateTeamName(string expectedName) =>
        Wait.Until(driver =>
        {
            var teamNameInput = driver.FindElement(By.Id("NewTeamDialog.TeamName"));

            return teamNameInput.Displayed && teamNameInput.GetAttribute("value") == expectedName;
        });

    public void SetKitColor(string color) => Wait.Until(driver =>
        {
            var colorInput = driver.FindElement(By.Id("NewTeamDialog.KitColor"));

            return (colorInput.Displayed, colorInput);
        },
        colorInput =>
        {
            colorInput.SendKeys(color);
            colorInput.SendKeys(Keys.Tab);
        });

    public void ValidateKitName(string expectedName) =>
        Wait.Until(driver =>
        {
            var teamNameInput = driver.FindElement(By.Id("NewTeamDialog.KitColor"));

            return teamNameInput.Displayed && teamNameInput.GetAttribute("value") == expectedName;
        });

    public void ClickCreate() => Wait.Until(driver =>
        {
            var createButton = driver.FindElement(By.Id("NewTeamDialog.CreateButton"));

            return (createButton.Displayed, createButton);
        },
        createButton => createButton.Click()
    );
}
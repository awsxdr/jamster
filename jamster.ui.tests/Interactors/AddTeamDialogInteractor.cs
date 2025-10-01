using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class AddTeamDialogInteractor(IWebDriver driver) : Interactor(driver)
{
    public void SetTeamName(string teamName) => Wait.Until(driver =>
    {
        var teamNameInput = driver.FindElement(By.Id("NewTeamDialog.TeamName"));

        var result = teamNameInput.Displayed;

        if(result)
            teamNameInput.SendKeys(teamName);

        return result;
    });

    public void SetKitColor(string color) => Wait.Until(driver =>
    {
        var colorInput = driver.FindElement(By.Id("NewTeamDialog.KitColor"));

        var result = colorInput.Displayed;

        if (result)
            colorInput.SendKeys(color);

        return result;
    });

    public void ClickCreate() => Wait.Until(driver =>
    {
        var createButton = driver.FindElement(By.Id("NewTeamDialog.CreateButton"));

        var result = createButton.Displayed;

        if(result)
            createButton.Click();

        return result;
    });
}
using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class PenaltyDialogInteractor(IWebDriver driver) : Interactor(driver)
{
    public void ClickPenalty(string penaltyCode) =>
        Wait.Until(driver =>
            {
                var penaltyButton = driver.FindElement(By.Id($"PenaltyLineup.PenaltyDialog.Penalty.{penaltyCode}"));

                return (penaltyButton.Displayed, penaltyButton);
            },
            penaltyButton => penaltyButton.Click());
}
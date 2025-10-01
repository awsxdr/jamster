using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace jamster.ui.tests.Interactors;

public abstract class Interactor
{
    protected WebDriverWait Wait { get; }

    protected Interactor(IWebDriver driver)
    {
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
    }
}
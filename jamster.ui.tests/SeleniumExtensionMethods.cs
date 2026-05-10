using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace jamster.ui.tests;

public static class SeleniumExtensionMethods
{
    public static void Until(this WebDriverWait wait, Func<IWebDriver, bool> until, Action then) =>
        wait.Until(until,
            () =>
            {
                then();
                return new object();
            });

    public static void Until<TPassThrough>(this WebDriverWait wait, Func<IWebDriver, (bool, TPassThrough)> until, Action<TPassThrough> then) =>
        wait.Until(until,
            e =>
            {
                then(e);
                return new object();
            });

    public static TResult Until<TResult>(this WebDriverWait wait, Func<IWebDriver, bool> until, Func<TResult> then) where TResult : class => 
        wait.Until(driver =>
        {
            var result = until(driver);

            if (result)
                return then();

            return null;
        });

    public static TResult Until<TResult, TPassThrough>(this WebDriverWait wait, Func<IWebDriver, (bool, TPassThrough)> until, Func<TPassThrough, TResult> then) where TResult : class =>
        wait.Until(driver =>
        {
            var (success, passThrough) = until(driver);

            if (success)
                return then(passThrough);

            return null;
        });
}
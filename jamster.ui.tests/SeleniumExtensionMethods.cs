using System.Diagnostics;

using OpenQA.Selenium;

namespace jamster.ui.tests;

public static class SeleniumExtensionMethods
{
    public static Task WaitForVisibleAsync(this IWebElement? element, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        if (element == null)
            return Task.CompletedTask;

        return Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout && !element.Displayed)
            {
                await Task.Delay(100);
            }
        });
    }

    public static void WaitForVisible(this IWebElement? element, TimeSpan? timeout = null) =>
        element.WaitForVisibleAsync(timeout).Wait();
}
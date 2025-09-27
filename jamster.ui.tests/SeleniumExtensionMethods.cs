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

    public static Task WaitForNotVisibleAsync(this IWebElement? element, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        if (element == null)
            return Task.CompletedTask;

        return Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout && element.Displayed)
            {
                await Task.Delay(100);
            }
        });
    }

    public static void WaitForNotVisible(this IWebElement? element, TimeSpan? timeout = null)
    {
        try
        {
            element.WaitForNotVisibleAsync(timeout).Wait();
        }
        catch (AggregateException ex)
        {
            if (ex.InnerExceptions.Count == 1)
                throw ex.InnerExceptions[0];

            throw;
        }
    }
}

public static class SeleniumHelpers
{
    public static async Task RetryOnStale(Func<Task> action)
    {
        var retries = 0;

        while (true)
        {
            try
            {
                await action();
            }
            catch (StaleElementReferenceException)
            {
                if (++retries > 5)
                    throw;

                Thread.Sleep(TimeSpan.FromSeconds(.1));
            }
        }
    }

    public static TResult RetryOnStale<TResult>(Func<TResult> action)
    {
        var retries = 0;

        while(true)
        {
            try
            {
                return action();
            }
            catch (StaleElementReferenceException)
            {
                if(++retries > 5)
                    throw;

                Thread.Sleep(TimeSpan.FromSeconds(.1));
            }
        }
    }

    public static void RetryOnStale(Action action) =>
        RetryOnStale(() =>
        {
            action();
            return 0;
        });
}
using System.Diagnostics;
using System.Linq.Expressions;

using Moq;

namespace jamster.ui.tests;

public static class MoqExtensionMethods
{
    public static void WaitVerify<TMock, TResult>(this Mock<TMock> mock, Expression<Func<TMock, TResult>> method, Times times, TimeSpan? timeout = null)
        where TMock : class
    {
        var stopwatch = Stopwatch.StartNew();
        timeout ??= TimeSpan.FromSeconds(5);

        while (true)
        {
            try
            {
                mock.Verify(method, times);

                return;
            }
            catch (MockException)
            {
                if (stopwatch.Elapsed > timeout)
                    throw;

                Thread.Sleep(10);
            }
        }
    }

    public static void WaitVerify<TMock, TResult>(this Mock<TMock> mock, Expression<Func<TMock, TResult>> method,
        Func<Times> times, TimeSpan? timeout = null)
        where TMock : class
        =>
            mock.WaitVerify(method, times(), timeout);
}
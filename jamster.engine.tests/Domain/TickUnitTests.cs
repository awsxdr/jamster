using FluentAssertions;

using jamster.engine.Domain;

namespace jamster.engine.tests.Domain;

[TestFixture]
public class TickUnitTests
{
    [Test]
    public void AddTick_AddsCorrectly()
    {
        Tick tick1 = 1234;
        Tick tick2 = 4321;
        (tick1 + tick2).Should().Be((Tick)5555);
    }

    [Test]
    public void AddLong_WhenTickLeftParam_AddsCorrectly()
    {
        Tick tick = 1234;
        (tick + 2000L).Should().Be((Tick)3234);
    }

    [Test]
    public void AddLong_WhenTickRightParam_AddsCorrectly()
    {
        Tick tick = 1234;
        (tick - 200L).Should().Be((Tick)1034);
    }

    [Test]
    public void ComparisonsWorkAsExpected()
    {
        // ReSharper disable EqualExpressionComparison

        ((Tick)999 <= (Tick)1000).Should().BeTrue();
        ((Tick)1000 <= (Tick)1000).Should().BeTrue();
        ((Tick)1001 <= (Tick)1000).Should().BeFalse();

        ((Tick)999 < (Tick)1000).Should().BeTrue();
        ((Tick)1000 < (Tick)1000).Should().BeFalse();
        ((Tick)1001 < (Tick)1000).Should().BeFalse();

        ((Tick)999 >= (Tick)1000).Should().BeFalse();
        ((Tick)1000 >= (Tick)1000).Should().BeTrue();
        ((Tick)1001 >= (Tick)1000).Should().BeTrue();

        ((Tick)999 > (Tick)1000).Should().BeFalse();
        ((Tick)1000 > (Tick)1000).Should().BeFalse();
        ((Tick)1001 > (Tick)1000).Should().BeTrue();

        (((Tick)999) == ((Tick)1000)).Should().BeFalse();
        (((Tick)1000) == ((Tick)1000)).Should().BeTrue();

        (((Tick)999) != ((Tick)1000)).Should().BeTrue();
        (((Tick)1000) != ((Tick)1000)).Should().BeFalse();

        // ReSharper restore EqualExpressionComparison
    }
}
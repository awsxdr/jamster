using jamster.Domain;
using FluentAssertions;

namespace jamster.engine.tests;

[TestFixture]
public class TickUnitTests
{
    [Test]
    public void EqualityWithOtherTickWorksAsExpected()
    {
        var value = (Tick) 12345L;
        var secondValue = (Tick) 12345L;
        var notEqualValue = (Tick) 54321L;
        var longValue = 12345L;

        (value == secondValue).Should().BeTrue();
        (value != secondValue).Should().BeFalse();
        (value == notEqualValue).Should().BeFalse();
        (value != notEqualValue).Should().BeTrue();
        (value == longValue).Should().BeTrue();
        (value != longValue).Should().BeFalse();
        (longValue == value).Should().BeTrue();
        (longValue != value).Should().BeFalse();
        value.Equals(secondValue).Should().BeTrue();
        value.Equals(notEqualValue).Should().BeFalse();
        value.Equals(longValue).Should().BeTrue();
        longValue.Equals(value).Should().BeTrue();
    }
}
using amethyst.Services;
using FluentAssertions;
using static amethyst.tests.DataGenerator;

namespace amethyst.tests;

[TestFixture]
public class Guid7UnitTests
{
    [Test, Repeat(1)]
    public void GuidFromLong_CorrectlyStoresTicks()
    {
        var tick = GetRandomTick();

        var guid = (Guid7) tick;

        guid.Tick.Should().Be(tick);
    }
}
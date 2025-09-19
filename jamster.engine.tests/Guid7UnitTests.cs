using jamster.Services;
using FluentAssertions;
using static jamster.engine.tests.DataGenerator;

namespace jamster.engine.tests;

[TestFixture]
public class Guid7UnitTests
{
    [Test, Repeat(1000)]
    public void GuidFromLong_CorrectlyStoresTicks()
    {
        var tick = GetRandomTick();

        var guid = (Guid7) tick;

        guid.Tick.Should().Be(tick);
    }

    [Test]
    public void Guid_CanBeCompared()
    {
        Enumerable.Range(0, 10)
            .Select(i => Guid7.FromTick(9 - i))
            .OrderBy(g => g)
            .Select(g => g.Tick)
            .Should().BeEquivalentTo(Enumerable.Range(0, 10));
    }

    [Test]
    public void Guid7_FromLong_DoesNotResultInCollisions()
    {
        Enumerable.Range(0, 1_000_000)
            .Select(_ => Guid7.FromTick(1234))
            .Distinct()
            .Should().HaveCount(1_000_000);
    }
}
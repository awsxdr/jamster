using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class JamLineupUnitTests : ReducerUnitTest<HomeTeamJamLineup, JamLineupState>
{
    [Test]
    public async Task SkaterOnTrack_WithPivot_SetsPivotAccordingly([Values] bool initialPivotNumberIsNull, [Values] bool pivotNumberIsNull)
    {
        string? initialPivotId = initialPivotNumberIsNull ? null : "123";
        string? pivotNumber = pivotNumberIsNull ? null : "321";

        var jammerNumber = "333";

        State = new(jammerNumber, initialPivotId);

        _ = await Subject.Handle(new SkaterOnTrack(0, new (TeamSide.Home, pivotNumber, SkaterPosition.Pivot)));

        State.PivotNumber.Should().Be(pivotNumber);
        State.JammerNumber.Should().Be(jammerNumber);
    }

    [Test]
    public async Task SkaterOnTrack_WithJammer_SetsPivotAccordingly([Values] bool initialJammerNumberIsNull, [Values] bool jammerNumberIsNull)
    {
        string? initialJammerNumber = initialJammerNumberIsNull ? null : "123";
        string? jammerNumber = jammerNumberIsNull ? null : "321";

        var pivotNumber = "333";

        State = new(initialJammerNumber, pivotNumber);

        _ = await Subject.Handle(new SkaterOnTrack(0, new (TeamSide.Home, jammerNumber, SkaterPosition.Jammer)));

        State.PivotNumber.Should().Be(pivotNumber);
        State.JammerNumber.Should().Be(jammerNumber);
    }

    [Test]
    public async Task SkaterOnTrack_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new("123", "321");

        _ = await Subject.Handle(new SkaterOnTrack(0, new (TeamSide.Away, "666", SkaterPosition.Jammer)));

        State.Should().Be(new JamLineupState("123", "321"));
    }

    [Test]
    public async Task SkaterOnTrack_WhenAddedJammerIsAlreadySetAsPivot_ClearsPivot()
    {
        State = new(null, "123");

        _ = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, "123", SkaterPosition.Jammer)));

        State.Should().Be(new JamLineupState("123", null));
    }

    [Test]
    public async Task SkaterOnTrack_WhenAddedPivotIsAlreadySetAsJammer_ClearsJammer()
    {
        State = new("123", null);

        _ = await Subject.Handle(new SkaterOnTrack(0, new(TeamSide.Home, "123", SkaterPosition.Pivot)));

        State.Should().Be(new JamLineupState(null, "123"));
    }

    [Test]
    public async Task JamEnded_ClearsLineup()
    {
        State = new("123", "321");

        _ = await Subject.Handle(new JamEnded(0));

        State.Should().Be(new JamLineupState(null, null));
    }
}
using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using FluentAssertions;

namespace jamster.engine.tests.Reducers;

public class PenaltyBoxUnitTests : ReducerUnitTest<HomePenaltyBox, PenaltyBoxState>
{
    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterNotAlreadyInBox_AddsSkaterToBox()
    {
        State = new(["321"], []);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, "123")));

        State.Should().Be(new PenaltyBoxState(["321", "123"], []));
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterAlreadyInBox_DoesNotChangeState()
    {
        State = new(["123", "321"], []);
        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new(["321"], []);
        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterInQueue_RemovesSkaterFromQueue()
    {
        State = new(["321"], ["123"]);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, "123")));

        State.Should().Be(new PenaltyBoxState(["321", "123"], []));
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamMatches_AndSkaterInBox_RemovesSkaterFromBox()
    {
        State = new(["123", "321"], []);

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, "123")));

        State.Should().Be(new PenaltyBoxState(["321"], []));
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamMatches_AndSkaterNotInBox_DoesNotChangeState()
    {
        State = new(["321"], []);
        var originalState = State;

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new(["123", "321"], []);
        var originalState = State;

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSubstitutedInBox_ReplacesTargetSkater()
    {
        State = new(["123", "321"], []);

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, "123", "555")));

        State.Should().Be(new PenaltyBoxState(["555", "321"], []));
    }

    [Test]
    public async Task SkaterSubstitutedInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new(["123", "321"], []);
        var originalState = State;

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Away, "123", "555")));

        State.Should().Be(originalState);
    }
}
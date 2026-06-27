using AwesomeAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class PenaltyBoxUnitTests : ReducerUnitTest<HomePenaltyBox, PenaltyBoxState>
{
    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterNotAlreadyInBox_AddsSkaterToBox()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterInBoxId], []);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterId)));

        State.Should().Be(new PenaltyBoxState([skaterInBoxId, skaterId], []));
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterAlreadyInBox_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterId, skaterInBoxId], []);
        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterId)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterInBoxId], []);
        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Away, skaterId)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamMatches_AndSkaterInQueue_RemovesSkaterFromQueue()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterInBoxId], [skaterId]);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, skaterId)));

        State.Should().Be(new PenaltyBoxState([skaterInBoxId, skaterId], []));
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamMatches_AndSkaterInBox_RemovesSkaterFromBox()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterId, skaterInBoxId], []);

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, skaterId)));

        State.Should().Be(new PenaltyBoxState([skaterInBoxId], []));
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamMatches_AndSkaterNotInBox_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterInBoxId], []);
        var originalState = State;

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Home, skaterId)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterReleasedFromBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();

        State = new([skaterId, skaterInBoxId], []);
        var originalState = State;

        await Subject.Handle(new SkaterReleasedFromBox(0, new(TeamSide.Away, skaterId)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSubstitutedInBox_ReplacesTargetSkater()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();
        var substituteSkaterId = Guid.NewGuid();

        State = new([skaterId, skaterInBoxId], []);

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Home, skaterId, substituteSkaterId)));

        State.Should().Be(new PenaltyBoxState([substituteSkaterId, skaterInBoxId], []));
    }

    [Test]
    public async Task SkaterSubstitutedInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var skaterId = Guid.NewGuid();
        var skaterInBoxId = Guid.NewGuid();
        var substituteSkaterId = Guid.NewGuid();

        State = new([skaterId, skaterInBoxId], []);
        var originalState = State;

        await Subject.Handle(new SkaterSubstitutedInBox(0, new(TeamSide.Away, skaterId, substituteSkaterId)));

        State.Should().Be(originalState);
    }
}
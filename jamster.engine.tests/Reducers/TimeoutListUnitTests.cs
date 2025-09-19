using jamster.Domain;
using jamster.Events;
using jamster.Reducers;
using FluentAssertions;

namespace jamster.engine.tests.Reducers;

public class TimeoutListUnitTests : ReducerUnitTest<TimeoutList, TimeoutListState>
{
    [Test]
    public async Task TimeoutStarted_AddsTimeoutToList()
    {
        var @event = new TimeoutStarted(0);
        MockState<GameStageState>(new(Stage.Lineup, 2, 5, 19, false));

        await Subject.Handle(@event);

        State.Timeouts.Should().HaveCount(1)
            .And.Subject.Single().Should().Be(new TimeoutListItem(@event.Id, TimeoutType.Untyped, 2, 5, null, null, false));
    }

    [Test]
    public async Task TimeoutTypeSet_SetsTypeOfLastTimeout()
    {
        State = new([
            new TimeoutListItem(0, TimeoutType.Team, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(1, TimeoutType.Review, 1, 2, TeamSide.Away, 95, true),
            new TimeoutListItem(2, TimeoutType.Team, 1, 3, TeamSide.Away, null, false),
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TimeoutTypeSet(0, new(TimeoutType.Review, TeamSide.Home)));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Team, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(eventIds[1], TimeoutType.Review, 1, 2, TeamSide.Away, 95, true),
            new TimeoutListItem(eventIds[2], TimeoutType.Review, 1, 3, TeamSide.Home, null, false)
        ]);
    }

    [Test]
    public async Task TimeoutEnded_SetsDurationOfLastTimeout()
    {
        State = new([
            new(0, TimeoutType.Team, 1, 1, TeamSide.Home, null, false)
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TimeoutEnded(10000));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Team, 1, 1, TeamSide.Home, 10, false)
        ]);
    }

    [Test]
    public async Task TimeoutStarted_WhenLastTimeoutIsMissingDuration_SetsDurationOfLastTimeout()
    {
        State = new([
            new(0, TimeoutType.Team, 1, 1, TeamSide.Home, null, false)
        ]);
        MockState<GameStageState>(new(Stage.Lineup, 2, 5, 19, false));

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        var startEvent = new TimeoutStarted(10000);

        await Subject.Handle(startEvent);

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Team, 1, 1, TeamSide.Home, 10, false),
            new(startEvent.Id, TimeoutType.Untyped, 2, 5, null, null, false),
        ]);
    }

    [Test]
    public async Task TimeoutStarted_WhenLastTimeoutHasDuration_DoesNotSetDurationOfLastTimeout()
    {
        State = new([
            new(0, TimeoutType.Team, 1, 1, TeamSide.Home, 12, false)
        ]);
        MockState<GameStageState>(new(Stage.Lineup, 2, 5, 19, false));

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        var startEvent = new TimeoutStarted(10000);

        await Subject.Handle(startEvent);

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Team, 1, 1, TeamSide.Home, 12, false),
            new(startEvent.Id, TimeoutType.Untyped, 2, 5, null, null, false),
        ]);
    }

    [Test]
    public async Task TeamReviewRetained_SetsSpecifiedReviewAsRetained()
    {
        State = new([
            new TimeoutListItem(0, TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(1, TimeoutType.Review, 1, 1, TeamSide.Away, 95, false),
            new TimeoutListItem(2, TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TeamReviewRetained(0, new(TeamSide.Away, eventIds[1])));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(eventIds[1], TimeoutType.Review, 1, 1, TeamSide.Away, 95, true),
            new TimeoutListItem(eventIds[2], TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);
    }

    [Test]
    public async Task TeamReviewRetained_WhenIdNotFound_DoesNotChangeState()
    {
        State = new([
            new TimeoutListItem(0, TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(1, TimeoutType.Review, 1, 1, TeamSide.Away, 95, false),
            new TimeoutListItem(2, TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TeamReviewRetained(0, new(TeamSide.Away, Guid.NewGuid())));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(eventIds[1], TimeoutType.Review, 1, 1, TeamSide.Away, 95, false),
            new TimeoutListItem(eventIds[2], TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);
    }

    [Test]
    public async Task TeamReviewLost_SetsSpecifiedReviewAsLost()
    {
        State = new([
            new TimeoutListItem(0, TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(1, TimeoutType.Review, 1, 1, TeamSide.Away, 95, true),
            new TimeoutListItem(2, TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TeamReviewLost(0, new(TeamSide.Away, eventIds[1])));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(eventIds[1], TimeoutType.Review, 1, 1, TeamSide.Away, 95, false),
            new TimeoutListItem(eventIds[2], TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);
    }

    [Test]
    public async Task TeamReviewLost_WhenIdNotFound_DoesNotChangeState()
    {
        State = new([
            new TimeoutListItem(0, TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(1, TimeoutType.Review, 1, 1, TeamSide.Away, 95, true),
            new TimeoutListItem(2, TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);

        var eventIds = State.Timeouts.Select(t => t.EventId).ToArray();

        await Subject.Handle(new TeamReviewLost(0, new(TeamSide.Away, Guid.NewGuid())));

        State.Timeouts.Should().BeEquivalentTo([
            new TimeoutListItem(eventIds[0], TimeoutType.Review, 1, 1, TeamSide.Home, 61, false),
            new TimeoutListItem(eventIds[1], TimeoutType.Review, 1, 1, TeamSide.Away, 95, true),
            new TimeoutListItem(eventIds[2], TimeoutType.Review, 1, 1, TeamSide.Away, null, false),
        ]);
    }
}
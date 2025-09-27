using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests.Reducers;

public class TeamTimeoutsUnitTests : ReducerUnitTest<HomeTeamTimeouts, TeamTimeoutsState>
{
    [TestCaseSource(nameof(TimeoutTypeSetTestCases))]
    public async Task TimeoutTypeSet_UpdatesStateAsExpected(
        TeamTimeoutsState initialState, 
        TimeoutTypeSetBody eventBody,
        TeamTimeoutsState expectedState)
    {
        State = initialState;

        await Subject.Handle(new TimeoutTypeSet(0, eventBody));

        State.Should().Be(expectedState);
    }

    [TestCase(ReviewStatus.Unused)]
    [TestCase(ReviewStatus.Retained)]
    [TestCase(ReviewStatus.Used)]
    public async Task PeriodFinalized_ResetsReviewUsage(ReviewStatus status)
    {
        State = new(3, status, TimeoutInUse.None);
        MockState<RulesState>(new(Rules.DefaultRules));

        await Subject.Handle(new PeriodFinalized(0));

        State.ReviewStatus.Should().Be(ReviewStatus.Unused);
    }

    [Test]
    public async Task PeriodFinalized_WhenRulesSayToDoSo_ResetsTimeoutUsage()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.None);
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            TimeoutRules = Rules.DefaultRules.TimeoutRules with
            {
                ResetBehavior = TimeoutResetBehavior.Period
            }
        }));

        await Subject.Handle(new PeriodFinalized(0));

        State.NumberTaken.Should().Be(0);
    }

    [Test]
    public async Task TimeoutEnded_WhenTimeoutRunning_SetsCurrentTimeoutToNone()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Timeout);

        await Subject.Handle(new TimeoutEnded(0));

        State.CurrentTimeout.Should().Be(TimeoutInUse.None);
    }

    [Test]
    public async Task TimeoutEnded_WhenNoTimeoutRunning_ShouldNotChangeState()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.None);

        var initialState = State;

        await Subject.Handle(new TimeoutEnded(0));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task TimeoutEnded_WhenReviewInProgress_ShouldChargeForReview()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Review);

        await Subject.Handle(new TimeoutEnded(0));

        State.ReviewStatus.Should().Be(ReviewStatus.Used);
    }

    [Test]
    public async Task TimeoutStarted_WhenTimeoutRunning_SetsCurrentTimeoutToNone()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Timeout);

        await Subject.Handle(new TimeoutStarted(0));

        State.CurrentTimeout.Should().Be(TimeoutInUse.None);
    }

    [Test]
    public async Task TimeoutStarted_WhenNoTimeoutRunning_ShouldNotChangeState()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.None);

        var initialState = State;

        await Subject.Handle(new TimeoutStarted(0));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task TimeoutStarted_WhenReviewInProgress_ShouldChargeForReview()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Review);

        await Subject.Handle(new TimeoutStarted(0));

        State.ReviewStatus.Should().Be(ReviewStatus.Used);
    }

    [Test]
    public async Task JamStarted_WhenTimeoutRunning_SetsCurrentTimeoutToNone()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Timeout);

        await Subject.Handle(new JamStarted(0));

        State.CurrentTimeout.Should().Be(TimeoutInUse.None);
    }

    [Test]
    public async Task JamStarted_WhenNoTimeoutRunning_ShouldNotChangeState()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.None);

        var initialState = State;

        await Subject.Handle(new JamStarted(0));

        State.Should().Be(initialState);
    }

    [Test]
    public async Task JamStarted_WhenReviewInProgress_ShouldChargeForReview()
    {
        State = new(3, ReviewStatus.Unused, TimeoutInUse.Review);

        await Subject.Handle(new JamStarted(0));

        State.ReviewStatus.Should().Be(ReviewStatus.Used);
    }

    [Test]
    public async Task TeamReviewRetained_SetsReviewStatusToRetained()
    {
        State = new(3, ReviewStatus.Used, TimeoutInUse.None);

        await Subject.Handle(new TeamReviewRetained(0, new(TeamSide.Home, Guid7.Empty)));

        State.ReviewStatus.Should().Be(ReviewStatus.Retained);
    }

    [Test]
    public async Task TeamReviewLost_SetsReviewStatusToUsed()
    {
        State = new(3, ReviewStatus.Retained, TimeoutInUse.None);

        await Subject.Handle(new TeamReviewLost(0, new(TeamSide.Home, Guid7.Empty)));

        State.ReviewStatus.Should().Be(ReviewStatus.Used);
    }

    public static IEnumerable<TestCaseData> TimeoutTypeSetTestCases =>
    [
        new TestCaseData(
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home),
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout))
            .SetName("Team timeout for this team when none running sets type and increments taken"),
        new TestCaseData(
                new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home),
                new TeamTimeoutsState(2, ReviewStatus.Unused, TimeoutInUse.Timeout))
            .SetName("Team timeout for this team when timeout already running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Home),
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout))
            .SetName("Team timeout for this team when review running changes review to timeout and does not charge for it"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Away),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Team timeout for other team when none running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Away),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Team timeout for other team when timeout already running refunds timeout"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Team, TeamSide.Away),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Team timeout for other team when review running stops review and does not charge for it"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review))
            .SetName("Review for this team when none running sets type"),
        new TestCaseData(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.Review))
            .SetName("Review for this team when timeout running changes timeout to review and refunds timeout"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Home),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review))
            .SetName("Review for this team when review already running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Away),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Review for other team when none running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Away),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Review for other team when timeout running stops and refunds timeout"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Review, TeamSide.Away),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Review for other team when review running stops review and does not charge for it"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Official, null),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Official timeout when none running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Official, null),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Official timeout when timeout running stops and refunds timeout"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Official, null),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Official timeout when review running stops review and does not charge for it"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None),
                new TimeoutTypeSetBody(TimeoutType.Untyped, null),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Untyped timeout when none running does not change state"),
        new TestCaseData(
                new TeamTimeoutsState(1, ReviewStatus.Unused, TimeoutInUse.Timeout),
                new TimeoutTypeSetBody(TimeoutType.Untyped, null),
                new TeamTimeoutsState(0, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Untyped timeout when timeout running stops and refunds timeout"),
        new TestCaseData(
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.Review),
                new TimeoutTypeSetBody(TimeoutType.Untyped, null),
                new TeamTimeoutsState(3, ReviewStatus.Unused, TimeoutInUse.None))
            .SetName("Untyped timeout when review running stops review and does not charge for it"),
    ];
}
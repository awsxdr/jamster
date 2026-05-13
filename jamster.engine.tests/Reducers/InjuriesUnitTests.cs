using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class InjuriesUnitTests : ReducerUnitTest<HomeInjuries, InjuriesState>
{
    [Test]
    public async Task SkaterInjuryAdded_WhenNotDuplicate_AddsNewInjury()
    {
        var existingInjurySkater = Guid.NewGuid();
        var newInjurySkater = Guid.NewGuid();

        State = new([new(existingInjurySkater, 1, 2, 2, true)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false, false));

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Home, newInjurySkater)));

        State.Injuries.Should().HaveCount(2).And
            .BeEquivalentTo([
                new Injury(existingInjurySkater, 1, 2, 2, true),
                new Injury(newInjurySkater, 2, 12, 30, false)
            ]);
    }

    [Test]
    public async Task SkaterInjuryAdded_WhenDuplicate_DoesNotChangeState()
    {
        var existingInjurySkater = Guid.NewGuid();
        var newInjurySkater = Guid.NewGuid();

        State = new([new(existingInjurySkater, 1, 2, 2, true), new(newInjurySkater, 2, 12, 30, false)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false, false));

        var originalState = State;

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Home, newInjurySkater)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryAdded_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var existingInjurySkater = Guid.NewGuid();
        var newInjurySkater = Guid.NewGuid();

        State = new([new(existingInjurySkater, 1, 2, 2, true)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false, false));

        var originalState = State;

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Away, newInjurySkater)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenExists_RemovesInjury()
    {
        var remainingInjurySkater = Guid.NewGuid();
        var removedInjurySkater = Guid.NewGuid();

        State = new([new(remainingInjurySkater, 1, 2, 2, true), new(removedInjurySkater, 2, 12, 30, false)]);

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Home, removedInjurySkater, 30)));

        State.Injuries.Should().BeEquivalentTo([
            new Injury(remainingInjurySkater, 1, 2, 2, true)
        ]);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenDoesNotExist_DoesNotChangeState()
    {
        var remainingInjurySkater = Guid.NewGuid();
        var removedInjurySkater = Guid.NewGuid();

        State = new([new(remainingInjurySkater, 1, 2, 2, true), new(removedInjurySkater, 2, 12, 30, false)]);

        var originalState = State;

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Home, removedInjurySkater, 31)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        var remainingInjurySkater = Guid.NewGuid();
        var removedInjurySkater = Guid.NewGuid();

        State = new([new(remainingInjurySkater, 1, 2, 2, true), new(removedInjurySkater, 2, 12, 30, false)]);

        var originalState = State;

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Away, removedInjurySkater, 30)));

        State.Should().Be(originalState);
    }

    [TestCase(0, 3, 2, 12, false)]
    [TestCase(0, 3, 2, 13, true)]
    [TestCase(0, 2, 2, 12, true)]
    [TestCase(1, 3, 2, 13, false)]
    [TestCase(1, 3, 3, 13, true)]
    [TestCase(2, 3, 3, 13, false)]
    public async Task JamEnded_ExpiresInjuriesAsAppropriate(int skaterIndex, int injuryDuration, int injuriesBeforeSittingOutPeriod, int jamNumber, bool expiryExpected)
    {
        var roster = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();

        State = new([
            new(roster[0], 1, 5, 5, true),
            new(roster[1], 1, 10, 10, true),
            new(roster[1], 2, 3, 23, true),
            new(roster[2], 2, 1, 21, true),
            new(roster[2], 2, 5, 25, true),
            new(roster[skaterIndex], 2, 10, 30, false)
        ]);
        MockState<GameStageState>(new(Stage.Lineup, 2, jamNumber, 20 + jamNumber, false, false));
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            InjuryRules = new(injuryDuration, injuriesBeforeSittingOutPeriod)
        }));

        await Subject.Handle(new JamEnded(0));

        State.Injuries.Last().Expired.Should().Be(expiryExpected);
    }

    [Test]
    public async Task PeriodFinalized_ExpiresInjuriesWhichArePastDuration()
    {
        var roster = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        State = new([
            new(roster[0], 1, 5, 5, true),
            new(roster[1], 1, 19, 19, false),
            new(roster[2], 1, 5, 5, true),
            new(roster[2], 1, 10, 10, false),
            new(roster[3], 1, 5, 5, true),
            new(roster[3], 1, 18, 18, false),
            new(roster[4], 1, 5, 5, true),
            new(roster[4], 1, 17, 17, false),
        ]);
        MockState<RulesState>(new(Rules.DefaultRules));
        MockState<GameStageState>(new(Stage.Intermission, 2, 0, 20, true, false));

        await Subject.Handle(new PeriodFinalized(0));

        State.Should().Be(new InjuriesState([
            new(roster[0], 1, 5, 5, true),
            new(roster[1], 1, 19, 19, false),
            new(roster[2], 1, 5, 5, true),
            new(roster[2], 1, 10, 10, true),
            new(roster[3], 1, 5, 5, true),
            new(roster[3], 1, 18, 18, false),
            new(roster[4], 1, 5, 5, true),
            new(roster[4], 1, 17, 17, true),
        ]));
    }
}
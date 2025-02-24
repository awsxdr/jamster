using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class InjuriesUnitTests : ReducerUnitTest<HomeInjuries, InjuriesState>
{
    [Test]
    public async Task SkaterInjuryAdded_WhenNotDuplicate_AddsNewInjury()
    {
        State = new([new("1", 1, 2, 2, true)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false));

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Home, "123")));

        State.Injuries.Should().HaveCount(2).And
            .BeEquivalentTo([
                new Injury("1", 1, 2, 2, true),
                new Injury("123", 2, 12, 30, false)
            ]);
    }

    [Test]
    public async Task SkaterInjuryAdded_WhenDuplicate_DoesNotChangeState()
    {
        State = new([new("1", 1, 2, 2, true), new("123", 2, 12, 30, false)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false));

        var originalState = State;

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Home, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryAdded_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([new("1", 1, 2, 2, true)]);
        MockState<GameStageState>(new(Stage.Timeout, 2, 12, 30, false));

        var originalState = State;

        await Subject.Handle(new SkaterInjuryAdded(0, new(TeamSide.Away, "123")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenExists_RemovesInjury()
    {
        State = new([new("1", 1, 2, 2, true), new("123", 2, 12, 30, false)]);

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Home, "123", 30)));

        State.Injuries.Should().BeEquivalentTo([
            new Injury("1", 1, 2, 2, true)
        ]);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenDoesNotExist_DoesNotChangeState()
    {
        State = new([new("1", 1, 2, 2, true), new("123", 2, 12, 30, false)]);

        var originalState = State;

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Home, "123", 31)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterInjuryRemoved_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([new("1", 1, 2, 2, true), new("123", 2, 12, 30, false)]);

        var originalState = State;

        await Subject.Handle(new SkaterInjuryRemoved(0, new(TeamSide.Away, "123", 30)));

        State.Should().Be(originalState);
    }

    [TestCase("1", 3, 2, 12, false)]
    [TestCase("1", 3, 2, 13, true)]
    [TestCase("1", 2, 2, 12, true)]
    [TestCase("2", 3, 2, 13, false)]
    [TestCase("2", 3, 3, 13, true)]
    [TestCase("3", 3, 3, 13, false)]
    public async Task JamEnded_ExpiresInjuriesAsAppropriate(string skaterNumber, int injuryDuration, int injuriesBeforeSittingOutPeriod, int jamNumber, bool expiryExpected)
    {
        State = new([
            new("1", 1, 5, 5, true),
            new("2", 1, 10, 10, true),
            new("2", 2, 3, 23, true),
            new("3", 2, 1, 21, true),
            new("3", 2, 5, 25, true),
            new(skaterNumber, 2, 10, 30, false)
        ]);
        MockState<GameStageState>(new(Stage.Lineup, 2, jamNumber, 20 + jamNumber, false));
        MockState<RulesState>(new(Rules.DefaultRules with
        {
            InjuryRules = new(injuryDuration, injuriesBeforeSittingOutPeriod)
        }));

        await Subject.Handle(new JamEnded(0));

        State.Injuries.Last().Expired.Should().Be(expiryExpected);
    }
}
using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class TeamJamStatsUnitTests : ReducerUnitTest<HomeTeamJamStats, TeamJamStatsState>
{
    [TestCase(false, TeamSide.Home, true, true)]
    [TestCase(false, TeamSide.Away, true, false)]
    [TestCase(true, TeamSide.Home, true, true)]
    [TestCase(true, TeamSide.Home, false, false)]
    [TestCase(true, TeamSide.Away, false, true)]
    [TestCase(false, TeamSide.Home, false, false)]
    public async Task LeadMarked_UpdatesStateAsExpected(bool initialLead, TeamSide side, bool lead, bool expectedLead)
    {
        State = State with { Lead = initialLead };

        await Subject.Handle(new LeadMarked(0, new(side, lead)));

        State.Lead.Should().Be(expectedLead);
    }

    [Test]
    public async Task LeadMarked_WhenTrueAndForOpponent_ClearsLead()
    {
        State = State with { Lead = true };

        await Subject.Handle(new LeadMarked(0, new(TeamSide.Away, true)));

        State.Lead.Should().Be(false);
    }

    [Test]
    public async Task LeadMarked_WhenFalseAndForOpponent_DoesNotClearLead()
    {
        State = State with { Lead = true };

        await Subject.Handle(new LeadMarked(0, new(TeamSide.Away, false)));

        State.Lead.Should().Be(true);
    }

    [TestCase(false, TeamSide.Home, true, true)]
    [TestCase(false, TeamSide.Away, true, false)]
    [TestCase(true, TeamSide.Home, true, true)]
    [TestCase(true, TeamSide.Home, false, false)]
    [TestCase(true, TeamSide.Away, false, true)]
    [TestCase(false, TeamSide.Home, false, false)]
    public async Task LostMarked_UpdatesStateAsExpected(bool initialLost, TeamSide side, bool lost, bool expectedLost)
    {
        State = State with { Lost = initialLost };

        await Subject.Handle(new LostMarked(0, new(side, lost)));

        State.Lost.Should().Be(expectedLost);
    }

    [TestCase(false, TeamSide.Home, true, true)]
    [TestCase(false, TeamSide.Away, true, false)]
    [TestCase(true, TeamSide.Home, true, true)]
    [TestCase(true, TeamSide.Home, false, false)]
    [TestCase(true, TeamSide.Away, false, true)]
    [TestCase(false, TeamSide.Home, false, false)]
    public async Task CallMarked_UpdatesStateAsExpected(bool initialCall, TeamSide side, bool call, bool expectedCall)
    {
        State = State with { Called = initialCall };

        await Subject.Handle(new CallMarked(0, new(side, call)));

        State.Called.Should().Be(expectedCall);
    }

    [TestCase(false, TeamSide.Home, true, true)]
    [TestCase(false, TeamSide.Away, true, false)]
    [TestCase(true, TeamSide.Home, true, true)]
    [TestCase(true, TeamSide.Home, false, false)]
    [TestCase(true, TeamSide.Away, false, true)]
    [TestCase(false, TeamSide.Home, false, false)]
    public async Task StarPassMarked_UpdatesStateAsExpected(bool initialStarPass, TeamSide side, bool starPass, bool expectedStarPass)
    {
        State = State with { StarPass = initialStarPass };

        await Subject.Handle(new StarPassMarked(0, new(side, starPass)));

        State.StarPass.Should().Be(expectedStarPass);
    }

    [TestCase(false, TeamSide.Home, true, true)]
    [TestCase(false, TeamSide.Away, true, false)]
    [TestCase(true, TeamSide.Home, true, true)]
    [TestCase(true, TeamSide.Home, false, false)]
    [TestCase(true, TeamSide.Away, false, true)]
    [TestCase(false, TeamSide.Home, false, false)]
    public async Task InitialTripCompleted_UpdatesStateAsExpected(bool initialCompleted, TeamSide side, bool completed, bool expectedCompleted)
    {
        State = State with { HasCompletedInitial = initialCompleted };

        await Subject.Handle(new InitialTripCompleted(0, new(side, completed)));

        State.HasCompletedInitial.Should().Be(expectedCompleted);
    }

    [Test]
    public async Task JamStarted_ResetsJamStats()
    {
        State = new(true, true, true, true, true);

        await Subject.Handle(new JamStarted(0));

        State.Should().Be(Subject.GetDefaultState());
    }
}
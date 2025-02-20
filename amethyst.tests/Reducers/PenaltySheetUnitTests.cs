using amethyst.Domain;
using amethyst.Events;
using amethyst.Reducers;
using FluentAssertions;

namespace amethyst.tests.Reducers;

public class PenaltySheetUnitTests : ReducerUnitTest<HomePenaltySheet, PenaltySheetState>
{
    [Test]
    public async Task TeamSet_AddsPenaltyLinesForSkaters()
    {
        State = new([
            new("1", [new("X", 1, 5, true)]),
            new("3", [new("P", 1, 5, true), new("I", 1, 5, true)]),
            new("5", [new("C", 1, 5, true)]),
        ]);

        await Subject.Handle(new TeamSet(0, new(TeamSide.Home, new(
            [],
            new(Color.Black, Color.White),
            [
                new("1", "Test Skater 1", true),
                new("2", "Test Skater 2", true),
                new("3", "Test Skater 3", true),
                new("4", "Test Skater 4", true),
                new("5", "Test Skater 5", true),
                new("6", "Test Skater 6", true),
                new("7", "Test Skater 7", true),
                new("8", "Test Skater 8", true),
                new("9", "Test Skater 9", true),
            ]
        ))));

        State.Should().Be(new PenaltySheetState([
            new("1", [new("X", 1, 5, true)]),
            new("2", []),
            new("3", [new("P", 1, 5, true), new("I", 1, 5, true)]),
            new("4", []),
            new("5", [new("C", 1, 5, true)]),
            new("6", []),
            new("7", []),
            new("8", []),
            new("9", []),
        ]));
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNumberKnown_AddsPenaltyForSkater()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "2", "P")));

        State.Should().Be(new PenaltySheetState([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("X", 1, 2, true), new("A", 1, 5, true), new("P", 1, 9, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNumberNotKnown_DoesNotChangeState()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        var originalState = State;

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, "5", "P")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyAssessed_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        var originalState = State;

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Away, "2", "P")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyKnown_RemovesPenalty()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, "2", "P", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, true)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyNotKnown_DoesNotChangeState()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        var originalState = State;

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, "5", "P", 1, 5)));
        
        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyRescinded_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false));

        var originalState = State;

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Away, "2", "P", 1, 5)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_MarksAllUnservedPenaltiesAsServedForSkater()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, false), new("I", 1, 5, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, "2")));

        State.Should().Be(new PenaltySheetState([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, true), new("I", 1, 5, true)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]));
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new("1", [new("X", 1, 3, true)]),
            new("2", [new("P", 1, 2, true), new("A", 1, 5, false), new("I", 1, 5, false)]),
            new("3", []),
            new("4", [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]);

        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Away, "2")));

        State.Should().Be(originalState);
    }
}
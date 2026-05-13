using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class PenaltySheetUnitTests : ReducerUnitTest<HomePenaltySheet, PenaltySheetState>
{
    private static Guid SkaterId(int n) => new(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    [Test]
    public async Task TeamSet_AddsPenaltyLinesForSkaters()
    {
        List<GameSkater> roster =
        [
            new(Guid.NewGuid(), "1", "Test Skater 1", true),
            new(Guid.NewGuid(), "2", "Test Skater 2", true),
            new(Guid.NewGuid(), "3", "Test Skater 3", true),
            new(Guid.NewGuid(), "4", "Test Skater 4", true),
            new(Guid.NewGuid(), "5", "Test Skater 5", true),
            new(Guid.NewGuid(), "6", "Test Skater 6", true),
            new(Guid.NewGuid(), "7", "Test Skater 7", true),
            new(Guid.NewGuid(), "8", "Test Skater 8", true),
            new(Guid.NewGuid(), "9", "Test Skater 9", true),
        ];

        State = new([
            new(roster[0].Id, roster[0].Number, null, [new("X", 1, 5, true)]),
            new(roster[2].Id, roster[2].Number, null, [new("P", 1, 5, true), new("I", 1, 5, true)]),
            new(roster[4].Id, roster[4].Number, null, [new("C", 1, 5, true)]),
        ]);

        await Subject.Handle(new TeamSet(0, new(TeamSide.Home, new(
            [],
            new(Color.Black, Color.White),
            roster
        ))));

        State.Should().Be(new PenaltySheetState([
            new(roster[0].Id, roster[0].Number, null, [new("X", 1, 5, true)]),
            new(roster[1].Id, roster[1].Number, null, []),
            new(roster[2].Id, roster[2].Number, null, [new("P", 1, 5, true), new("I", 1, 5, true)]),
            new(roster[3].Id, roster[3].Number, null, []),
            new(roster[4].Id, roster[4].Number, null, [new("C", 1, 5, true)]),
            new(roster[5].Id, roster[5].Number, null, []),
            new(roster[6].Id, roster[6].Number, null, []),
            new(roster[7].Id, roster[7].Number, null, []),
            new(roster[8].Id, roster[8].Number, null, []),
        ]));
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNumberKnown_AddsPenaltyForSkater()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, SkaterId(2), "P")));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("X", 1, 2, true), new("A", 1, 5, true), new("P", 1, 9, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]));
    }

    [TestCase(1, 2, 2)]
    [TestCase(2, 3, 3)]
    [TestCase(1, 0, 1)]
    [TestCase(2, 0, 1)]
    public async Task PenaltyAssessed_WhenSkaterNumberKnown_AddsPenaltyInCorrectJam(int period, int jam, int expectedJam)
    {
        State = new([new(SkaterId(123), "123", null, [])]);
        MockState<GameStageState>(new(Stage.Lineup, period, jam, (period - 1) * 20 + jam, false, false));

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, SkaterId(123), "X")));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [new("X", period, expectedJam, false)])
        ]));
    }

    [Test]
    public async Task PenaltyAssessed_WhenSkaterNumberNotKnown_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        var originalState = State;

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Home, SkaterId(5), "P")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyAssessed_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("X", 1, 2, true), new("A", 1, 5, true)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        var originalState = State;

        await Subject.Handle(new PenaltyAssessed(0, new(TeamSide.Away, SkaterId(2), "P")));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyKnown_RemovesPenalty()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, SkaterId(2), "P", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, true)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyRescinded_WhenMultiplePenaltiesMatch_OnlyRemovesASinglePenalty()
    {
        State = new([
            new(SkaterId(123), "123", null, [
                new("X", 1, 1, false),
                new("X", 1, 1, false),
                new("X", 1, 1, false),
            ])
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, SkaterId(123), "X", 1, 1)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [
                new("X", 1, 1, false),
                new("X", 1, 1, false),
            ])
        ]));
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyNotKnown_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        var originalState = State;

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, SkaterId(5), "P", 1, 5)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyRescinded_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, true), new("P", 1, 5, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 7, true)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 9, 9, false, false));

        var originalState = State;

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Away, SkaterId(2), "P", 1, 5)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyIsExpulsionPenalty_RemovesExpulsionPenalty()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, false), [new("C", 1, 3, true), new("B", 1, 5, true), new("X", 1, 5, false)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 5, 5, false, false));

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [new("C", 1, 3, true), new("B", 1, 5, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyRescinded_WhenPenaltyIsExpulsionPenalty_ButMultiplePenaltiesMatchExpulsionPenalty_DoesNotRemoveExpulsionPenalty()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, false), [new("C", 1, 3, true), new("X", 1, 5, false), new("X", 1, 5, false)]),
        ]);
        MockState<GameStageState>(new(Stage.Jam, 1, 5, 5, false, false));

        await Subject.Handle(new PenaltyRescinded(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("X", 1, 5, false), [new("C", 1, 3, true), new("X", 1, 5, false)]),
        ]));
    }

    [Test]
    public async Task PenaltyUpdated_WhenPenaltyKnown_ChangesPenaltyAndSortsList()
    {
        State = new([
            new(SkaterId(123), "123", null, [
                new("A", 1, 5, true),
                new("B", 1, 7, true),
                new("C", 1, 9, true),
                new("D", 2, 2, true),
                new("E", 2, 3, true),
                new("F", 2, 3, true),
            ]),
        ]);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "F", 2, 3, "X", 1, 10)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [
                new("A", 1, 5, true),
                new("B", 1, 7, true),
                new("C", 1, 9, true),
                new("X", 1, 10, true),
                new("D", 2, 2, true),
                new("E", 2, 3, true),
            ]),
        ]));
    }

    [Test]
    public async Task PenaltyUpdated_WithDuplicatePenalties_OnlyUpdatesASinglePenalty()
    {
        State = new([
            new(SkaterId(123), "123", null, [
                new("X", 1, 3, true),
                new("X", 1, 3, true),
                new("A", 1, 5, false),
                new("A", 1, 5, false),
                new("A", 1, 5, false),
            ])
        ]);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "A", 1, 5, "X", 1, 6)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [
                new("X", 1, 3, true),
                new("X", 1, 3, true),
                new("A", 1, 5, false),
                new("A", 1, 5, false),
                new("X", 1, 6, false),
            ])
        ]));
    }

    [Test]
    public async Task PenaltyUpdated_WhenPenaltyNotKnown_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [
                new("A", 1, 5, true),
                new("B", 1, 7, true),
                new("C", 1, 9, true),
                new("D", 2, 2, true),
                new("E", 2, 3, true),
                new("F", 2, 3, true),
            ]),
            new(SkaterId(2), "2", null, []),
        ]);

        var originalState = State;

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "X", 2, 3, "?", 1, 10)));

        State.Should().Be(originalState);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "F", 2, 2, "?", 1, 10)));

        State.Should().Be(originalState);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(2), "F", 2, 3, "?", 1, 10)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyUpdated_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [
                new("A", 1, 5, true),
                new("B", 1, 7, true),
                new("C", 1, 9, true),
                new("D", 2, 2, true),
                new("E", 2, 3, true),
                new("F", 2, 3, true),
            ]),
            new(SkaterId(2), "2", null, []),
        ]);

        var originalState = State;

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Away, SkaterId(123), "F", 2, 3, "?", 1, 10)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyUpdated_WhenPenaltyIsExpulsionPenalty_UpdatesExpulsionPenalty()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5, "B", 2, 3)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("B", 2, 3, true), [new("B", 2, 3, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyUpdated_WhenPenaltyIsExpulsionPenalty_ButMultiplePenaltiesMatchExpulsionPenalty_DoesNotChangeExpulsionPenalty()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true), new("X", 1, 5, true)]),
        ]);

        await Subject.Handle(new PenaltyUpdated(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5, "B", 2, 3)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true), new("B", 2, 3, true)]),
        ]));
    }

    [Test]
    public async Task SkaterExpelled_WhenPenaltyMatches_AddsExpulsion()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, true)]),
        ]);

        await Subject.Handle(new SkaterExpelled(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]));
    }

    [Test]
    public async Task SkaterExpelled_WhenPenaltyDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, true)]),
        ]);

        var originalState = State;

        await Subject.Handle(new SkaterExpelled(0, new(TeamSide.Home, SkaterId(123), "X", 1, 6)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterExpelled_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, true)]),
        ]);

        var originalState = State;

        await Subject.Handle(new SkaterExpelled(0, new(TeamSide.Away, SkaterId(123), "X", 1, 5)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterExpelled_WhenExpulsionAlreadySet_ReplacesExpulsion()
    {
        State = new([
            new(SkaterId(123), "123", new("B", 1, 3, true), [new("B", 1, 3, true), new("X", 1, 5, true)]),
        ]);

        await Subject.Handle(new SkaterExpelled(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("B", 1, 3, true), new("X", 1, 5, true)]),
        ]));
    }

    [Test]
    public async Task ExpulsionCleared_WhenExpulsionExists_ClearsExpulsion()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]);

        await Subject.Handle(new ExpulsionCleared(0, new(TeamSide.Home, SkaterId(123))));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", null, [new("X", 1, 5, true)]),
        ]));
    }

    [Test]
    public async Task ExpulsionCleared_WhenExpulsionDoesNotExist_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, true)]),
        ]);

        var originalState = State;

        await Subject.Handle(new ExpulsionCleared(0, new(TeamSide.Home, SkaterId(123))));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task ExpulsionCleared_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]);

        var originalState = State;

        await Subject.Handle(new ExpulsionCleared(0, new(TeamSide.Away, SkaterId(123))));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_MarksAllUnservedPenaltiesAsServedForSkater()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, false), new("I", 1, 5, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, SkaterId(2))));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, true), new("I", 1, 5, true)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]));
    }

    [Test]
    public async Task SkaterSatInBox_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(1), "1", null, [new("X", 1, 3, true)]),
            new(SkaterId(2), "2", null, [new("P", 1, 2, true), new("A", 1, 5, false), new("I", 1, 5, false)]),
            new(SkaterId(3), "3", null, []),
            new(SkaterId(4), "4", null, [new("P", 1, 1, true), new("E", 1, 5, false)]),
        ]);

        var originalState = State;

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Away, SkaterId(2))));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task SkaterSatInBox_SetsExpulsionPenaltyAsSat()
    {
        State = new([
            new(SkaterId(123), "123", new("X", 1, 5, false), [new("X", 1, 5, false)]),
        ]);

        await Subject.Handle(new SkaterSatInBox(0, new(TeamSide.Home, SkaterId(123))));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(123), "123", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]));
    }

    [Test]
    public async Task PenaltyServedSet_WhenPenaltyFound_SetsPenaltyAsExpected([Values] bool served)
    {
        State = new([
            new(SkaterId(321), "321", null, [new("X", 1, 5, !served)]),
            new(SkaterId(123), "123", null, [new("X", 1, 5, !served), new("X", 1, 5, !served), new("I", 1, 5, !served), new("B", 1, 8, !served)])
        ]);

        await Subject.Handle(new PenaltyServedSet(0, new(TeamSide.Home, SkaterId(123), "X", 1, 5, served)));

        State.Should().Be(new PenaltySheetState([
            new(SkaterId(321), "321", null, [new("X", 1, 5, !served)]),
            new(SkaterId(123), "123", null, [new("X", 1, 5, !served), new("I", 1, 5, !served), new("X", 1, 5, served), new("B", 1, 8, !served)])
        ]));
    }

    [Test]
    public async Task PenaltyServedSet_WhenPenaltyNotFound_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, false)])
        ]);

        var originalState = State;

        await Subject.Handle(new PenaltyServedSet(0, new(TeamSide.Home, SkaterId(123), "F", 1, 5, true)));

        State.Should().Be(originalState);
    }

    [Test]
    public async Task PenaltyServedSet_WhenTeamDoesNotMatch_DoesNotChangeState()
    {
        State = new([
            new(SkaterId(123), "123", null, [new("X", 1, 5, false)])
        ]);

        var originalState = State;

        await Subject.Handle(new PenaltyServedSet(0, new(TeamSide.Away, SkaterId(123), "X", 1, 5, true)));

        State.Should().Be(originalState);
    }
}


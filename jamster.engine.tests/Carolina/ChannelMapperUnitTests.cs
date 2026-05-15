using FluentAssertions;

using jamster.engine.Carolina;
using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests.Carolina;

public class ChannelMapperUnitTests : UnitTest<ChannelMapper, IChannelMapper>
{
    [TestCase(Stage.Jam, true)]
    [TestCase(Stage.Lineup, false)]
    [TestCase(Stage.Timeout, false)]
    [TestCase(Stage.AfterTimeout, false)]
    [TestCase(Stage.Intermission, false)]
    [TestCase(Stage.BeforeGame, false)]
    [TestCase(Stage.AfterGame, false)]
    public void Map_WithGameStageState_ShouldCorrectlySetInJam(Stage stage, bool expected)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { Stage = stage };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).InJam")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(1)]
    [TestCase(2)]
    public void Map_WithGameStageState_ShouldCorrectlySetCurrentPeriodNumber(int periodNumber)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { PeriodNumber = periodNumber };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).CurrentPeriodNumber")
            .WhoseValue.Should().Be(periodNumber);
    }

    [TestCase(1)]
    [TestCase(14)]
    public void Map_WithGameStageState_ShouldCorrectlySetJamNumber(int jamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { JamNumber = jamNumber };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Number")
            .WhoseValue.Should().Be(jamNumber);
    }

    [TestCase(1)]
    [TestCase(2)]
    public void Map_WithGameStageState_ShouldCorrectlySetPeriodClockNumber(int periodNumber)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { PeriodNumber = periodNumber };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Number")
            .WhoseValue.Should().Be(periodNumber);
    }

    [TestCase(1)]
    [TestCase(14)]
    public void Map_WithGameStageState_ShouldCorrectlySetLineupClockNumber(int jamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { JamNumber = jamNumber };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Number")
            .WhoseValue.Should().Be(jamNumber);
    }

    [TestCase(Stage.Intermission, 1, 1)]
    [TestCase(Stage.Intermission, 2, 2)]
    [TestCase(Stage.AfterGame, 1, 2)]
    [TestCase(Stage.AfterGame, 2, 3)]
    public void Map_WithGameStageState_ShouldCorrectlySetIntermissionClockNumber(Stage stage, int periodNumber, int expectedNumber)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { Stage = stage, PeriodNumber = periodNumber };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Number")
            .WhoseValue.Should().Be(expectedNumber);
    }

    [TestCase(Stage.BeforeGame, false, "Prepared")]
    [TestCase(Stage.Lineup, false, "Running")]
    [TestCase(Stage.Jam, false, "Running")]
    [TestCase(Stage.Timeout, false, "Running")]
    [TestCase(Stage.AfterTimeout, false, "Running")]
    [TestCase(Stage.Intermission, false, "Running")]
    [TestCase(Stage.Intermission, true, "Running")]
    [TestCase(Stage.AfterGame, false, "Running")]
    [TestCase(Stage.AfterGame, true, "Finished")]
    public void Map_WithGameStageState_ShouldCorrectlySetState(Stage stage, bool periodFinalized, string expected)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { Stage = stage, PeriodIsFinalized = periodFinalized };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).State")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(Stage.Lineup, true)]
    [TestCase(Stage.Jam, true)]
    [TestCase(Stage.Timeout, true)]
    [TestCase(Stage.AfterTimeout, true)]
    [TestCase(Stage.BeforeGame, false)]
    [TestCase(Stage.Intermission, false)]
    [TestCase(Stage.AfterGame, false)]
    public void Map_WithGameStageState_ShouldCorrectlySetInPeriod(Stage stage, bool expected)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { Stage = stage };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).InPeriod")
            .WhoseValue.Should().Be(expected);
    }

    [Test]
    public void Map_WithGameStageState_ShouldCorrectlySetNoMoreJam([Values] bool nextJamShouldStart)
    {
        var gameId = Guid.NewGuid();
        var state = GameStageState.Default with { Stage = Stage.Lineup, NextJamShouldStart = nextJamShouldStart };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).NoMoreJam")
            .WhoseValue.Should().Be(!nextJamShouldStart);
    }

    [Test]
    public void Map_WithOvertimeState_ShouldCorrectlySetInOvertime([Values] bool isInOvertime)
    {
        var gameId = Guid.NewGuid();
        var state = new OvertimeState(isInOvertime);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).InOvertime")
            .WhoseValue.Should().Be(isInOvertime);
    }

    [Test]
    public void Map_WithJamClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(isRunning, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(0, 0)]
    [TestCase(30, 30000)]
    [TestCase(45, 45000)]
    public void Map_WithJamClockState_ShouldCorrectlySetTime(int secondsPassed, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, Tick.FromSeconds(secondsPassed), true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithJamClockState_ShouldSetDirectionToCountDown()
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Direction")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithJamClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Id")
            .WhoseValue.Should().Be($"{gameId}_Jam");
    }

    [Test]
    public void Map_WithJamClockState_ShouldSetNameToJam()
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Name")
            .WhoseValue.Should().Be("Jam");
    }

    [Test]
    public void Map_WithJamClockState_ShouldSetMaximumTimeToJamDuration()
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).MaximumTime")
            .WhoseValue.Should().Be(120000);
    }

    [TestCase(0, 120000)]
    [TestCase(30, 90000)]
    [TestCase(90, 30000)]
    public void Map_WithJamClockState_ShouldSetInvertedTime(int secondsPassed, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, Tick.FromSeconds(secondsPassed), true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithJamClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new JamClockState(false, 0, 0, true, false);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Jam).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithLineupClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(isRunning, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(0, 0)]
    [TestCase(25, 25000)]
    [TestCase(30, 30000)]
    public void Map_WithLineupClockState_ShouldCorrectlySetTime(int secondsPassed, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, Tick.FromSeconds(secondsPassed));

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithLineupClockState_ShouldSetDirectionToCountUp()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Direction")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithLineupClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Id")
            .WhoseValue.Should().Be($"{gameId}_Lineup");
    }

    [Test]
    public void Map_WithLineupClockState_ShouldSetNameToLineup()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Name")
            .WhoseValue.Should().Be("Lineup");
    }

    [Test]
    public void Map_WithLineupClockState_ShouldSetMaximumTimeTo24Hours()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).MaximumTime")
            .WhoseValue.Should().Be(86400000);
    }

    [TestCase(0, 86400000)]
    [TestCase(10, 86390000)]
    [TestCase(25, 86375000)]
    public void Map_WithLineupClockState_ShouldSetInvertedTime(int secondsPassed, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, Tick.FromSeconds(secondsPassed));

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithLineupClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupClockState(false, 0, 0);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Lineup).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = TimeoutClockState.Default with { IsRunning = isRunning };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(0, 0)]
    [TestCase(60, 60000)]
    [TestCase(90, 90000)]
    public void Map_WithTimeoutClockState_ShouldCorrectlySetTime(int secondsPassed, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = TimeoutClockState.Default with { TicksPassed = Tick.FromSeconds(secondsPassed) };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldSetDirectionToCountUp()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TimeoutClockState.Default, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Direction")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TimeoutClockState.Default, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Id")
            .WhoseValue.Should().Be($"{gameId}_Timeout");
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldSetNameToTimeout()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TimeoutClockState.Default, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Name")
            .WhoseValue.Should().Be("Timeout");
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldSetMaximumTimeTo24Hours()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TimeoutClockState.Default, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).MaximumTime")
            .WhoseValue.Should().Be(86400000);
    }

    [TestCase(0, 86400000)]
    [TestCase(30, 86370000)]
    [TestCase(60, 86340000)]
    public void Map_WithTimeoutClockState_ShouldSetInvertedTime(int secondsPassed, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = TimeoutClockState.Default with { TicksPassed = Tick.FromSeconds(secondsPassed) };

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithTimeoutClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TimeoutClockState.Default, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(isRunning, false, true, 0, 0, 0);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(0, 1800, 1800000)]
    [TestCase(30, 1800, 1770000)]
    [TestCase(1800, 1800, 0)]
    public void Map_WithPeriodClockState_ShouldCorrectlySetTime(int secondsPassed, int periodDurationSeconds, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, Tick.FromSeconds(secondsPassed));
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldSetDirectionToCountDown()
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, 0);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Direction")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, 0);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Id")
            .WhoseValue.Should().Be($"{gameId}_Period");
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldSetNameToPeriod()
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, 0);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Name")
            .WhoseValue.Should().Be("Period");
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldSetMaximumTimeToPeriodDuration()
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, 0);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).MaximumTime")
            .WhoseValue.Should().Be(1800000);
    }

    [TestCase(0, 0)]
    [TestCase(30, 30000)]
    [TestCase(1800, 1800000)]
    public void Map_WithPeriodClockState_ShouldSetInvertedTime(int secondsPassed, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, Tick.FromSeconds(secondsPassed));

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithPeriodClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new PeriodClockState(false, false, true, 0, 0, 0);

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Period).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(isRunning, false, 0, 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(900, 900000)]
    [TestCase(30, 30000)]
    [TestCase(0, 0)]
    public void Map_WithIntermissionClockState_ShouldCorrectlySetTime(int secondsRemaining, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, 0, 0, secondsRemaining);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldSetDirectionToCountDown()
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, 0, 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Direction")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, 0, 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Id")
            .WhoseValue.Should().Be($"{gameId}_Intermission");
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldSetNameToIntermission()
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, 0, 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Name")
            .WhoseValue.Should().Be("Intermission");
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldSetMaximumTimeToInitialDuration()
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, Tick.FromSeconds(900), 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).MaximumTime")
            .WhoseValue.Should().Be(900000);
    }

    [TestCase(900, 0)]
    [TestCase(600, 300000)]
    [TestCase(0, 900000)]
    public void Map_WithIntermissionClockState_ShouldSetInvertedTime(int secondsRemaining, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, Tick.FromSeconds(900), 0, secondsRemaining);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithIntermissionClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new IntermissionClockState(false, false, 0, 0, 900);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldCorrectlySetRunning([Values] bool isRunning)
    {
        var gameId = Guid.NewGuid();
        var state = PostGameClockState.Default with { IsRunning = isRunning };

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Running")
            .WhoseValue.Should().Be(isRunning);
    }

    [TestCase(0, 0)]
    [TestCase(30, 30000)]
    [TestCase(900, 900000)]
    public void Map_WithPostGameClockState_ShouldCorrectlySetTime(int secondsPassed, int expectedMilliseconds)
    {
        var gameId = Guid.NewGuid();
        var state = PostGameClockState.Default with { TicksPassed = Tick.FromSeconds(secondsPassed) };

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Time")
            .WhoseValue.Should().Be(expectedMilliseconds);
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldSetDirectionToCountUp()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(PostGameClockState.Default, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Direction")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldSetId()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(PostGameClockState.Default, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Id")
            .WhoseValue.Should().Be($"{gameId}_Intermission");
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldSetNameToIntermission()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(PostGameClockState.Default, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Name")
            .WhoseValue.Should().Be("Intermission");
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldSetMaximumTimeToIntermissionDuration()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(PostGameClockState.Default, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).MaximumTime")
            .WhoseValue.Should().Be(900000);
    }

    [TestCase(0, 900000)]
    [TestCase(30, 870000)]
    [TestCase(900, 0)]
    public void Map_WithPostGameClockState_ShouldSetInvertedTime(int secondsPassed, int expectedInvertedTime)
    {
        var gameId = Guid.NewGuid();
        var state = PostGameClockState.Default with { TicksPassed = Tick.FromSeconds(secondsPassed) };

        var result = Subject.Map(state, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).InvertedTime")
            .WhoseValue.Should().Be(expectedInvertedTime);
    }

    [Test]
    public void Map_WithPostGameClockState_ShouldSetReadonlyToFalse()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(PostGameClockState.Default, Rules.DefaultRules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Intermission).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 42, 1)]
    [TestCase(TeamSide.Away, 35, 2)]
    public void Map_WithTeamScoreState_ShouldCorrectlySetScore(TeamSide side, int score, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamScoreState(score, 0);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Score")
            .WhoseValue.Should().Be(score);
    }

    [TestCase(TeamSide.Home, 7, 1)]
    [TestCase(TeamSide.Away, 4, 2)]
    public void Map_WithTeamScoreState_ShouldCorrectlySetJamScore(TeamSide side, int jamScore, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamScoreState(0, jamScore);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).JamScore")
            .WhoseValue.Should().Be(jamScore);
    }

    [TestCase(TeamSide.Home, 42, 7, 35, 1)]
    [TestCase(TeamSide.Away, 100, 4, 96, 2)]
    public void Map_WithTeamScoreState_ShouldSetLastScoreToScoreMinusJamScore(TeamSide side, int score, int jamScore, int expectedLastScore, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamScoreState(score, jamScore);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).LastScore")
            .WhoseValue.Should().Be(expectedLastScore);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamScoreState_ShouldSetActiveScoreAdjustmentAmountToZero(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamScoreState(10, 4);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).ActiveScoreAdjustmentAmount")
            .WhoseValue.Should().Be(0);
    }

    [TestCase(TeamSide.Home, 3, 1)]
    [TestCase(TeamSide.Away, 4, 2)]
    public void Map_WithTripScoreState_ShouldCorrectlySetTripScore(TeamSide side, int score, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TripScoreState(score, 0, 0);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).TripScore")
            .WhoseValue.Should().Be(score);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTripScoreState_WhenScoreIsNull_ShouldSetTripScoreToZero(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TripScoreState(null, 0, 0);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).TripScore")
            .WhoseValue.Should().Be(0);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTripScoreState_ShouldSetCurrentTrip(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TripScoreState(3, 1, 0);

        var result = Subject.Map(state, side, 1, 5, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).CurrentTrip")
            .WhoseValue.Should().NotBeNull();
    }

    [Test]
    public void Map_WithTripScoreState_CurrentTripChangesWhenTripCountChanges()
    {
        var gameId = Guid.NewGuid();

        var result1 = Subject.Map(new TripScoreState(3, 1, 0), TeamSide.Home, 1, 5, gameId);
        var result2 = Subject.Map(new TripScoreState(0, 2, 0), TeamSide.Home, 1, 5, gameId);

        result1[$"ScoreBoard.Game({gameId}).Team(1).CurrentTrip"]
            .Should().NotBe(result2[$"ScoreBoard.Game({gameId}).Team(1).CurrentTrip"]);
    }

    [Test]
    public void Map_WithTripScoreState_CurrentTripIsStable()
    {
        var gameId = Guid.NewGuid();
        var state = new TripScoreState(3, 1, 0);

        var result1 = Subject.Map(state, TeamSide.Home, 1, 5, gameId);
        var result2 = Subject.Map(state, TeamSide.Home, 1, 5, gameId);

        result1[$"ScoreBoard.Game({gameId}).Team(1).CurrentTrip"]
            .Should().Be(result2[$"ScoreBoard.Game({gameId}).Team(1).CurrentTrip"]);
    }

    [TestCase(TeamSide.Home, true, 1)]
    [TestCase(TeamSide.Home, false, 1)]
    [TestCase(TeamSide.Away, true, 2)]
    [TestCase(TeamSide.Away, false, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetLead(TeamSide side, bool lead, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(lead, false, false, false, false);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Lead")
            .WhoseValue.Should().Be(lead);
    }

    [TestCase(TeamSide.Home, true, 1)]
    [TestCase(TeamSide.Home, false, 1)]
    [TestCase(TeamSide.Away, true, 2)]
    [TestCase(TeamSide.Away, false, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetLost(TeamSide side, bool lost, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(false, lost, false, false, false);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Lost")
            .WhoseValue.Should().Be(lost);
    }

    [TestCase(TeamSide.Home, true, 1)]
    [TestCase(TeamSide.Home, false, 1)]
    [TestCase(TeamSide.Away, true, 2)]
    [TestCase(TeamSide.Away, false, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetCalloff(TeamSide side, bool called, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(false, false, called, false, false);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Calloff")
            .WhoseValue.Should().Be(called);
    }

    [TestCase(TeamSide.Home, true, 1)]
    [TestCase(TeamSide.Home, false, 1)]
    [TestCase(TeamSide.Away, true, 2)]
    [TestCase(TeamSide.Away, false, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetStarPass(TeamSide side, bool starPass, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(false, false, false, starPass, false);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).StarPass")
            .WhoseValue.Should().Be(starPass);
    }

    [TestCase(TeamSide.Home, true, false, 1)]
    [TestCase(TeamSide.Home, false, true, 1)]
    [TestCase(TeamSide.Away, true, false, 2)]
    [TestCase(TeamSide.Away, false, true, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetNoInitial(TeamSide side, bool hasCompletedInitial, bool expectedNoInitial, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(false, false, false, false, hasCompletedInitial);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).NoInitial")
            .WhoseValue.Should().Be(expectedNoInitial);
    }

    [TestCase(TeamSide.Home, true, false, true, 1)]
    [TestCase(TeamSide.Home, true, true, false, 1)]
    [TestCase(TeamSide.Home, false, false, false, 1)]
    [TestCase(TeamSide.Away, true, false, true, 2)]
    [TestCase(TeamSide.Away, true, true, false, 2)]
    [TestCase(TeamSide.Away, false, false, false, 2)]
    public void Map_WithTeamJamStatsState_ShouldCorrectlySetDisplayLead(TeamSide side, bool lead, bool lost, bool expectedDisplayLead, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamJamStatsState(lead, lost, false, false, false);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).DisplayLead")
            .WhoseValue.Should().Be(expectedDisplayLead);
    }

    [TestCase(TeamSide.Home, 0, 3, 1)]
    [TestCase(TeamSide.Home, 2, 1, 1)]
    [TestCase(TeamSide.Away, 1, 2, 2)]
    public void Map_WithTeamTimeoutsState_ShouldCorrectlySetTimeouts(TeamSide side, int numberTaken, int expectedRemaining, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamTimeoutsState(numberTaken, ReviewStatus.Unused, TimeoutInUse.None);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, side, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Timeouts")
            .WhoseValue.Should().Be(expectedRemaining);
    }

    [TestCase(TeamSide.Home, ReviewStatus.Unused, 1, 1)]
    [TestCase(TeamSide.Home, ReviewStatus.Retained, 1, 1)]
    [TestCase(TeamSide.Home, ReviewStatus.Used, 0, 1)]
    [TestCase(TeamSide.Away, ReviewStatus.Unused, 1, 2)]
    [TestCase(TeamSide.Away, ReviewStatus.Retained, 1, 2)]
    [TestCase(TeamSide.Away, ReviewStatus.Used, 0, 2)]
    public void Map_WithTeamTimeoutsState_ShouldCorrectlySetOfficialReviews(TeamSide side, ReviewStatus reviewStatus, int expectedReviews, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamTimeoutsState(0, reviewStatus, TimeoutInUse.None);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, side, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).OfficialReviews")
            .WhoseValue.Should().Be(expectedReviews);
    }

    [TestCase(TeamSide.Home, ReviewStatus.Retained, true, 1)]
    [TestCase(TeamSide.Home, ReviewStatus.Unused, false, 1)]
    [TestCase(TeamSide.Home, ReviewStatus.Used, false, 1)]
    [TestCase(TeamSide.Away, ReviewStatus.Retained, true, 2)]
    [TestCase(TeamSide.Away, ReviewStatus.Unused, false, 2)]
    [TestCase(TeamSide.Away, ReviewStatus.Used, false, 2)]
    public void Map_WithTeamTimeoutsState_ShouldCorrectlySetRetainedOfficialReview(TeamSide side, ReviewStatus reviewStatus, bool expectedRetained, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamTimeoutsState(0, reviewStatus, TimeoutInUse.None);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, side, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).RetainedOfficialReview")
            .WhoseValue.Should().Be(expectedRetained);
    }

    [TestCase(TeamSide.Home, TimeoutInUse.Timeout, true, 1)]
    [TestCase(TeamSide.Home, TimeoutInUse.None, false, 1)]
    [TestCase(TeamSide.Home, TimeoutInUse.Review, false, 1)]
    [TestCase(TeamSide.Away, TimeoutInUse.Timeout, true, 2)]
    [TestCase(TeamSide.Away, TimeoutInUse.None, false, 2)]
    [TestCase(TeamSide.Away, TimeoutInUse.Review, false, 2)]
    public void Map_WithTeamTimeoutsState_ShouldCorrectlySetInTimeout(TeamSide side, TimeoutInUse currentTimeout, bool expectedInTimeout, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamTimeoutsState(0, ReviewStatus.Unused, currentTimeout);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, side, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).InTimeout")
            .WhoseValue.Should().Be(expectedInTimeout);
    }

    [TestCase(TeamSide.Home, TimeoutInUse.Review, true, 1)]
    [TestCase(TeamSide.Home, TimeoutInUse.None, false, 1)]
    [TestCase(TeamSide.Home, TimeoutInUse.Timeout, false, 1)]
    [TestCase(TeamSide.Away, TimeoutInUse.Review, true, 2)]
    [TestCase(TeamSide.Away, TimeoutInUse.None, false, 2)]
    [TestCase(TeamSide.Away, TimeoutInUse.Timeout, false, 2)]
    public void Map_WithTeamTimeoutsState_ShouldCorrectlySetInOfficialReview(TeamSide side, TimeoutInUse currentTimeout, bool expectedInOfficialReview, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamTimeoutsState(0, ReviewStatus.Unused, currentTimeout);
        var rules = Rules.DefaultRules;

        var result = Subject.Map(state, side, rules, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).InOfficialReview")
            .WhoseValue.Should().Be(expectedInOfficialReview);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetJammerRosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var jammerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(jammerId, null, [null, null, null]);
        var teamDetails = TeamDetailsWithSkater(jammerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenJammerAbsent_ShouldSetJammerRosterNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).RosterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetPivotRosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var pivotId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, pivotId, [null, null, null]);
        var teamDetails = TeamDetailsWithSkater(pivotId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Pivot).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenPivotAbsent_ShouldSetPivotRosterNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Pivot).RosterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker1RosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [blockerId, null, null]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker1).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker1Absent_ShouldSetBlocker1RosterNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker1).RosterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker2RosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [null, blockerId, null]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker2).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker2Absent_ShouldSetBlocker2RosterNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker2).RosterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker3RosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [null, null, blockerId]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker3).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker3Absent_ShouldSetBlocker3RosterNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker3).RosterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetJammerName(TeamSide side, int expectedTeamNumber)
    {
        var jammerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(jammerId, null, [null, null, null]);
        var teamDetails = TeamDetailsWithSkater(jammerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenJammerAbsent_ShouldSetJammerNameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetPivotName(TeamSide side, int expectedTeamNumber)
    {
        var pivotId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, pivotId, [null, null, null]);
        var teamDetails = TeamDetailsWithSkater(pivotId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Pivot).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenPivotAbsent_ShouldSetPivotNameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Pivot).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker1Name(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [blockerId, null, null]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker1).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker1Absent_ShouldSetBlocker1NameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker1).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker2Name(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [null, blockerId, null]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker2).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker2Absent_ShouldSetBlocker2NameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker2).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldCorrectlySetBlocker3Name(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [null, null, blockerId]);
        var teamDetails = TeamDetailsWithSkater(blockerId, "101", "Test Skater");

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker3).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenBlocker3Absent_ShouldSetBlocker3NameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker3).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TimeoutType.Untyped, null, "")]
    [TestCase(TimeoutType.Official, null, "O")]
    [TestCase(TimeoutType.Team, TeamSide.Home, "1")]
    [TestCase(TimeoutType.Team, TeamSide.Away, "2")]
    [TestCase(TimeoutType.Review, TeamSide.Home, "1")]
    [TestCase(TimeoutType.Review, TeamSide.Away, "2")]
    public void Map_WithCurrentTimeoutTypeState_ShouldCorrectlySetTimeoutOwner(TimeoutType type, TeamSide? teamSide, string expected)
    {
        var gameId = Guid.NewGuid();
        var state = new CurrentTimeoutTypeState(type, teamSide);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).TimeoutOwner")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(TimeoutType.Review, true)]
    [TestCase(TimeoutType.Team, false)]
    [TestCase(TimeoutType.Official, false)]
    [TestCase(TimeoutType.Untyped, false)]
    public void Map_WithCurrentTimeoutTypeState_ShouldCorrectlySetOfficialReview(TimeoutType type, bool expected)
    {
        var gameId = Guid.NewGuid();
        var state = new CurrentTimeoutTypeState(type, null);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).OfficialReview")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetName(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["team"] = "Test Team" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Name")
            .WhoseValue.Should().Be("Test Team");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_WhenNamesIsEmpty_ShouldSetNameToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Name")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetTeamName(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["team"] = "Riveters" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).TeamName")
            .WhoseValue.Should().Be("Riveters");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenTeamNameAbsent_ShouldSetTeamNameToNull()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).TeamName")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetLeagueName(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["league"] = "Rose City Rollers" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).LeagueName")
            .WhoseValue.Should().Be("Rose City Rollers");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenLeagueNameAbsent_ShouldSetLeagueNameToNull()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).LeagueName")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_WhenBothLeagueAndTeamNamePresent_ShouldConcatenateFullName(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["league"] = "Rose City Rollers", ["team"] = "Riveters" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).FullName")
            .WhoseValue.Should().Be("Rose City Rollers Riveters");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenOnlyLeagueNamePresent_ShouldSetFullNameToLeagueName()
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["league"] = "Rose City Rollers" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).FullName")
            .WhoseValue.Should().Be("Rose City Rollers");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenOnlyTeamNamePresent_ShouldSetFullNameToTeamName()
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["team"] = "Riveters" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).FullName")
            .WhoseValue.Should().Be("Riveters");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenNeitherLeagueNorTeamNamePresent_ShouldSetFullNameToNull()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).FullName")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetUniformColor(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["color"] = "White" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).UniformColor")
            .WhoseValue.Should().Be("White");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetSkaterName(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            [],
            new TeamColor(Color.Black, Color.White),
            [new GameSkater(skaterId, "101", "Test Skater", true)]
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).Name")
            .WhoseValue.Should().Be("Test Skater");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldCorrectlySetSkaterRosterNumber(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            [],
            new TeamColor(Color.Black, Color.White),
            [new GameSkater(skaterId, "101", "Test Skater", true)]
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).RosterNumber")
            .WhoseValue.Should().Be("101");
    }

    [Test]
    public void Map_WithTeamDetailsState_ShouldEmitChannelsForAllRosterSkaters()
    {
        var skater1Id = Guid.NewGuid();
        var skater2Id = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            [],
            new TeamColor(Color.Black, Color.White),
            [
                new GameSkater(skater1Id, "101", "Skater One", true),
              new GameSkater(skater2Id, "202", "Skater Two", true),
            ]
        ));

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skater1Id}).Name");
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skater2Id}).Name");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldSetTeamId(TeamSide side, int expectedTeamNumber)
    {
        var teamId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(teamId, [], new TeamColor(Color.Black, Color.White), []));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Id")
            .WhoseValue.Should().Be(teamId);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldSetInitialsFromMultiWordLeagueName(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["league"] = "Rose City Rollers" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Initials")
            .WhoseValue.Should().Be("RCR");
    }

    [Test]
    public void Map_WithTeamDetailsState_ShouldSetInitialsFromSingleWordLeagueName()
    {
        var gameId = Guid.NewGuid();
        var state = new TeamDetailsState(new GameTeam(
            new() { ["league"] = "Riveters" },
            new TeamColor(Color.Black, Color.White),
            []
        ));

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Initials")
            .WhoseValue.Should().Be("R");
    }

    [Test]
    public void Map_WithTeamDetailsState_WhenLeagueNameAbsent_ShouldSetInitialsToNull()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Initials")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldSetPreparedTeamConnectedToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).PreparedTeamConnected")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithTeamDetailsState_ShouldSetReadonlyToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Readonly")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenPivotAbsent_ShouldSetNoPivotToTrue(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).NoPivot")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenPivotPresent_ShouldSetNoPivotToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, Guid.NewGuid(), [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).NoPivot")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenAllPositionsFilled_ShouldSetAllBlockersSetToTrue(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).AllBlockersSet")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenPivotSlotIsEmpty_ShouldSetAllBlockersSetToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, null, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).AllBlockersSet")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_WhenABlockerSlotIsEmpty_ShouldSetAllBlockersSetToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new JamLineupState(null, Guid.NewGuid(), [Guid.NewGuid(), null, Guid.NewGuid()]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).AllBlockersSet")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithJamLineupState_ShouldSetFieldingAdvancePendingToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).FieldingAdvancePending")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithScoreSheetState_WhenLastJamHasInjury_ShouldSetInjuryToTrue(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Injury = true }]);

        var result = Subject.Map(state, ScoreSheetState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Injury")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenLastJamHasNoInjury_ShouldSetInjuryToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Injury = false }]);

        var result = Subject.Map(state, ScoreSheetState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Injury")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenNoJams_ShouldSetInjuryToFalse()
    {
        var gameId = Guid.NewGuid();

        var result = Subject.Map(ScoreSheetState.Default, ScoreSheetState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Injury")
            .WhoseValue.Should().Be(false);
    }


    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetTotalScore(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Period = 1, Jam = 3, GameTotal = 42 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(3).TeamJam({expectedTeamNumber}).TotalScore")
            .WhoseValue.Should().Be(42);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetJamScore(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Period = 1, Jam = 3, JamTotal = 15 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(3).TeamJam({expectedTeamNumber}).JamScore")
            .WhoseValue.Should().Be(15);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetLead(bool lead)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Lead = lead }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).Lead")
            .WhoseValue.Should().Be(lead);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetLost(bool lost)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Lost = lost }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).Lost")
            .WhoseValue.Should().Be(lost);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetCalloff(bool called)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Called = called }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).Calloff")
            .WhoseValue.Should().Be(called);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetNoInitial(bool noInitial)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { NoInitial = noInitial }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).NoInitial")
            .WhoseValue.Should().Be(noInitial);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassTripIsSet_ShouldSetStarPassToTrue()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = 2 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).StarPass")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassTripIsNull_ShouldSetStarPassToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = null }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).StarPass")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldCorrectlySetScoringTripScore()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(null), new JamLineTrip(4)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).Score")
            .WhoseValue.Should().Be(4);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldIndexScoringTripsFromOne()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Score")
            .WhoseValue.Should().Be(0);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenNoStarPass_ShouldSetAfterSPToFalseForAllTrips()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)], StarPassTrip = null }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).AfterSP")
            .WhoseValue.Should().Be(false);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).AfterSP")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassAtIndexZero_ShouldSetAllTripsAfterSP()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)], StarPassTrip = 0 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).AfterSP")
            .WhoseValue.Should().Be(true);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).AfterSP")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassAtIndexOne_ShouldSetAfterSPBasedOnIndex()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(3), new JamLineTrip(0), new JamLineTrip(2)], StarPassTrip = 1 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).AfterSP")
            .WhoseValue.Should().Be(false);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).AfterSP")
            .WhoseValue.Should().Be(true);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(3).AfterSP")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldSetScoringTripNumberToOneBased()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Number")
            .WhoseValue.Should().Be(1);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).Number")
            .WhoseValue.Should().Be(2);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldSetCurrentToFalseForAllTrips()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Current")
            .WhoseValue.Should().Be(false);
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).Current")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldEmitNonNullIdForEachTrip()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Id")
            .WhoseValue.Should().NotBeNull();
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).Id")
            .WhoseValue.Should().NotBeNull();
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldEmitDistinctIdForEachTrip()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0), new JamLineTrip(3)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        var id1 = result[$"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Id"];
        var id2 = result[$"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(2).Id"];

        id1.Should().NotBe(id2);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldEmitStableIdForEachTrip()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Trips = [new JamLineTrip(0)] }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result1 = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);
        var result2 = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result1[$"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Id"]
            .Should().Be(result2[$"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Id"]);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldEmitDistinctIdForSameTripNumberAcrossJams()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([
            ScoreSheetJam.Default with { Period = 1, Jam = 1, Trips = [new JamLineTrip(3)] },
            ScoreSheetJam.Default with { Period = 1, Jam = 2, Trips = [new JamLineTrip(3)] },
        ]);
        var otherTeamState = new ScoreSheetState([
            ScoreSheetJam.Default with { Period = 1, Jam = 1 },
            ScoreSheetJam.Default with { Period = 1, Jam = 2 },
        ]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        var idJam1 = result[$"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).ScoringTrip(1).Id"];
        var idJam2 = result[$"ScoreBoard.Game({gameId}).Period(1).Jam(2).TeamJam(1).ScoringTrip(1).Id"];

        idJam1.Should().NotBe(idJam2);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetOvertime(bool isOvertime)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { IsOvertimeJam = isOvertime }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).Overtime")
            .WhoseValue.Should().Be(isOvertime);
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldEmitChannelsForEachJam()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([
            ScoreSheetJam.Default with { Period = 1, Jam = 1 },
            ScoreSheetJam.Default with { Period = 1, Jam = 2 },
        ]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default, ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).JamScore");
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(2).TeamJam(1).JamScore");
    }

    [Test]
    public void Map_WithScoreSheetState_ShouldNotEmitChannelsForDeletedJams()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([new DeletedScoreSheetJam(ScoreSheetJam.Default with { Period = 1, Jam = 1 })]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().NotContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).JamScore");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetLastScore(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Period = 1, Jam = 3, GameTotal = 42, JamTotal = 15 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(3).TeamJam({expectedTeamNumber}).LastScore")
            .WhoseValue.Should().Be(27);
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetHistoricJamInjury(bool injury)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Period = 1, Jam = 2, Injury = injury }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(2).TeamJam(1).Injury")
            .WhoseValue.Should().Be(injury);
    }

    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(false, false, false)]
    public void Map_WithScoreSheetState_ShouldCorrectlySetHistoricJamDisplayLead(bool lead, bool lost, bool expectedDisplayLead)
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { Lead = lead, Lost = lost }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).DisplayLead")
            .WhoseValue.Should().Be(expectedDisplayLead);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassTripIsSet_ShouldSetJamLevelStarPassToTrue()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = 2 }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = null }]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).StarPass")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenStarPassTripIsNull_ShouldSetJamLevelStarPassToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = null }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = null }]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).StarPass")
            .WhoseValue.Should().Be(false);
    }

    [Test]
    public void Map_WithScoreSheetState_WhenOtherTeamHasStarPass_ShouldSetJamLevelStarPassToTrue()
    {
        var gameId = Guid.NewGuid();
        var state = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = null }]);
        var otherTeamState = new ScoreSheetState([ScoreSheetJam.Default with { StarPassTrip = 2 }]);

        var result = Subject.Map(state, otherTeamState, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).StarPass")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetTotalPenalties(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(Guid.NewGuid(), "101", null, [new("X", 1, 1, false), new("B", 1, 2, false)]),
            new(Guid.NewGuid(), "202", null, [new("C", 1, 3, false)]),
        ]);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).TotalPenalties")
            .WhoseValue.Should().Be(3);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetSkaterPenaltyCount(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 1, 1, false), new("B", 1, 2, false)]),
        ]);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).PenaltyCount")
            .WhoseValue.Should().Be(2);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetPenaltyCode(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 1, 1, false)]),
        ]);

        var result = Subject.Map(state, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).Penalty(1).Code")
            .WhoseValue.Should().Be("X");
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldIndexPenaltiesFromOne()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 1, 1, false), new("B", 1, 2, false)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(2).Code")
            .WhoseValue.Should().Be("B");
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetPenaltyPeriodNumber()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 2, 5, false)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(1).PeriodNumber")
            .WhoseValue.Should().Be(2);
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetPenaltyJamNumber()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 2, 5, false)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(1).JamNumber")
            .WhoseValue.Should().Be(5);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetPenaltyServed(bool served)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, [new("X", 1, 1, served)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(1).Served")
            .WhoseValue.Should().Be(served);
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldSetExpulsionPenaltyAtIndexZero()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", new("X", 1, 5, true), [new("X", 1, 5, true)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(0).Code")
            .WhoseValue.Should().Be("X");
    }

    [Test]
    public void Map_WithPenaltySheetState_WhenNoExpulsion_ShouldNotEmitPenaltyZero()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", null, []),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().NotContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(0).Code");
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetExpulsionPenaltyPeriodNumber()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", new("X", 2, 5, true), [new("X", 2, 5, true)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(0).PeriodNumber")
            .WhoseValue.Should().Be(2);
    }

    [Test]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetExpulsionPenaltyJamNumber()
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", new("X", 2, 5, true), [new("X", 2, 5, true)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(0).JamNumber")
            .WhoseValue.Should().Be(5);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithPenaltySheetState_ShouldCorrectlySetExpulsionPenaltyServed(bool served)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltySheetState([
            new(skaterId, "101", new("X", 1, 3, served), [new("X", 1, 3, served)]),
        ]);

        var result = Subject.Map(state, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Skater({skaterId}).Penalty(0).Served")
            .WhoseValue.Should().Be(served);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltyBoxState_WhenSkaterIsInBox_ShouldSetSkaterPenaltyBoxToTrue(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([skaterId], []);
        var teamDetails = TeamDetailsWithSkater(skaterId);

        var result = Subject.Map(state, new JamLineupState(null, null, [null, null, null]), teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).PenaltyBox")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltyBoxState_WhenSkaterIsNotInBox_ShouldSetSkaterPenaltyBoxToFalse(TeamSide side, int expectedTeamNumber)
    {
        var skaterId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([], []);
        var teamDetails = TeamDetailsWithSkater(skaterId);

        var result = Subject.Map(state, new JamLineupState(null, null, [null, null, null]), teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Skater({skaterId}).PenaltyBox")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltyBoxState_WhenJammerIsInBox_ShouldSetJammerPositionPenaltyBoxToTrue(TeamSide side, int expectedTeamNumber)
    {
        var jammerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([jammerId], []);

        var result = Subject.Map(state, new JamLineupState(jammerId, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).PenaltyBox")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltyBoxState_WhenJammerIsNotInBox_ShouldSetJammerPositionPenaltyBoxToFalse(TeamSide side, int expectedTeamNumber)
    {
        var jammerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([], []);

        var result = Subject.Map(state, new JamLineupState(jammerId, null, [null, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Jammer).PenaltyBox")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithPenaltyBoxState_WhenBlockerIsInBox_ShouldSetBlockerPositionPenaltyBoxToTrue(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([blockerId], []);

        var result = Subject.Map(state, new JamLineupState(null, null, [blockerId, null, null]), TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team({expectedTeamNumber}).Position(Blocker1).PenaltyBox")
            .WhoseValue.Should().Be(true);
    }

    [Test]
    public void Map_WithPenaltyBoxState_WhenPositionHasNoSkater_ShouldSetPositionPenaltyBoxToFalse()
    {
        var gameId = Guid.NewGuid();
        var state = new PenaltyBoxState([], []);

        var result = Subject.Map(state, new JamLineupState(null, null, [null, null, null]), TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Team(1).Position(Jammer).PenaltyBox")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_ShouldCorrectlySetJammerNumber(TeamSide side, int expectedTeamNumber)
    {
        var jammerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var teamDetails = TeamDetailsWithSkater(jammerId, "101", "Test Skater");
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, jammerId, null, [null, null, null])]);

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Jammer).SkaterNumber")
            .WhoseValue.Should().Be("101");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_WhenJammerAbsent_ShouldSetJammerNumberToNull(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, null, [null, null, null])]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Jammer).SkaterNumber")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_ShouldCorrectlySetPivotNumber(TeamSide side, int expectedTeamNumber)
    {
        var pivotId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var teamDetails = TeamDetailsWithSkater(pivotId, "202", "Test Skater");
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, pivotId, [null, null, null])]);

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Pivot).SkaterNumber")
            .WhoseValue.Should().Be("202");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_ShouldCorrectlySetBlocker1Number(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var teamDetails = TeamDetailsWithSkater(blockerId, "303", "Test Skater");
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, null, [blockerId, null, null])]);

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Blocker1).SkaterNumber")
            .WhoseValue.Should().Be("303");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_ShouldCorrectlySetBlocker2Number(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var teamDetails = TeamDetailsWithSkater(blockerId, "404", "Test Skater");
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, null, [null, blockerId, null])]);

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Blocker2).SkaterNumber")
            .WhoseValue.Should().Be("404");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_ShouldCorrectlySetBlocker3Number(TeamSide side, int expectedTeamNumber)
    {
        var blockerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var teamDetails = TeamDetailsWithSkater(blockerId, "505", "Test Skater");
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, null, [null, null, blockerId])]);

        var result = Subject.Map(state, teamDetails, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).Fielding(Blocker3).SkaterNumber")
            .WhoseValue.Should().Be("505");
    }

    [Test]
    public void Map_WithLineupSheetState_ShouldEmitChannelsForEachJam()
    {
        var gameId = Guid.NewGuid();
        var state = new LineupSheetState([
            new LineupSheetJam(1, 1, false, null, null, [null, null, null]),
          new LineupSheetJam(1, 2, false, null, null, [null, null, null]),
      ]);

        var result = Subject.Map(state, TeamDetailsState.Default, TeamSide.Home, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam(1).Fielding(Jammer).SkaterNumber");
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(2).TeamJam(1).Fielding(Jammer).SkaterNumber");
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_WhenPivotAbsent_ShouldSetNoPivotToTrue(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, null, [null, null, null])]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).NoPivot")
            .WhoseValue.Should().Be(true);
    }

    [TestCase(TeamSide.Home, 1)]
    [TestCase(TeamSide.Away, 2)]
    public void Map_WithLineupSheetState_WhenPivotPresent_ShouldSetNoPivotToFalse(TeamSide side, int expectedTeamNumber)
    {
        var gameId = Guid.NewGuid();
        var state = new LineupSheetState([new LineupSheetJam(1, 1, false, null, Guid.NewGuid(), [null, null, null])]);

        var result = Subject.Map(state, TeamDetailsState.Default, side, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Jam(1).TeamJam({expectedTeamNumber}).NoPivot")
            .WhoseValue.Should().Be(false);
    }

    [TestCase(TimeoutType.Untyped, null, "")]
    [TestCase(TimeoutType.Official, null, "O")]
    [TestCase(TimeoutType.Team, TeamSide.Home, "1")]
    [TestCase(TimeoutType.Team, TeamSide.Away, "2")]
    [TestCase(TimeoutType.Review, TeamSide.Home, "1")]
    [TestCase(TimeoutType.Review, TeamSide.Away, "2")]
    public void Map_WithTimeoutListState_ShouldCorrectlySetTimeoutOwner(TimeoutType type, TeamSide? side, string expected)
    {
        var eventId = Guid7.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, type, 1, 1, side, null, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).Owner")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(TimeoutType.Review, true)]
    [TestCase(TimeoutType.Team, false)]
    [TestCase(TimeoutType.Official, false)]
    [TestCase(TimeoutType.Untyped, false)]
    public void Map_WithTimeoutListState_ShouldCorrectlySetReview(TimeoutType type, bool expected)
    {
        var eventId = Guid7.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, type, 1, 1, null, null, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).Review")
            .WhoseValue.Should().Be(expected);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Map_WithTimeoutListState_ShouldCorrectlySetRetainedReview(bool retained)
    {
        var eventId = Guid7.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Review, 1, 1, TeamSide.Home, null, retained)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).RetainedReview")
            .WhoseValue.Should().Be(retained);
    }

    [Test]
    public void Map_WithTimeoutListState_ShouldConvertDurationToMilliseconds()
    {
        var eventId = Guid7.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Official, 1, 1, null, 30, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).Duration")
            .WhoseValue.Should().Be(30000);
    }

    [Test]
    public void Map_WithTimeoutListState_WhenDurationIsNull_ShouldSetDurationToNull()
    {
        Guid7 eventId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Official, 1, 1, null, null, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).Duration")
            .WhoseValue.Should().BeNull();
    }

    [Test]
    public void Map_WithTimeoutListState_ShouldEmitChannelsForEachTimeout()
    {
        Guid7 eventId1 = Guid.NewGuid();
        Guid7 eventId2 = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([
            new TimeoutListItem(eventId1, TimeoutType.Official, 1, 5, null, 60, false),
            new TimeoutListItem(eventId2, TimeoutType.Team, 2, 1, TeamSide.Home, 30, false),
        ]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId1}).Owner");
        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(2).Timeout({eventId2}).Owner");
    }

    [Test]
    public void Map_WithTimeoutListState_ShouldSetWalltimeStartFromEventId()
    {
        var tick = 1_000_000_000L;
        Guid7 eventId = tick;
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Official, 1, 1, null, null, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).WalltimeStart")
            .WhoseValue.Should().Be(tick);
    }

    [Test]
    public void Map_WithTimeoutListState_WhenDurationIsSet_ShouldSetWalltimeEnd()
    {
        var tick = 1_000_000_000L;
        Guid7 eventId = tick;
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Official, 1, 1, null, 30, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).WalltimeEnd")
            .WhoseValue.Should().Be(tick + 30_000);
    }

    [Test]
    public void Map_WithTimeoutListState_WhenDurationIsNull_ShouldSetWalltimeEndToNull()
    {
        Guid7 eventId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var state = new TimeoutListState([new TimeoutListItem(eventId, TimeoutType.Official, 1, 1, null, null, false)]);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Period(1).Timeout({eventId}).WalltimeEnd")
            .WhoseValue.Should().BeNull();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    public void Map_WithTimeoutListState_ShouldSetTimeoutClockNumber(int timeoutCount)
    {
        var gameId = Guid.NewGuid();
        var items = Enumerable.Range(0, timeoutCount)
            .Select(_ => new TimeoutListItem((Guid7)Guid.NewGuid(), TimeoutType.Official, 1, 1, null, null, false))
            .ToArray();
        var state = new TimeoutListState(items);

        var result = Subject.Map(state, gameId);

        result.Should().ContainKey($"ScoreBoard.Game({gameId}).Clock(Timeout).Number")
            .WhoseValue.Should().Be(timeoutCount);
    }

    private static TeamDetailsState TeamDetailsWithSkater(Guid id, string number = "101", string name = "Test Skater 1") =>
        new(new GameTeam(
            [],
            new TeamColor(Color.Black, Color.White),
            [new GameSkater(id, number, name, true)]
        ));
}
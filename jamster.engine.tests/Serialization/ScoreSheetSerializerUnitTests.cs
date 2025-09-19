using jamster.Domain;
using jamster.Reducers;
using jamster.Serialization;
using jamster.Services;
using FluentAssertions;

namespace jamster.engine.tests.Serialization;

public class ScoreSheetSerializerUnitTests : UnitTest<ScoreSheetSerializer>
{
    [Test]
    public void Serialize_CorrectlyStoresJamNumbers()
    {
        var homeScoreSheetState = new ScoreSheetState([
            new(1, 1, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 2, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 3, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
            new(1, 4, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 5, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            new(1, 1, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 2, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 3, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 4, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
            new(1, 5, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
        ]);

        MockKeyedState(TeamSide.Home, homeScoreSheetState);
        MockKeyedState(TeamSide.Away, awayScoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(8);
        result.HomePeriod1.Lines[0].Jam.ToString().Should().Be("1");
        result.HomePeriod1.Lines[1].Jam.ToString().Should().Be("2");
        result.HomePeriod1.Lines[2].Jam.ToString().Should().Be("3");
        result.HomePeriod1.Lines[3].Jam.ToString().Should().Be("SP");
        result.HomePeriod1.Lines[4].Jam.ToString().Should().Be("4");
        result.HomePeriod1.Lines[5].Jam.ToString().Should().Be("SP*");
        result.HomePeriod1.Lines[6].Jam.ToString().Should().Be("5");
        result.HomePeriod1.Lines[7].Jam.ToString().Should().Be("SP");
    }

    [Test]
    public void Serialize_CorrectlyStoresJammerNumbers()
    {
        var homeScoreSheetState = new ScoreSheetState([
            new(1, 1, "123", "321", false, false, false, false, false, [], null, 0, 0),
            new(1, 2, "456", "654", false, false, false, false, false, [], null, 0, 0),
            new(1, 3, "123", "321", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
            new(1, 4, "456", "654", false, false, false, false, false, [], null, 0, 0),
            new(1, 5, "123", "321", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            new(1, 1, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 2, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 3, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 4, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
            new(1, 5, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
        ]);

        MockKeyedState(TeamSide.Home, homeScoreSheetState);
        MockKeyedState(TeamSide.Away, awayScoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(8);
        result.HomePeriod1.Lines[0].JammerNumber.Should().Be("123");
        result.HomePeriod1.Lines[1].JammerNumber.Should().Be("456");
        result.HomePeriod1.Lines[2].JammerNumber.Should().Be("123");
        result.HomePeriod1.Lines[3].JammerNumber.Should().Be("321");
        result.HomePeriod1.Lines[4].JammerNumber.Should().Be("456");
        result.HomePeriod1.Lines[5].JammerNumber.Should().Be("");
        result.HomePeriod1.Lines[6].JammerNumber.Should().Be("123");
        result.HomePeriod1.Lines[7].JammerNumber.Should().Be("321");
    }

    [Test]
    public void Serialize_CorrectlyStoresJamStats()
    {
        var scoreSheetState = new ScoreSheetState(
            Enumerable.Range(0, 0b11111)
                .Select(i => new ScoreSheetJam(1, i + 1, "", "", (i & 0b10000) > 0, (i & 0b01000) > 0, (i & 0b00100) > 0, (i & 0b00010) > 0, (i & 0b00001) > 0, [], null, 0, 0))
                .ToArray()
            );

        MockKeyedState(TeamSide.Home, scoreSheetState);
        MockKeyedState(TeamSide.Away, scoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        var expectedLines = Enumerable.Range(0, 0b11111)
            .Select(i => new ScoreSheetLine(i + 1, "", (i & 0b10000) > 0, (i & 0b01000) > 0, (i & 0b00100) > 0, (i & 0b00010) > 0, (i & 0b00001) > 0, Enumerable.Repeat(new ScoreSheetTrip(null), 9).ToArray()))
            .ToArray();

        result.HomePeriod1.Lines.Should().BeEquivalentTo(expectedLines);
    }

    [Test]
    public void Serialize_CorrectlyStoresScores()
    {
        var homeScoreSheetState = new ScoreSheetState([
            new(1, 1, "", "", false, false, false, false, false, [new(4), new(3)], null, 7, 7),
            new(1, 2, "", "", false, false, false, false, false, [], null, 0, 7),
            new(1, 3, "", "", false, false, false, false, false, [new(4), new(4)], 1, 8, 15),
            new(1, 4, "", "", false, false, false, false, false, [new(4), new(4)], null, 8, 23),
            new(1, 5, "", "", false, false, false, false, false, [new(4), new(4)], 0, 8, 31),
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            new(1, 1, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 2, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 3, "", "", false, false, false, false, false, [], null, 0, 0),
            new(1, 4, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
            new(1, 5, "", "", false, false, false, false, false, [new(4), new(4)], 1, 0, 0),
        ]);

        MockKeyedState(TeamSide.Home, homeScoreSheetState);
        MockKeyedState(TeamSide.Away, awayScoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(8);
        result.HomePeriod1.Lines[0].Should().Be(new ScoreSheetLine(1, "", false, false, false, false, false, [new(4), new(3), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[1].Should().Be(new ScoreSheetLine(2, "", false, false, false, false, false, [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[2].Should().Be(new ScoreSheetLine(3, "", false, false, false, false, false, [new(4), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[3].Should().Be(new ScoreSheetLine("SP", "", false, false, false, false, false, [new(null), new(4), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[4].Should().Be(new ScoreSheetLine(4, "", false, false, false, false, false, [new(4), new(4), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[5].Should().Be(new ScoreSheetLine("SP*", "", false, false, false, false, false, [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[6].Should().Be(new ScoreSheetLine(5, "", false, false, false, false, true, [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
        result.HomePeriod1.Lines[7].Should().Be(new ScoreSheetLine("SP", "", false, false, false, false, false, [new(4), new(4), new(null), new(null), new(null), new(null), new(null), new(null), new(null)]));
    }

    private void MockState<TState>(TState state) where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetState<TState>()).Returns(state);

    private void MockKeyedState<TKey, TState>(TKey key, TState state) where TKey : notnull where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetKeyedState<TState>(key.ToString() ?? "")).Returns(state);
}
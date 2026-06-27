using AwesomeAssertions;

using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Serialization;
using jamster.engine.Services;

namespace jamster.engine.tests.Serialization;

public class ScoreSheetSerializerUnitTests : UnitTest<ScoreSheetSerializer>
{
    [Test]
    public void Serialize_CorrectlyStoresJamNumbers()
    {
        var homeScoreSheetState = new ScoreSheetState([
            ScoreSheetJam.Default with { Jam = 1, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 2, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 3, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 8, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 4, NoInitial = true, GameTotal = 8 },
            ScoreSheetJam.Default with { Jam = 5, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 16, StarPassTrip = 1 },
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            ScoreSheetJam.Default with { Jam = 1, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 2, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 3, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 4, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 8, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 5, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 16, StarPassTrip = 1 },
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
            ScoreSheetJam.Default with { Jam = 1, JammerNumber = "123", PivotNumber = "321", NoInitial = true },
            ScoreSheetJam.Default with { Jam = 2, JammerNumber = "456", PivotNumber = "654", NoInitial = true },
            ScoreSheetJam.Default with { Jam = 3, JammerNumber = "123", PivotNumber = "321", NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 8, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 4, JammerNumber = "456", PivotNumber = "654", NoInitial = true, GameTotal = 8 },
            ScoreSheetJam.Default with { Jam = 5, JammerNumber = "123", PivotNumber = "321", NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 16, StarPassTrip = 1 },
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            ScoreSheetJam.Default with { Jam = 1, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 2, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 3, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 4, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 8, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 5, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 16, StarPassTrip = 1 },
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
                .Select(i => ScoreSheetJam.Default with
                {
                    Jam = i + 1, 
                    Lost = (i & 0b10000) > 0,
                    Lead = (i & 0b01000) > 0,
                    Called = (i & 0b00100) > 0,
                    Injury = (i & 0b00010) > 0,
                    NoInitial = (i & 0b00001) > 0,
                })
                .ToArray()
            );

        MockKeyedState(TeamSide.Home, scoreSheetState);
        MockKeyedState(TeamSide.Away, scoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        var expectedLines = Enumerable.Range(0, 0b11111)
            .Select(i => ScoreSheetLine.Default with
            {
                Jam = i + 1,
                Lost = (i & 0b10000) > 0,
                Lead = (i & 0b01000) > 0,
                Call = (i & 0b00100) > 0,
                Injury = (i & 0b00010) > 0,
                NoInitial = (i & 0b00001) > 0,
                Trips = Enumerable.Repeat(new ScoreSheetTrip(null), 9).ToArray(),
            })
            .ToArray();

        result.HomePeriod1.Lines.Should().BeEquivalentTo(expectedLines);
    }

    [Test]
    public void Serialize_CorrectlyStoresScores()
    {
        var homeScoreSheetState = new ScoreSheetState([
            ScoreSheetJam.Default with { Jam = 1, NoInitial = false, Trips = [4, 3], JamTotal = 7, GameTotal = 7 },
            ScoreSheetJam.Default with { Jam = 2, NoInitial = true,  Trips = [], JamTotal = 0, GameTotal = 7 },
            ScoreSheetJam.Default with { Jam = 3, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 15, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 4, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 23 },
            ScoreSheetJam.Default with { Jam = 5, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 31, StarPassTrip = 0 },
            ScoreSheetJam.Default with { Jam = 6, NoInitial = false, Trips = [4, 3], JamTotal = 7, GameTotal = 38, IsOvertimeJam = true },
            ScoreSheetJam.Default with { Jam = 7, NoInitial = false, Trips = [4, 4, 4, 4, 4, 4, 4, 4, 3, 2], JamTotal = 37, GameTotal = 75 },
            ScoreSheetJam.Default with { Jam = 8, NoInitial = false, Trips = [1, 2, 3, 4, 4, 4, 4, 4, 3, 2, 1], JamTotal = 32, GameTotal = 107, IsOvertimeJam = true },
            ScoreSheetJam.Default with { Jam = 9, NoInitial = false, Trips = [3, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2], JamTotal = 40, GameTotal = 147, IsOvertimeJam = true, StarPassTrip = 2 },
            ScoreSheetJam.Default with { Jam = 10, NoInitial = false, Trips = [3, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2], JamTotal = 40, GameTotal = 147, IsOvertimeJam = true, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 11, NoInitial = false, Trips = [3, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2], JamTotal = 40, GameTotal = 147, IsOvertimeJam = true, StarPassTrip = 0 },
        ]);

        var awayScoreSheetState = new ScoreSheetState([
            ScoreSheetJam.Default with { Jam = 1, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 2, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 3, NoInitial = true },
            ScoreSheetJam.Default with { Jam = 4, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 8, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 5, NoInitial = false, Trips = [4, 4], JamTotal = 8, GameTotal = 16, StarPassTrip = 1 },
            ScoreSheetJam.Default with { Jam = 6, NoInitial = true, GameTotal = 16 },
            ScoreSheetJam.Default with { Jam = 7, NoInitial = true, GameTotal = 16 },
            ScoreSheetJam.Default with { Jam = 8, NoInitial = true, GameTotal = 16 },
            ScoreSheetJam.Default with { Jam = 9, NoInitial = true, GameTotal = 16 },
            ScoreSheetJam.Default with { Jam = 10, NoInitial = true, GameTotal = 16 },
            ScoreSheetJam.Default with { Jam = 11, NoInitial = true, GameTotal = 16 },
        ]);

        MockKeyedState(TeamSide.Home, homeScoreSheetState);
        MockKeyedState(TeamSide.Away, awayScoreSheetState);

        var result = Subject.Serialize(GetMock<IGameStateStore>().Object);

        result.HomePeriod1.Lines.Should().HaveCount(17);
        result.HomePeriod1.Lines[0].Should().Be(ScoreSheetLine.Default with { Jam = 1, NoInitial = false, Trips = [4, 3, new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[1].Should().Be(ScoreSheetLine.Default with { Jam = 2, NoInitial = true, Trips = [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[2].Should().Be(ScoreSheetLine.Default with { Jam = 3, NoInitial = false, Trips = [4, new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[3].Should().Be(ScoreSheetLine.Default with { Jam = "SP", NoInitial = false, Trips = [new(null), 4, new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[4].Should().Be(ScoreSheetLine.Default with { Jam = 4, NoInitial = false, Trips = [4, 4, new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[5].Should().Be(ScoreSheetLine.Default with { Jam = "SP*", NoInitial = false, Trips = [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[6].Should().Be(ScoreSheetLine.Default with { Jam = 5, NoInitial = true, Trips = [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[7].Should().Be(ScoreSheetLine.Default with { Jam = "SP", NoInitial = false, Trips = [4, 4, new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[8].Should().Be(ScoreSheetLine.Default with { Jam = 6, NoInitial = false, Trips = ["4+3", new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[9].Should().Be(ScoreSheetLine.Default with { Jam = 7, NoInitial = false, Trips = [4, 4, 4, 4, 4, 4, 4, 4, "3+2"] });
        result.HomePeriod1.Lines[10].Should().Be(ScoreSheetLine.Default with { Jam = 8, NoInitial = false, Trips = ["1+2", 3, 4, 4, 4, 4, 4, 3, "2+1"] });
        result.HomePeriod1.Lines[11].Should().Be(ScoreSheetLine.Default with { Jam = 9, NoInitial = false, Trips = ["3+4", new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[12].Should().Be(ScoreSheetLine.Default with { Jam = "SP", NoInitial = false, Trips = [new(null), 4, 4, 4, 4, 4, 4, 4, "3+2"] });
        result.HomePeriod1.Lines[13].Should().Be(ScoreSheetLine.Default with { Jam = 10, NoInitial = false, Trips = ["3", new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[14].Should().Be(ScoreSheetLine.Default with { Jam = "SP", NoInitial = false, Trips = [4, 4, 4, 4, 4, 4, 4, 4, "3+2"] });
        result.HomePeriod1.Lines[15].Should().Be(ScoreSheetLine.Default with { Jam = 11, NoInitial = true, Trips = [new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null), new(null)] });
        result.HomePeriod1.Lines[16].Should().Be(ScoreSheetLine.Default with { Jam = "SP", NoInitial = false, Trips = ["3+4", 4, 4, 4, 4, 4, 4, 4, "3+2"] });
    }

    private void MockState<TState>(TState state) where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetState<TState>()).Returns(state);

    private void MockKeyedState<TKey, TState>(TKey key, TState state) where TKey : notnull where TState : class =>
        GetMock<IGameStateStore>().Setup(mock => mock.GetKeyedState<TState>(key.ToString() ?? "")).Returns(state);
}
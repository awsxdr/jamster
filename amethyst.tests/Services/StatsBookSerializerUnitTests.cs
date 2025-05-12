using System.IO.Compression;
using System.Reflection;
using amethyst.Serialization;
using amethyst.Services.Stats;
using FluentAssertions;
using Func;
using Moq;
using IgrfSerializer = amethyst.Services.Stats.IgrfSerializer;
using IIgrfSerializer = amethyst.Services.Stats.IIgrfSerializer;
using IScoreSheetSerializer = amethyst.Services.Stats.IScoreSheetSerializer;
using ScoreSheetSerializer = amethyst.Services.Stats.ScoreSheetSerializer;

namespace amethyst.tests.Services;

public class StatsBookSerializerUnitTests : UnitTest<StatsBookSerializer>
{
    protected override StatsBookSerializer SubjectFactory() =>
            Create<Func<IIgrfSerializer, IScoreSheetSerializer, StatsBookSerializer>>()
                (Create<IgrfSerializer>(), Create<ScoreSheetSerializer>());

    [Test]
    public async Task DeserializeStream_ShouldDeserializeValidStatsBookCorrectly()
    {
        GetMock<IStatsBookValidator>()
            .Setup(mock => mock.ValidateStatsBook(It.IsAny<ZipArchive>()))
            .ReturnsAsync(Result.Succeed(new StatsBookInfo("Test")));

        await using var file = GetTestFile("ValidStatsbook1.xlsx");

        var result = await Subject.DeserializeStream(file);

        var statsBook = result.Should().BeAssignableTo<Success<StatsBook>>().Subject.Value;

        statsBook.Igrf.Location.Venue.Should().Be("Biddulph Valley Leisure Centre");
        statsBook.Igrf.Location.City.Should().Be("Stoke");
        statsBook.Igrf.Location.Province.Should().Be("UK");

        statsBook.Igrf.GameDetails.EventName.Should().Be("Five Nations 2024 - Tier 4 WFTDA West");
        statsBook.Igrf.GameDetails.GameNumber.Should().Be("1");
        statsBook.Igrf.GameDetails.HostLeagueName.Should().Be("Stoke City Rollers");
        statsBook.Igrf.GameDetails.GameStart.Should().Be(new DateTime(2024, 6, 30, 11, 0, 0));

        statsBook.Igrf.GameSummary.Period1Summary.HomeTeamScore.Should().Be(106);
        statsBook.Igrf.GameSummary.Period1Summary.HomeTeamPenalties.Should().Be(15);
        statsBook.Igrf.GameSummary.Period1Summary.AwayTeamScore.Should().Be(133);
        statsBook.Igrf.GameSummary.Period1Summary.AwayTeamPenalties.Should().Be(16);
        statsBook.Igrf.GameSummary.Period2Summary.HomeTeamScore.Should().Be(61);
        statsBook.Igrf.GameSummary.Period2Summary.HomeTeamPenalties.Should().Be(19);
        statsBook.Igrf.GameSummary.Period2Summary.AwayTeamScore.Should().Be(115);
        statsBook.Igrf.GameSummary.Period2Summary.AwayTeamPenalties.Should().Be(7);

        statsBook.Igrf.Teams.HomeTeam.LeagueName.Should().Be("Coventry Roller Derby");
        statsBook.Igrf.Teams.HomeTeam.TeamName.Should().Be("Coventry Roller Derby");
        statsBook.Igrf.Teams.HomeTeam.ColorName.Should().Be("Black");
        statsBook.Igrf.Teams.AwayTeam.LeagueName.Should().Be("Liverpool Roller Birds");
        statsBook.Igrf.Teams.AwayTeam.TeamName.Should().Be("Yellow Shovemarines");
        statsBook.Igrf.Teams.AwayTeam.ColorName.Should().Be("Yellow");

        statsBook.Igrf.Teams.HomeTeam.Skaters.Should().BeEquivalentTo([
            new StatsBookSkater("05", "Rhythm And Bruise", true),
            new StatsBookSkater("107", "Toni Smaxton", true),
            new StatsBookSkater("1121", "Rainbow Raider", true),
            new StatsBookSkater("113", "Cluck Ewe", true),
            new StatsBookSkater("134", "SoNiK BOOM", false),
            new StatsBookSkater("147", "SDS-PAIN", true),
            new StatsBookSkater("17", "Hittapotamus", true),
            new StatsBookSkater("178", "SuperNova", false),
            new StatsBookSkater("196", "Sylvia Wrath", true),
            new StatsBookSkater("23", "Nev", true),
            new StatsBookSkater("27", "Babs", false),
            new StatsBookSkater("3110", "Ashy Slashy", true),
            new StatsBookSkater("314", "Darcey Daydream", false),
            new StatsBookSkater("42", "Eerie Indy", true),
            new StatsBookSkater("55", "Doom Rayder", true),
            new StatsBookSkater("71", "H", true),
            new StatsBookSkater("77", "Von Queef", true),
            new StatsBookSkater("841", "Ruthess Beast", true),
            new StatsBookSkater("888", "Dread Knocks", false),
            new StatsBookSkater("9", "Magpie", true),
        ]);

        statsBook.Igrf.Teams.AwayTeam.Skaters.Should().BeEquivalentTo([
            new StatsBookSkater("011", "Lez Zeppelin", false),
            new StatsBookSkater("017", "G", true),
            new StatsBookSkater("112", "Wearing", true),
            new StatsBookSkater("135", "Potter", true),
            new StatsBookSkater("2019", "Rowdy", true),
            new StatsBookSkater("209", "Dougie", true),
            new StatsBookSkater("222", "Streetwise", true),
            new StatsBookSkater("231", "Brody Hellz", true),
            new StatsBookSkater("2820", "Grrrrrrrrrrr", false),
            new StatsBookSkater("33", "Cat Attack", true),
            new StatsBookSkater("34", "Smashley", true),
            new StatsBookSkater("36", "Atomic", true),
            new StatsBookSkater("420", "Hugh Jass", true),
            new StatsBookSkater("51", "Meatball", true),
            new StatsBookSkater("56", "Mai", false),
            new StatsBookSkater("7", "Scary Biscuit", true),
            new StatsBookSkater("808", "Ellis", false),
            new StatsBookSkater("93", "Gillotine", true),
            new StatsBookSkater("94", "Jackson", true),
            new StatsBookSkater("99", "Tinie Tempest", false),
        ]);
    }

    private Stream GetTestFile(string fileName) =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), fileName) 
        ?? throw new Exception("Could not find test file");
}
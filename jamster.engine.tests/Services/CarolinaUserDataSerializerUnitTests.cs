using jamster.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using jamster.Configurations;
using FluentAssertions;

namespace jamster.engine.tests.Services;

public class CarolinaUserDataSerializerUnitTests : UnitTest<CarolinaUserDataSerializer>
{
    [Test]
    public void Deserializer_WithValidFile_DeserializesExpectedValues()
    {
        var testStream = GetTestFile("CrgUserExport.json");

        var result = Subject.Deserialize(JsonSerializer.Deserialize<JsonObject>(testStream)!);

        var user = result.Should().BeSuccess<IEnumerable<UserWithConfigurations>>()
            .Which.Value.Should().ContainSingle().Subject;

        user.UserName.Should().Be("CrgExportTest");
        var inputControls = user.Configurations.Should().ContainKey(nameof(InputControls)).WhoseValue.Should().BeOfType<InputControls>().Which;

        inputControls.Clocks.Start!.Binding.Should().Be("`");
        inputControls.Clocks.Stop!.Binding.Should().Be("q");
        inputControls.Clocks.Timeout!.Binding.Should().Be("shift+q");
        inputControls.Clocks.Undo!.Binding.Should().Be("w");

        inputControls.HomeScore.DecrementScore!.Binding.Should().Be("shift+w");
        inputControls.HomeScore.IncrementScore!.Binding.Should().Be("-");
        inputControls.HomeScore.SetTripScoreUnknown.Should().BeNull();
        inputControls.HomeScore.SetTripScore0!.Binding.Should().Be("[");
        inputControls.HomeScore.SetTripScore1!.Binding.Should().Be("]");
        inputControls.HomeScore.SetTripScore2!.Binding.Should().Be("{");
        inputControls.HomeScore.SetTripScore3!.Binding.Should().Be("}");
        inputControls.HomeScore.SetTripScore4!.Binding.Should().Be("+");

        inputControls.AwayScore.DecrementScore!.Binding.Should().Be(".");
        inputControls.AwayScore.IncrementScore!.Binding.Should().Be("?");
        inputControls.AwayScore.SetTripScoreUnknown.Should().BeNull();
        inputControls.AwayScore.SetTripScore0!.Binding.Should().Be("$");
        inputControls.AwayScore.SetTripScore1!.Binding.Should().Be("£");
        inputControls.AwayScore.SetTripScore2!.Binding.Should().Be("\"");
        inputControls.AwayScore.SetTripScore3!.Binding.Should().Be("!");
        inputControls.AwayScore.SetTripScore4!.Binding.Should().Be("<");

        inputControls.HomeStats.Lead!.Binding.Should().Be("@");
        inputControls.HomeStats.Lost!.Binding.Should().Be("'");
        inputControls.HomeStats.Called!.Binding.Should().Be(";");
        inputControls.HomeStats.StarPass!.Binding.Should().Be("#");
        inputControls.HomeStats.InitialTrip!.Binding.Should().Be("/");

        inputControls.AwayStats.Lead!.Binding.Should().Be("^");
        inputControls.AwayStats.Lost!.Binding.Should().Be("%");
        inputControls.AwayStats.Called!.Binding.Should().Be("&");
        inputControls.AwayStats.StarPass!.Binding.Should().Be("v");
        inputControls.AwayStats.InitialTrip!.Binding.Should().Be(">");
    }

    private Stream GetTestFile(string fileName) =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), fileName)
        ?? throw new Exception("Could not find test file");

}
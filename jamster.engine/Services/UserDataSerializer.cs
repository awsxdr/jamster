using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using jamster.Configurations;

namespace jamster.Services;

public interface IUserDataSerializer
{
    Result<IEnumerable<UserWithConfigurations>> Deserialize(Stream jsonStream);
    string Serialize(IEnumerable<UserWithConfigurations> users);
}

[Singleton]
public class UserDataSerializer(IEnumerable<IUserJsonDataDeserializer> deserializers) : IUserDataSerializer
{
    public Result<IEnumerable<UserWithConfigurations>> Deserialize(Stream jsonStream)
    {
        var json = JsonSerializer.Deserialize<JsonObject>(jsonStream, Program.JsonSerializerOptions);
        if (json == null) return Result<IEnumerable<UserWithConfigurations>>.Fail<InvalidStreamContentError>();

        var deserializer = GetDeserializer(json);
        if (deserializer == null) return Result<IEnumerable<UserWithConfigurations>>.Fail<InvalidStreamContentError>();

        return deserializer.Deserialize(json);
    }

    public string Serialize(IEnumerable<UserWithConfigurations> users) =>
        JsonSerializer.Serialize(new UserExportFileEnvelope(["Users"], users.ToArray()), Program.JsonSerializerOptions);

    private IUserJsonDataDeserializer? GetDeserializer(JsonObject json) =>
        deserializers.FirstOrDefault(d => d.CanHandle(json));
}

public interface IUserJsonDataDeserializer
{
    bool CanHandle(JsonObject json);
    Result<IEnumerable<UserWithConfigurations>> Deserialize(JsonObject json);
}

[Singleton]
public class JamsterUserJsonDataSerializer : IUserJsonDataDeserializer
{
    public bool CanHandle(JsonObject json) =>
        json["dataTypes"]?.AsArray().Select(a => a?.GetValue<string>()).Contains("Users") ?? false;

    public Result<IEnumerable<UserWithConfigurations>> Deserialize(JsonObject json)
    {
        var deserializedData = json.Deserialize<UserExportFileEnvelope>();

        if (!(deserializedData?.DataTypes.Contains("Users") ?? false) || deserializedData.Users == null)
            return Result<IEnumerable<UserWithConfigurations>>.Fail<InvalidStreamContentError>();

        return Result.Succeed<IEnumerable<UserWithConfigurations>>(deserializedData.Users);
    }
}

[Singleton]
public class CarolinaUserDataSerializer : IUserJsonDataDeserializer
{
    public bool CanHandle(JsonObject json) =>
        json["state"]?["ScoreBoard.Version(release)"] != null;

    public Result<IEnumerable<UserWithConfigurations>> Deserialize(JsonObject json)
    {
        if (json["state"] == null)
            return Result<IEnumerable<UserWithConfigurations>>.Fail<InvalidStreamContentError>();

        var state = json["state"]!.AsObject().ToDictionary();

        var userNames = GetUserNames(state).ToArray();

        return Result.Succeed(userNames.Select(u => new UserWithConfigurations(
            u,
            new Dictionary<string, object>
            {
                [nameof(InputControls)] = ParseInputControlsForUser(u, state),
            })));
    }

    private static IEnumerable<string> GetUserNames(IDictionary<string, JsonNode?> json)
    {
        var userNameRegex = new Regex(@"^ScoreBoard\.Settings\.Setting\(ScoreBoard\.Operator__(?<n>[^\.]+)", RegexOptions.Compiled);

        return json.Keys
            .Select(k => userNameRegex.Match(k))
            .Where(m => m.Success)
            .Select(k => k.Groups["n"].Value)
            .Distinct();
    }

    private static InputControls ParseInputControlsForUser(string userName, IDictionary<string, JsonNode?> json)
    {
        var settingKeyRegex =
            new Regex(@$"^ScoreBoard\.Settings\.Setting\(ScoreBoard\.Operator__{Regex.Escape(userName)}\.KeyControl\.(?<k>[^\)]+)\)$",
                RegexOptions.Compiled);

        var extractedKeys = json.Keys
            .Select(k => settingKeyRegex.Match(k))
            .Where(m => m.Success)
            .ToDictionary(m => m.Groups["k"].Value, m => json[m.Value]!.AsValue().GetValue<string>());

        return new InputControls(
            new ClockControls(
                ParseInputControl("StartJam"),
                ParseInputControl("StopJam"),
                ParseInputControl("Timeout"),
                ParseInputControl("ClockUndo") ?? ParseInputControl("ClockReplace")
            ),
            new ScoreControls(
                ParseInputControl("Team1ScoreDown"),
                ParseInputControl("Team1ScoreUp"),
                null,
                ParseInputControl("Team1TripScore0"),
                ParseInputControl("Team1TripScore1"),
                ParseInputControl("Team1TripScore2"),
                ParseInputControl("Team1TripScore3"),
                ParseInputControl("Team1TripScore4")
            ),
            new ScoreControls(
                ParseInputControl("Team2ScoreDown"),
                ParseInputControl("Team2ScoreUp"),
                null,
                ParseInputControl("Team2TripScore0"),
                ParseInputControl("Team2TripScore1"),
                ParseInputControl("Team2TripScore2"),
                ParseInputControl("Team2TripScore3"),
                ParseInputControl("Team2TripScore4")
            ),
            new StatsControls(
                ParseInputControl("Team1Lead"),
                ParseInputControl("Team1Lost"),
                ParseInputControl("Team1Call"),
                ParseInputControl("Team1StarPass"),
                ParseInputControl("Team1NI") ?? ParseInputControl("Team1AddTrip")
            ),
            new StatsControls(
                ParseInputControl("Team2Lead"),
                ParseInputControl("Team2Lost"),
                ParseInputControl("Team2Call"),
                ParseInputControl("Team2StarPass"),
                ParseInputControl("Team2NI") ?? ParseInputControl("Team2AddTrip")
            ));

        InputControl? ParseInputControl(string key) =>
            extractedKeys.TryGetValue(key, out var input)
                ? new(InputType.Keyboard, ParseBinding(input))
                : null;
    }

    private static string ParseBinding(string input)
    {
        if (input.Length != 1) return input;

        var lowercaseInput = input.ToLowerInvariant();
        if (lowercaseInput != input) return $"shift+{lowercaseInput}";

        return input;
    }
}

public sealed class InvalidStreamContentError : ResultError;

public record ExportFileEnvelope(string[] DataTypes);

public record UserExportFileEnvelope(string[] DataTypes, UserWithConfigurations[]? Users) : ExportFileEnvelope(DataTypes);
public record UserWithConfigurations(string UserName, Dictionary<string, object> Configurations);
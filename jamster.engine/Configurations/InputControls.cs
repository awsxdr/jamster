namespace jamster.engine.Configurations;

public record InputControls(ClockControls Clocks, ScoreControls HomeScore, ScoreControls AwayScore, StatsControls HomeStats, StatsControls AwayStats);

public class InputControlsConfigurationFactory : IConfigurationFactory<InputControls>
{
    public InputControls GetDefaultValue() => new(
        new(null, null, null, null),
        new(null, null, null, null, null, null, null, null),
        new(null, null, null, null, null, null, null, null),
        new(null, null, null, null, null),
        new(null, null, null, null, null)
    );
}

public record InputControl(InputType Type, string Binding);

public record ClockControls(
    InputControl? Start,
    InputControl? Stop,
    InputControl? Timeout,
    InputControl? Undo
);

public record ScoreControls(
    InputControl? DecrementScore,
    InputControl? IncrementScore,
    InputControl? SetTripScoreUnknown,
    InputControl? SetTripScore0,
    InputControl? SetTripScore1,
    InputControl? SetTripScore2,
    InputControl? SetTripScore3,
    InputControl? SetTripScore4
);

public record StatsControls(
    InputControl? Lead,
    InputControl? Lost,
    InputControl? Called,
    InputControl? StarPass,
    InputControl? InitialTrip
);

public enum InputType
{
    Keyboard
}
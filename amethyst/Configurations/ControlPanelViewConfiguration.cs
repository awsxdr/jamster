namespace amethyst.Configurations;

public record ControlPanelViewConfiguration(
    bool ShowClockControls,
    bool ShowScoreControls,
    bool ShowStatsControls,
    bool ShowLineupControls,
    bool ShowClocks,
    bool ShowTimeoutList,
    bool ShowScoreSheet,
    bool ShowTimeline,
    DisplaySide DisplaySide
);

public enum DisplaySide
{
    Home,
    Away,
    Both,
}

public class ControlPanelViewConfigurationFactory : IConfigurationFactory<ControlPanelViewConfiguration>
{
    public ControlPanelViewConfiguration GetDefaultValue() => new(
        true,
        true,
        true,
        true,
        true,
        false,
        true,
        false,
        DisplaySide.Both);
}
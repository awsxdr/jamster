namespace jamster.Configurations;

public record OverlayConfiguration(float Scale, bool UseBackground, string BackgroundColor, string Language);

public sealed class OverlayConfigurationFactory : IConfigurationFactory<OverlayConfiguration>
{
    public OverlayConfiguration GetDefaultValue() => new(1.0f, false, "#00ff00", "en");
}
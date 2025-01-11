namespace amethyst.Configurations;

public record OverlayConfiguration(float Scale, string Language);

public sealed class OverlayConfigurationFactory : IConfigurationFactory<OverlayConfiguration>
{
    public OverlayConfiguration GetDefaultValue() => new(1.0f, "en");
}
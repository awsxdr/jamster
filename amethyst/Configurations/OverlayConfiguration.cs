namespace amethyst.Configurations;

public record OverlayConfiguration(float Scale);

public sealed class OverlayConfigurationFactory : IConfigurationFactory<OverlayConfiguration>
{
    public OverlayConfiguration GetDefaultValue() => new(1.0f);
}
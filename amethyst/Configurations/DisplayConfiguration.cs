namespace amethyst.Configurations;

public record DisplayConfiguration(bool ShowSidebars, bool UseTextBackgrounds);

public class DisplayConfigurationFactory : IConfigurationFactory<DisplayConfiguration>
{
    public DisplayConfiguration GetDefaultValue() => new(true, true);
}
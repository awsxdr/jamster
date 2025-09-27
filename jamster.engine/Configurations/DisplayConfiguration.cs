namespace jamster.engine.Configurations;

public record DisplayConfiguration(bool ShowSidebars, bool UseTextBackgrounds, string Language);

public class DisplayConfigurationFactory : IConfigurationFactory<DisplayConfiguration>
{
    public DisplayConfiguration GetDefaultValue() => new(true, true, "en");
}
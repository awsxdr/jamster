namespace amethyst.Services;

public interface ISystemTime
{
    DateTimeOffset UtcNow();
}

public class SystemTime : ISystemTime
{
    public DateTimeOffset UtcNow() => DateTimeOffset.Now;
}
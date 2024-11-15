namespace amethyst.Services;

public interface ISystemTime
{
    DateTimeOffset UtcNow();
    long GetTick();
}

public class SystemTime : ISystemTime
{
    public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    public long GetTick() => UtcNow().ToUnixTimeMilliseconds();
}
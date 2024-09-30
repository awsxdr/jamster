namespace amethyst.Domain;

public struct Tick
{
    private readonly long _value;

    public const long MaxValue = long.MaxValue & 0x0000ffffffffffff;
    public const long MinValue = 0;

    public Tick(long value)
    {
        if (value > MaxValue) throw new ArgumentException("Value too large", nameof(value));
        if (value < MinValue) throw new ArgumentException("Value too small", nameof(value));

        _value = value;
    }

    public static implicit operator long(Tick tick) => tick._value;
    public static implicit operator Tick(long tick) => new(tick);
    public static implicit operator Tick(int tick) => new(tick);
}
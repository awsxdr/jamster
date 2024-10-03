namespace amethyst.Domain;

public readonly struct Tick(long value)
{
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    private readonly long _value = value switch
    {
        > MaxValue => throw new ArgumentException("Value too large", nameof(value)),
        < MinValue => throw new ArgumentException("Value too small", nameof(value)),
        _ => value
    };

    public const long MaxValue = long.MaxValue & 0x0000ffffffffffff;
    public const long MinValue = 0;

    public static implicit operator long(Tick tick) => tick._value;
    public static implicit operator Tick(long tick) => new(tick);

    public static bool operator ==(Tick left, Tick right) => left.Equals(right);
    public static bool operator !=(Tick left, Tick right) => !(left == right);

    public readonly bool Equals(Tick other) => _value == other._value;

    public readonly override bool Equals(object? obj) =>
        obj switch
        {
            Tick t => Equals(t),
            long l => Equals(l),
            int i => Equals(i),
            short s => Equals(s),
            byte b => Equals(b),
            ulong ul => Equals((long)ul),
            uint ui => Equals(ui),
            ushort us => Equals(us),
            _ => false
        };

    public readonly override string ToString() => _value.ToString();
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace jamster.Domain;

public readonly struct Tick(long value)
{
    public const long MaxValue = long.MaxValue & 0x0000ffffffffffff;
    public const long MinValue = 0;
    public const int TicksPerSecond = 1000;

    public static uint EqualityVariance { get; set; } = 0;

    private readonly long _value = value;

    public static Tick FromSeconds(float seconds) => (int)(seconds * TicksPerSecond);

    public int Seconds => (int)(_value / TicksPerSecond);

    public static implicit operator long(Tick tick) => tick._value;
    public static implicit operator Tick(long tick) => new(tick);

    public static bool operator ==(Tick left, Tick right) => left.Equals(right);
    public static bool operator !=(Tick left, Tick right) => !(left == right);

    public static Tick operator +(Tick left, Tick right) => new(left._value + right._value);
    public static Tick operator +(Tick left, long right) => new(left._value + right);
    public static Tick operator +(Tick left, int right) => new(left._value + right);
    public static Tick operator -(Tick left, Tick right) => new(left._value - right._value);
    public static Tick operator -(Tick left, long right) => new(left._value - right);
    public static Tick operator -(Tick left, int right) => new(left._value - right);
    public static Tick operator *(Tick left, Tick right) => new(left._value * right._value);
    public static Tick operator *(Tick left, long right) => new(left._value * right);
    public static Tick operator *(Tick left, int right) => new(left._value * right);
    public static Tick operator /(Tick left, Tick right) => new(left._value / right._value);
    public static Tick operator /(Tick left, long right) => new(left._value / right);
    public static Tick operator /(Tick left, int right) => new(left._value / right);

    public bool Equals(Tick other) => _value >= other._value - EqualityVariance && _value <= other._value + EqualityVariance;

    public override bool Equals(object? obj) =>
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

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();
}

public class TickJsonConverter : JsonConverter<Tick>
{
    public override Tick Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TryGetInt64(out var longValue)
            ? longValue
            : long.Parse(reader.GetString() ?? "0");

    public override void Write(Utf8JsonWriter writer, Tick value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}
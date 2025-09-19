using System.Text.Json;
using System.Text.Json.Serialization;
using jamster.Domain;

namespace jamster.Services;

[JsonConverter(typeof(Guid7JsonConverter))]
public class Guid7 : IComparable<Guid>, IComparable<Guid7>
{
    private static readonly Random Random = new();

    private readonly byte[] _data;

    public static Guid7 Empty => Guid.Empty;

    public long Tick
    {
        get
        {
            var timestampBytes = new byte[8];
            _data[0..4].CopyTo(timestampBytes, 2);
            _data[4..6].CopyTo(timestampBytes, 0);
            return BitConverter.ToInt64(timestampBytes);
        }
    }

    public Guid7(byte[] data)
    {
        if (data.Length != 16) throw new InvalidDataSizeException();

        _data = data;
    }

    public static Guid7 NewGuid()
    {
        var tick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return FromTick(tick);
    }

    public static Guid7 FromTick(Tick tick)
    {
        var tickBytes = BitConverter.GetBytes(tick);

        var uuidData = new byte[16];
        Random.NextBytes(uuidData);

        tickBytes[2..6].CopyTo(uuidData, 0);
        tickBytes[..2].CopyTo(uuidData, 4);

        uuidData[6] &= 0b00001111;
        uuidData[6] |= 0b11100000;
        uuidData[7] &= 0b00001111;
        uuidData[7] |= 0b01110000;
        uuidData[8] &= 0b00111111;
        uuidData[8] |= 0b10000000;

        return new Guid7(uuidData);
    }

    public static implicit operator Guid(Guid7 guid) => new Guid(guid._data);
    public static implicit operator Guid7(Guid guid) => new Guid7(guid.ToByteArray());
    public static implicit operator Guid7(Tick tick) => FromTick(tick);
    public static implicit operator Guid7(long tick) => FromTick(tick);

    public byte[] ToByteArray()
    {
        var result = new byte[16];
        _data.CopyTo(result.AsSpan());
        return result;
    }

    public override string ToString() => ((Guid)this).ToString();

    public int CompareTo(Guid other) => ((Guid)this).CompareTo(other);

    public int CompareTo(Guid7? other) => CompareTo((Guid) (other ?? Empty));

    public override bool Equals(object? obj) =>
        obj switch
        {
            Guid7 g7 => ((Guid) this).Equals((Guid) g7),
            Guid g => ((Guid) this).Equals(g),
            _ => false
        };

    public override int GetHashCode() => ((Guid) this).GetHashCode();

    public class InvalidDataSizeException : ArgumentException;
}

internal class Guid7JsonConverter : JsonConverter<Guid7>
{
    public override Guid7? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Guid.TryParse(reader.GetString() ?? "", out var value) ? value : null;

    public override void Write(Utf8JsonWriter writer, Guid7 value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
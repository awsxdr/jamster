using amethyst.Domain;

namespace amethyst.Services;

public class Guid7
{
    private static readonly Random Random = new();

    private readonly byte[] _data;

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

    public class InvalidDataSizeException : ArgumentException;
}
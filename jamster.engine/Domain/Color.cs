using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace jamster.Domain;

[JsonConverter(typeof(ColorJsonConverter))]
public readonly struct Color
{
    public byte Red { get; init; }
    public byte Green { get; init; }
    public byte Blue { get; init; }

    public string HtmlCode => $"#{Red:X2}{Green:X2}{Blue:X2}";

    public static bool TryParse(string value, out Color color)
    {
        var rawValue = value.Trim().TrimStart('#').AsSpan();
        if (rawValue.Length == 3)
        {
            byte green = 0, blue = 0;

            var result =
                byte.TryParse(rawValue.Slice(0, 1), NumberStyles.HexNumber, default, out var red)
                && byte.TryParse(rawValue.Slice(1, 1), NumberStyles.HexNumber, default, out green)
                && byte.TryParse(rawValue.Slice(2, 1), NumberStyles.HexNumber, default, out blue);

            color = new Color { Red = red, Green = green, Blue = blue };

            return result;
        }

        if (rawValue.Length == 6)
        {
            byte green = 0, blue = 0;

            var result =
                byte.TryParse(rawValue.Slice(0, 2), NumberStyles.HexNumber, default, out var red)
                && byte.TryParse(rawValue.Slice(2, 2), NumberStyles.HexNumber, default, out green)
                && byte.TryParse(rawValue.Slice(4, 2), NumberStyles.HexNumber, default, out blue);

            color = new Color { Red = red, Green = green, Blue = blue };

            return result;
        }

        color = default;
        return false;
    }

    public static Color FromRgb(byte red, byte green, byte blue) => 
        new()
        {
            Red = red,
            Green = green,
            Blue = blue
        };

    public static Color Black => Color.FromRgb(0, 0, 0);
    public static Color White => Color.FromRgb(255, 255, 255);

    public bool Equals(Color other) =>
        other.Red == Red
        && other.Green == Green
        && other.Blue == Blue;

    public override bool Equals(object? obj) =>
        obj is Color c && Equals(c);

    public override int GetHashCode() => Red.GetHashCode() ^ Green.GetHashCode() ^ Blue.GetHashCode();

    public override string ToString() => HtmlCode;
}

internal class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Color.TryParse(reader.GetString() ?? "", out var color) ? color : Color.FromRgb(0, 0, 0);

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.HtmlCode);
}
export type HslColor = {
    hue: number;
    saturation: number;
    lightness: number;
}

export type HsvColor = {
    hue: number;
    saturation: number;
    value: number;
}

export class Color {
    public static hsvToHsl(color: HsvColor): HslColor {
        color = this.clampHsvValues(color);

        const lightness = color.value * (1 - color.saturation / 2);
        const saturation =
            lightness === 0 || lightness === 1
            ? 0
            : (color.value - lightness) / Math.min(lightness, 1 - lightness);

        return {
            hue: color.hue,
            saturation,
            lightness
        };
    }

    public static hslToHsv(color: HslColor): HsvColor {
        color = this.clampHslValues(color);

        const value = color.lightness + color.saturation * Math.min(color.lightness, 1 - color.lightness);
        const saturation = value === 0 ? 0 : 2 * (1 - color.lightness / value);

        return {
            hue: color.hue,
            saturation,
            value,
        }
    }

    public static clampHsvValues(color: HsvColor): HsvColor {
        return {
            hue: this.clamp(color.hue, 0, 360),
            saturation: this.clamp(color.saturation, 0, 1),
            value: this.clamp(color.value, 0, 1),
        }
    }

    public static clampHslValues(color: HslColor): HslColor {
        return {
            hue: this.clamp(color.hue, 0, 360),
            saturation: this.clamp(color.saturation, 0, 1),
            lightness: this.clamp(color.lightness, 0, 1),
        }
    }

    private static clamp(value: number, min: number, max: number) {
        return Math.min(Math.max(value, min), max);
    }    
}
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

export type RgbColor = {
    red: number;
    green: number;
    blue: number;
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

    public static hsvToRgb(color: HsvColor): RgbColor {
        color = this.clampHsvValues(color);

        const chroma = color.value * color.saturation;
        const colorComponent = chroma * (1 - Math.abs((color.hue / 60) % 2 - 1));

        const pureColor = color.hue < 60 ? { red: chroma, green: colorComponent, blue: 0 }
            : color.hue < 120 ? { red: colorComponent, green: chroma, blue: 0 }
            : color.hue < 180 ? { red: 0, green: chroma, blue: colorComponent }
            : color.hue < 240 ? { red: 0, green: colorComponent, blue: chroma }
            : color.hue < 300 ? { red: colorComponent, green: 0, blue: chroma }
            : { red: chroma, green: 0, blue: colorComponent };

        const valueAdjustment = color.value - chroma;
        return {
            red: pureColor.red + valueAdjustment,
            green: pureColor.green + valueAdjustment,
            blue: pureColor.blue + valueAdjustment,
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

    public static hslToRgb(color: HslColor): RgbColor {
        color = this.clampHslValues(color);

        const chroma = (1 - Math.abs(color.lightness * 2 - 1)) * color.saturation;
        const colorComponent = chroma * (1 - Math.abs((color.hue / 60) % 2 - 1));

        const pureColor = color.hue < 60 ? { red: chroma, green: colorComponent, blue: 0 }
            : color.hue < 120 ? { red: colorComponent, green: chroma, blue: 0 }
            : color.hue < 180 ? { red: 0, green: chroma, blue: colorComponent }
            : color.hue < 240 ? { red: 0, green: colorComponent, blue: chroma }
            : color.hue < 300 ? { red: colorComponent, green: 0, blue: chroma }
            : { red: chroma, green: 0, blue: colorComponent };

        const lightnessAdjustment = color.lightness - chroma / 2;
        return {
            red: pureColor.red + lightnessAdjustment,
            green: pureColor.green + lightnessAdjustment,
            blue: pureColor.blue + lightnessAdjustment,
        }
    }

    public static hslToString(color: HslColor): string {
        color = this.clampHslValues(color);

        return `hsl(${color.hue}deg ${color.saturation * 100}% ${color.lightness * 100}%)`;
    }

    public static rgbToHsv(color: RgbColor): HsvColor {
        color = this.clampRgbValues(color);

        const maxComponent = Math.max(color.red, color.green, color.blue);
        const minComponent = Math.min(color.red, color.green, color.blue);

        const value = maxComponent;
        const chroma = maxComponent - minComponent;

        let hue = chroma === 0 ? 0
            : maxComponent === color.red ? 60 * (((color.green - color.blue) / chroma) % 6)
            : maxComponent === color.green ? 60 * (((color.blue - color.red) / chroma) + 2)
            : 60 * (((color.red - color.green) / chroma) + 4);

        if(hue < 0) {
            hue += 360;
        }
    
        const saturation = value === 0 ? 0 : chroma / value;
        
        return { hue, saturation, value };
    }

    public static rgbToHsl(color: RgbColor): HslColor {
        color = this.clampRgbValues(color);

        const maxComponent = Math.max(color.red, color.green, color.blue);
        const minComponent = Math.min(color.red, color.green, color.blue);

        const value = maxComponent;
        const chroma = maxComponent - minComponent;

        const lightness = (maxComponent + minComponent) / 2;
        let hue = chroma === 0 ? 0
            : maxComponent === color.red ? 60 * (((color.green - color.blue) / chroma) % 6)
            : maxComponent === color.green ? 60 * (((color.blue - color.red) / chroma) + 2)
            : 60 * (((color.red - color.green) / chroma) + 4);

        if(hue < 0) {
            hue += 360;
        }

        const saturation = 
            lightness === 0 || lightness === 1 ? 0 
            : chroma / (1 - Math.abs(2 * value - chroma - 1));
        
        return { hue, saturation, lightness };
    }

    public static rgbToString(color: RgbColor): string {
        color = this.clampRgbValues(color);

        const toHexComponent = (value: number) => Math.round(value * 255).toString(16).padStart(2, '0');

        return `#${toHexComponent(color.red)}${toHexComponent(color.green)}${toHexComponent(color.blue)}`;
    }

    public static parseRgb(colorString: string): RgbColor | undefined {
        const hexValues = /^#?([0-9a-f]{6})$/.exec(colorString.trim().toLowerCase());

        if(hexValues?.length !== 2) {
            return undefined;
        }

        const value = hexValues[1];

        const [redValue, greenValue, blueValue] = 
            value.length === 3 ? [ `${value[0]}${value[0]}`, `${value[1]}${value[1]}`, `${value[2]}${value[2]}` ]
            : [ value.substring(0, 2), value.substring(2, 4), value.substring(4, 6) ];

        return {
            red: parseInt(redValue, 16) / 255,
            green: parseInt(greenValue, 16) / 255,
            blue: parseInt(blueValue, 16) / 255,
        };
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

    public static clampRgbValues(color: RgbColor): RgbColor {
        return {
            red: this.clamp(color.red, 0, 1),
            green: this.clamp(color.green, 0, 1),
            blue: this.clamp(color.blue, 0, 1),
        };
    }

    private static clamp(value: number, min: number, max: number) {
        return Math.min(Math.max(value, min), max);
    }    
}
import { useMemo } from "react";
import { Button, Popover, PopoverContent, PopoverTrigger } from "@/components/ui"
import { ColorSlider } from "./ColorSlider";
import { Color, HslColor } from "@/types";
import { ColorSpace } from "./ColorSpace";

type ColorSelectButtonProps = {
    color: HslColor;
    onColorChanged: (color: HslColor) => void;
}

export const ColorSelectButton = ({ color, onColorChanged }: ColorSelectButtonProps) => {

    const clampedColor = Color.clampHslValues(color);
    
    const colorString = useMemo(() => `hsl(${clampedColor.hue}deg ${clampedColor.saturation * 100}% ${clampedColor.lightness * 100}%)`, [clampedColor]);

    return (
        <Popover>
            <PopoverTrigger>
                <Button 
                    className="transition-[filter] contrast-100 hover:contrast-75" 
                    style={{ backgroundColor: colorString }}
                />
            </PopoverTrigger>
            <PopoverContent>
                <ColorSlider hue={clampedColor.hue} onHueChanged={h => onColorChanged({ ...color, hue: h })} />
                <ColorSpace color={clampedColor} className="h-[200px]" onColorChanged={onColorChanged} />
            </PopoverContent>
        </Popover>
    )
}
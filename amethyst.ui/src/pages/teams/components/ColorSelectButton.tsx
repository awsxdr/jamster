import { useMemo, ChangeEvent, useState, useEffect } from "react";
import { Button, Input, Label, Popover, PopoverContent, PopoverTrigger, Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui"
import { ColorSlider } from "./ColorSlider";
import { Color, HslColor } from "@/types";
import { ColorSpace } from "./ColorSpace";
import { useI18n } from "@/hooks/I18nHook";
import { cn } from "@/lib/utils";

type ColorSelectButtonProps = {
    color: HslColor;
    title?: string;
    description?: string;
    disabled?: boolean;
    className?: string;
    onColorChanged?: (color: HslColor) => void;
    onOpen?: () => void;
    onClose?: () => void;
}

export const ColorSelectButton = ({ color, title, description, disabled, className, onColorChanged, onOpen, onClose }: ColorSelectButtonProps) => {

    const { translate } = useI18n();

    const clampedColor = useMemo(() => Color.clampHslValues(color), [color]);
    
    const colorString = useMemo(() => Color.rgbToString(Color.hslToRgb(clampedColor)), [clampedColor]);

    const [colorInputString, setColorInputString] = useState(colorString);

    useEffect(() => {
        setColorInputString(colorString);
    }, [colorString]);

    const handleColorStringChange = (event: ChangeEvent<HTMLInputElement>) => {
        const newValue = event.target.value ?? '';

        const newColor = Color.parseRgb(newValue);

        if(newColor) {
            onColorChanged?.(Color.rgbToHsl(newColor));
        }

        setColorInputString(newValue);
    }

    const handleOpenChange = (open: boolean) => {
        if(open) {
            onOpen?.();
        } else {
            onClose?.();
        }
    }

    return (
        <TooltipProvider>
            <Tooltip>
                <Popover onOpenChange={handleOpenChange}>
                    <TooltipTrigger asChild>
                        <PopoverTrigger asChild disabled={disabled}>
                            <Button 
                                className={cn("transition-[filter] contrast-100 hover:contrast-75", className)} 
                                style={{ backgroundColor: colorString }}
                            />
                        </PopoverTrigger>
                    </TooltipTrigger>
                    {
                        title &&
                        <TooltipContent>
                            <p>{title}</p>
                        </TooltipContent>
                    }
                    <PopoverContent className="w-[400px]">
                        {title && <p className="font-bold">{title}</p>}
                        {description && <p className="pb-2">{description}</p>}
                        <ColorSlider hue={clampedColor.hue} onHueChanged={h => onColorChanged?.({ ...color, hue: h })} />
                        <ColorSpace color={clampedColor} className="h-[200px]" onColorChanged={onColorChanged} />
                        <div>
                            <Label>{translate("ColorSelectButton.Hex")}</Label>
                            <Input value={colorInputString} onChange={handleColorStringChange} />
                        </div>
                    </PopoverContent>
                </Popover>
            </Tooltip>
        </TooltipProvider>
    )
}
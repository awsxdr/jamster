import { CSSProperties, MouseEvent, MouseEventHandler, useEffect, useRef, useState } from "react";
import { cn } from "@/lib/utils";
import { Color, HslColor } from "@/types";

type ColorSpaceGradientProps = {
    color: HslColor;
    onMouseDown?: MouseEventHandler;
}

const ColorSpaceGradient = ({ color, onMouseDown }: ColorSpaceGradientProps) => {
    return (
        <>
            <div className="absolute bg-gradient-to-r from-white w-full h-full" style={{'--tw-gradient-to': `hsl(${color.hue}deg 100% 50%)`} as CSSProperties} onMouseDown={onMouseDown}></div>
            <div className="absolute bg-gradient-to-t from-black w-full h-full" onMouseDown={onMouseDown}></div>
        </>
    )
}

type ColorSpaceProps = {
    color: HslColor;
    className?: string;
    onColorChanged?: (color: HslColor) => void;
}

export const ColorSpace = ({ color, className, onColorChanged }: ColorSpaceProps) => {

    const [isDragging, setIsDragging] = useState(false);

    const hsvColor = Color.hslToHsv(color);
    const { saturation, value } = hsvColor;

    const cursorTop = 1 - value;
    const cursorLeft = saturation;

    const pickerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if(!pickerRef.current) {
            return;
        }

        const handleMouseMove = (event: globalThis.MouseEvent) => {
            event.preventDefault();

            const boundingBox = pickerRef.current!.getBoundingClientRect();
            
            const xOffset = event.clientX - boundingBox.left;
            const yOffset = event.clientY - boundingBox.top;

            const newColor = {
                ...hsvColor,
                saturation: xOffset / pickerRef.current!.clientWidth,
                value: 1 - yOffset / pickerRef.current!.clientHeight,
            };

            onColorChanged?.(Color.hsvToHsl(newColor));
        }

        const handleMouseUp = (event: globalThis.MouseEvent) => {
            event.preventDefault();

            setIsDragging(false);
        }

        if(isDragging) {
            document.addEventListener('mousemove', handleMouseMove);
            document.addEventListener('mouseup', handleMouseUp);
        }

        return () => {
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        }

    }, [isDragging]);

    const handleMouseDown = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();

        setIsDragging(true);
    }

    return (
        <div className={cn("w-full h-full relative", className)} onMouseDown={handleMouseDown} ref={pickerRef}>
            <ColorSpaceGradient color={color} />
            <div 
                className="w-[11px] h-[11px] border border-black rounded-[100%] absolute top-[calc(var(--cursor-top)-5px)] left-[calc(var(--cursor-left)-5px)]"
                style={{ '--cursor-top': `${cursorTop * 100}%`, '--cursor-left': `${cursorLeft * 100}%` } as CSSProperties}
            >
                <div className="w-[9px] h-[9px] border border-white rounded-[100%]">
                    <div className="w-[7px] h-[7px] border border-black rounded-[100%]"></div>
                </div>
            </div>
        </div>
    );
}
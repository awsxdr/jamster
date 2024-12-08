import { useEffect, useMemo, useRef, useState, MouseEvent, CSSProperties } from "react";

type ColorSliderProps = {
    hue: number;
    onHueChanged?: (hue: number) => void;
}

export const ColorSlider = ({ hue, onHueChanged }: ColorSliderProps) => {

    const pickerRef = useRef<HTMLDivElement>(null);

    const sliderLeft = useMemo(() =>
        (pickerRef.current?.clientWidth ?? 0) / 360 * Math.min(Math.max(hue, 0), 359) - 3,
        [pickerRef, hue]);

    const [isDragging, setIsDragging] = useState(false);

    useEffect(() => {
        if(!pickerRef.current) {
            return;
        }

        const handleMouseMove = (event: globalThis.MouseEvent) => {
            event.preventDefault();
            
            const xOffset = event.clientX - pickerRef.current!.getBoundingClientRect().left;
            const newHue = Math.min(Math.max(xOffset, 0), pickerRef.current!.clientWidth) / pickerRef.current!.clientWidth * 359;

            onHueChanged?.(newHue);
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

    const handleSliderMouseDown = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();

        setIsDragging(true);
    }

    return (
        <div className="h-[30px] w-full relative">
            <div 
                className="flex h-[20px] w-full" 
                onMouseDown={handleSliderMouseDown}
                ref={pickerRef}
            >
                <div className="bg-gradient-to-r from-[#ff0000] to-[#ffff00] grow"></div>
                <div className="bg-gradient-to-r from-[#ffff00] to-[#00ff00] grow"></div>
                <div className="bg-gradient-to-r from-[#00ff00] to-[#00ffff] grow"></div>
                <div className="bg-gradient-to-r from-[#00ffff] to-[#0000ff] grow"></div>
                <div className="bg-gradient-to-r from-[#0000ff] to-[#ff00ff] grow"></div>
                <div className="bg-gradient-to-r from-[#ff00ff] to-[#ff0000] grow"></div>

                <div 
                    className="w-[7px] h-[30px] border border-black rounded-[2px] absolute top-[-5px] left-[calc(var(--slider-value)-3px)]" 
                    style={{ '--slider-value': `${hue / 360 * 100}%` } as CSSProperties}
                    onMouseDown={handleSliderMouseDown}
                >
                    <div className="w-[5px] h-[28px] border border-white rounded-[1px]">
                        <div className="w-[3px] h-[26px] border border-black">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
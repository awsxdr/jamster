import { Popover, PopoverAnchor, PopoverContent } from "@/components/ui";
import { EllipsisVertical } from "lucide-react";
import { PointerEvent, useRef, useState } from "react";

type TimelineSeparatorProps = {
    onAdjust: (seconds: number) => void;
}

export const TimelineSeparator = ({ onAdjust }: TimelineSeparatorProps) => {
 
    const [isDragging, setIsDragging] = useState(false);
    const [dragStartX, setDragStartX] = useState(0);
    const [adjustSeconds, setAdjustSeconds] = useState(0);

    const dragBarRef = useRef<HTMLDivElement>(null);

    const handleStartDrag = (event: PointerEvent<HTMLDivElement>) => {
        event.preventDefault();

        dragBarRef.current?.setPointerCapture(event.pointerId);
        
        setIsDragging(true);
        setDragStartX(event.clientX);
    }

    const handleDrag = (event: PointerEvent<HTMLDivElement>) => {
        if(!isDragging) {
            return;
        }

        event.preventDefault();

        const difference = event.clientX - dragStartX;
        setAdjustSeconds(Math.floor(difference / 50));
    }

    const handleEndDrag = (event: PointerEvent<HTMLDivElement>) => {
        if(!isDragging) {
            return;
        }

        event.preventDefault();

        dragBarRef.current?.releasePointerCapture(event.pointerId);
        setIsDragging(false);

        if(adjustSeconds !== 0) {
            onAdjust(adjustSeconds);
        }
    }

    return (
        <Popover open={isDragging}>
            <PopoverAnchor asChild>
                <div className="bg-gray-300 dark:bg-gray-600 cursor-ew-resize flex items-center justify-center" ref={dragBarRef} onPointerDown={handleStartDrag} onPointerMove={handleDrag} onPointerUp={handleEndDrag}>
                    <EllipsisVertical size={18} />
                </div>
            </PopoverAnchor>
            <PopoverContent className="w-auto h-auto p-2">
                {adjustSeconds > 0 ? "+" : ""}{adjustSeconds}s
            </PopoverContent>
        </Popover>                
    );
}
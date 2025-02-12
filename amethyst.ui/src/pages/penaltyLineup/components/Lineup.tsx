import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { cn } from "@/lib/utils";
import { LineupSkater, SkaterDropTarget } from ".";
import { useTeamDetailsState } from "@/hooks";
import { TeamSide } from "@/types";

export const Lineup = () => {
    
    const { team } = useTeamDetailsState(TeamSide.Home) ?? {};

    const [isDragging, setIsDragging] = useState(false);
    const [draggingNumber, setDraggingNumber] = useState("");
    const [skaterNumbers, setSkaterNumbers] = useState<string[]>(["", "", "", "", ""]);
    const [pointerPosition, setPointerPosition] = useState({ x: 0, y: 0 });

    const portalRef = useRef<HTMLDivElement>(null);

    const handleMouseMove = (event: MouseEvent) => {
        setPointerPosition({ x: event.clientX, y: event.clientY })
    }

    const handleTouchMove = (event: TouchEvent) => {
        setPointerPosition({ x: event.touches[0]?.clientX ?? 0, y: event.touches[0]?.clientY ?? 0 })
    }

    useEffect(() => {
        if(!isDragging) {
            return;
        }

        window.addEventListener('mousemove', handleMouseMove);
        window.addEventListener('touchmove', handleTouchMove);

        return () => {
            window.removeEventListener('mousemove', handleMouseMove);
            window.removeEventListener('touchmove', handleTouchMove);
        }
    }, [isDragging]);

    const handleDragStart = (number: string) => {
        setIsDragging(true);
        setDraggingNumber(number);
    }

    const handleDragEnd = () => {
        setIsDragging(false);
    }

    const handleDrop = (index: number) => {
        const newNumbers = [...skaterNumbers];
        const existingNumberIndex = newNumbers.findIndex(s => s === draggingNumber);
        if(existingNumberIndex >= 0) {
            newNumbers[existingNumberIndex] = "";
        }
        newNumbers[index] = draggingNumber;
        setSkaterNumbers(newNumbers);
    }

    const handleClear = (index: number) => {
        const newNumbers = [...skaterNumbers];
        newNumbers[index] = "";
        setSkaterNumbers(newNumbers);
    }

    const skaterPositions = ["Jammer", "Pivot", "Blocker", "Blocker", "Blocker"];

    if(!team) {
        return <></>;
    }

    return (
        <>
            { portalRef.current && (
                <>
                    <div className="flex flex-wrap justify-center gap-1">
                        {team.roster.map(s => (
                            <LineupSkater 
                                key={s.number}
                                number={s.number} 
                                portalElement={portalRef.current!} 
                                inLineup={skaterNumbers.includes(s.number)}
                                onDragStart={() => handleDragStart(s.number)} 
                                onDragEnd={handleDragEnd} 
                            />
                        ))}
                    </div>
                    <div className="flex gap-1 md:gap-2 xl:gap-3">
                        { skaterPositions.map((p, i) => (
                            <SkaterDropTarget
                                key={i}
                                title={p}
                                skaterNumber={skaterNumbers[i]}
                                isDragging={isDragging}
                                pointerPosition={pointerPosition}
                                portalElement={portalRef.current!}
                                onDrop={() => handleDrop(i)}
                                onClear={() => handleClear(i)}
                                onDragStart={() => handleDragStart(skaterNumbers[i])}
                                onDragEnd={handleDragEnd}
                            />
                        ))}
                    </div>
                </>
            )}
            { createPortal(
                <div 
                    ref={portalRef} 
                    className={cn(
                        "absolute z-10 left-0 top-0 w-full h-full overflow-hidden",
                        !isDragging && "pointer-events-none",
                        isDragging && "overscroll-none"
                    )}
                >
                </div>,
                document.body
            )}
        </>
    )
}
import { Button, Card } from "@/components/ui";
import { useCallback, useEffect, useRef, useState } from "react";
import { LineupSkater } from "./LineupSkater";
import { cn } from "@/lib/utils";
import { Pointer } from "lucide-react";
import { PenaltyDialog } from "./PenaltyDialog";

type SkaterDropTargetProps = {
    title: string;
    isDragging?: boolean;
    skaterNumber?: string;
    pointerPosition: { x: number, y: number };
    portalElement: Element | DocumentFragment;
    onDrop?: () => void;
    onClear?: () => void;
    onDragStart?: () => void;
    onDragEnd?: () => void;
}

export const SkaterDropTarget = ({ title, isDragging, skaterNumber, pointerPosition, portalElement, onDrop, onClear, onDragStart, onDragEnd }: SkaterDropTargetProps) => {

    const cardRef = useRef<HTMLDivElement>(null);
    const [isOver, setIsOver] = useState(false);
    const [isDraggingThis, setIsDraggingThis] = useState(false);
    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);

    useEffect(() => {
        if(!isDragging || !onDrop || !cardRef.current) {
            return;
        }

        const bounds = cardRef.current.getBoundingClientRect();
        setIsOver(
            pointerPosition.x >= bounds.left && pointerPosition.x <= bounds.right
            && pointerPosition.y >= bounds.top && pointerPosition.y <= bounds.bottom
        );
    }, [pointerPosition]);

    useEffect(() => {
        if(isDragging || !onDrop) {
            return;
        }

        if(isOver) {
            onDrop();
        }
    }, [isDragging]);

    const handleDragStart = () => {
        setIsDraggingThis(true);
        onDragStart?.();
    }

    const handleDragEnd = useCallback(() => {
        if(!isDraggingThis) {
            return;
        }

        setIsDraggingThis(false);
        onClear?.();
        onDragEnd?.();
    }, [isDraggingThis]);

    const handlePenaltySelected = (_code: string) => {
        setPenaltyDialogOpen(false);
    }

    return (
        <Card ref={cardRef} className="bg-accent p-1 text-center flex flex-col gap-2 grow">
            <div>{title}</div>
            <LineupSkater 
                number={skaterNumber ?? ""} 
                portalElement={portalElement} 
                className={cn(
                    "w-full p-0 rounded-lg grow flex justify-center items-center",
                    isDragging && "border border-dashed border-primary",
                    isOver && "border-solid"
                )}
                onDragStart={handleDragStart} 
                onDragEnd={handleDragEnd}
                onDoubleClick={onClear}
            />
            <PenaltyDialog open={penaltyDialogOpen} onOpenChanged={setPenaltyDialogOpen} onPenaltySelected={handlePenaltySelected} />
        </Card>
    );
}
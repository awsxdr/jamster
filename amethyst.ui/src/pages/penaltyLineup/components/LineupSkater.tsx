import { MouseEvent, TouchEvent, useCallback, useEffect, useRef, useState } from 'react';
import { Button } from "@/components/ui";
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';
import { TriangleAlert } from 'lucide-react';

type LineupSkaterProps = {
    number: string;
    inLineup?: boolean;
    warning?: string;
    portalElement: Element | DocumentFragment;
    className?: string;
    onDragStart?: () => void;
    onDragEnd?: () => void;
    onDoubleClick?: () => void;
}

const DRAG_DEAD_ZONE = Math.pow(5, 2);

export const LineupSkater = ({ number, inLineup, warning, portalElement, className, onDragStart, onDragEnd, onDoubleClick }: LineupSkaterProps) => {
    const [isMouseDown, setIsMouseDown] = useState(false);
    const [isDragging, setIsDragging] = useState(false);
    const buttonRef = useRef<HTMLButtonElement>(null);
    const draggableRef = useRef<HTMLButtonElement>(null);
    const [mouseStartPosition, setMouseStartPosition] = useState({ x: 0, y: 0 });
    const [dragButtonSize, setDragButtonSize] = useState({ width: 0, height: 0 });

    const handleMouseDown = (event: MouseEvent<HTMLButtonElement>) => {
        if(!buttonRef.current) {
            return;
        }

        handleDragStart(event.clientX, event.clientY);
    }

    const handleTouchStart = (event: TouchEvent<HTMLButtonElement>) => {
        if(!buttonRef.current) {
            return;
        }

        handleDragStart(event.touches[0].clientX, event.touches[0].clientY);
    }

    const handleDragStart = (x: number, y: number) => {
        if(!buttonRef.current) {
            return;
        }

        const buttonBounds = buttonRef.current.getBoundingClientRect();
        setMouseStartPosition({ x: x - buttonBounds.x, y: y - buttonBounds.y });
        setDragButtonSize({ width: buttonBounds.width, height: buttonBounds.height });

        if(draggableRef.current) {
            draggableRef.current.style.left = `${x - mouseStartPosition.x}px`;
            draggableRef.current.style.top = `${y - mouseStartPosition.y}px`;
        }

        setIsMouseDown(true);
    }

    useEffect(() => {
        if(!draggableRef.current) {
            return;
        }

        draggableRef.current.style.width = `${dragButtonSize.width}px`;
        draggableRef.current.style.height = `${dragButtonSize.height}px`;

    }, [draggableRef.current, dragButtonSize]);

    const handleDragEnd = useCallback(() => {
        if(!buttonRef.current || !isMouseDown) {
            return;
        }

        setIsMouseDown(false);
        if(isDragging) {
            onDragEnd?.();
            setIsDragging(false);
        }
    }, [isDragging, isMouseDown]);

    const handleMouseMove = (event: globalThis.MouseEvent) => {
        if(!isMouseDown) {
            return;
        }

        handleMove(event.clientX, event.clientY);
    }

    const handleTouchMove = (event: globalThis.TouchEvent) => {
        if(!isMouseDown) {
            return;
        }

        handleMove(event.touches[0].clientX, event.touches[0].clientY);
    }

    const handleMove = (x: number, y: number) => {
        if(!buttonRef.current || !isMouseDown) {
            return;
        }

        if(!isDragging) {
            const buttonBounds = buttonRef.current.getBoundingClientRect();
            const totalMovement = Math.pow(x - mouseStartPosition.x - buttonBounds.x, 2) + Math.pow(y - mouseStartPosition.y - buttonBounds.y, 2);

            if(totalMovement > DRAG_DEAD_ZONE) {
                setIsDragging(true);
                onDragStart?.();
            }
        }

        if(draggableRef.current) {
            draggableRef.current.style.left = `${x - mouseStartPosition.x}px`;
            draggableRef.current.style.top = `${y - mouseStartPosition.y}px`;
            draggableRef.current.style.width = `${dragButtonSize.width}px`;
            draggableRef.current.style.height = `${dragButtonSize.height}px`;
        }
    }

    useEffect(() => {
        window.addEventListener('touchmove', handleTouchMove);
        window.addEventListener('touchend', handleDragEnd);
        window.addEventListener('mousemove', handleMouseMove)
        window.addEventListener('mouseup', handleDragEnd);

        return () => {
            window.removeEventListener('touchmove', handleTouchMove);
            window.removeEventListener('touchend', handleDragEnd);
            window.removeEventListener('mousemove', handleMouseMove)
            window.removeEventListener('mouseup', handleDragEnd);
        }
    }, [isMouseDown, handleDragEnd]);

    const content = <span className="flex gap-1">{warning && <TriangleAlert className="text-yellow-600" />} {number}</span>

    if(inLineup) {
        return (
            <Button
                variant="outline"
                className="bg-accent"
                disabled
            >
                <span className="invisible">{content}</span>
            </Button>
        )
    }

    return (
        <>
            <Button 
                ref={buttonRef} 
                variant="outline" 
                className={cn(
                    isDragging && "border-dashed border border-primary",
                    className
                )}
                onMouseDown={handleMouseDown}
                onTouchStart={handleTouchStart}
                onDoubleClick={onDoubleClick}
            >
                <span className={cn(isDragging && "invisible")}>{content}</span>
            </Button>
            { isDragging && createPortal(
                <Button ref={draggableRef} variant="outline" className={cn("absolute opacity-80", !isDragging && "hidden")}>
                    <span>{content}</span>
                </Button>
                , portalElement
            )}
        </>
    );
}
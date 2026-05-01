import { PointerEvent, useCallback, useEffect, useRef, useState } from 'react';
import { Button, ButtonProps } from "@/components/ui"

type RepeaterButtonProps = ButtonProps & {
    onEditingStart?: () => void;
    onEditingEnd?: () => void;
};

const INITIAL_INPUT_DELAY = 600;
const MINIMUM_DELAY = 75;

export const RepeaterButton = ({ 
    onPointerDown,
    onPointerUp,
    onClick,
    onEditingStart, 
    onEditingEnd, 
    ...props 
}: RepeaterButtonProps) => {

    const [buttonDown, setButtonDown] = useState(false);
    const [pointerDownEvent, setPointerDownEvent] = useState<PointerEvent<HTMLButtonElement>>();
    const buttonRef = useRef<HTMLButtonElement>(null);

    const handlePointerDown = (event: PointerEvent<HTMLButtonElement>) => {
        onPointerDown?.(event);

        if(event.defaultPrevented) {
            return;
        }

        buttonRef.current?.setPointerCapture(event.pointerId);

        event.preventDefault();

        onEditingStart?.();

        onClick?.(event);

        setPointerDownEvent(event);
        setButtonDown(true);
    };

    const handlePointerUp = (event: PointerEvent<HTMLButtonElement>) => {
        onPointerUp?.(event);

        if(event.defaultPrevented) {
            return;
        }

        buttonRef.current?.releasePointerCapture(event.pointerId);

        event.preventDefault();

        onEditingEnd?.();

        setPointerDownEvent(undefined);
        setButtonDown(false);
    }

    const buttonDownRef = useRef(buttonDown);
    buttonDownRef.current = buttonDown;

    const repeatInput = useCallback((nextDelay: number) => {
        if(!buttonDownRef.current || !pointerDownEvent) {
            return;
        }

        onClick?.(pointerDownEvent);

        const inputDelay = Math.max(MINIMUM_DELAY, nextDelay / 2);

        setTimeout(() => repeatInput(inputDelay), inputDelay);
    }, [buttonDown]);

    useEffect(() => {

        if(!buttonDown) {
            return;
        }

        setTimeout(() => repeatInput(INITIAL_INPUT_DELAY), INITIAL_INPUT_DELAY);

    }, [buttonDown]);

    return (
        <Button 
            ref={buttonRef}
            onPointerDown={handlePointerDown}
            onPointerUp={handlePointerUp}
            onClick={onClick} 
            {...props} 
        />
    );
}
import { KeyboardEvent, useEffect, useRef } from "react";

type EditableCellProps = {
    value: string;
    disabled?: boolean;
    className?: string;
    onValueChanged?: (value: string) => void;
}

export const EditableCell = ({ value: initialValue, disabled, className, onValueChanged }: EditableCellProps) => {

    const divRef = useRef<HTMLDivElement>(null);

    const handleBlur = () => {
        if(!divRef.current) {
            return;
        }

        const value = divRef.current.innerText;
        if(value === initialValue) {
            return;
        }

        onValueChanged?.(value);
    }

    const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
        if(!divRef.current) {
            return;
        }

        if(event.key === "Enter") {
            event.preventDefault();
            divRef.current.blur();
        } else if(event.key === "Escape") {
            event.preventDefault();
            divRef.current.innerText = initialValue;
            divRef.current.blur();
        }
    }

    useEffect(() => {
        if(!divRef.current) {
            return;
        }

        divRef.current.innerText = initialValue;
    }, [initialValue, divRef]);
    
    return (
        <div contentEditable={!disabled} ref={divRef} className={className} onBlur={handleBlur} onKeyDown={handleKeyDown} />
    )
}
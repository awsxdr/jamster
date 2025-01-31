import { cn } from "@/lib/utils";
import { KeyboardEvent, MouseEvent } from "react";

type EditableCellProps = {
    checked: boolean;
    disabled?: boolean;
    className?: string;
    onCheckedChanged?: (checked: boolean) => void;
}

export const CheckCell = ({ checked, disabled, className, onCheckedChanged }: EditableCellProps) => {

    const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
        if(event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            if(disabled) { 
                return;
            }
            onCheckedChanged?.(!checked);
        }
    }

    const handleClick = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        if(disabled) { 
            return;
        }
        onCheckedChanged?.(!checked);
    }
    
    return (
        <div tabIndex={0} className={cn("cursor-default", className)} onKeyDown={handleKeyDown} onClick={handleClick}>
            { checked ? "X" : " " }
        </div>
    )
}
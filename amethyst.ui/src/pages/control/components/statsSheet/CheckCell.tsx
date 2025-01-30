import { cn } from "@/lib/utils";
import { KeyboardEvent, MouseEvent } from "react";

type EditableCellProps = {
    checked: boolean;
    className?: string;
    onCheckedChanged?: (checked: boolean) => void;
}

export const CheckCell = ({ checked, className, onCheckedChanged }: EditableCellProps) => {

    const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
        if(event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            onCheckedChanged?.(!checked);
        }
    }

    const handleClick = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        onCheckedChanged?.(!checked);
    }
    
    return (
        <div tabIndex={0} className={cn("cursor-default", className)} onKeyDown={handleKeyDown} onClick={handleClick}>
            { checked ? "X" : " " }
        </div>
    )
}
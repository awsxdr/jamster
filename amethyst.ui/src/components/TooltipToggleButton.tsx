import { forwardRef, PropsWithChildren, ReactNode } from "react";
import { Toggle, Tooltip, TooltipContent, TooltipTrigger } from "./ui";
import { ToggleProps } from "@radix-ui/react-toggle";

export type TooltipToggleButtonProps = {
    description: ReactNode;
    ref?: React.LegacyRef<HTMLButtonElement>;
} & ToggleProps;

export const TooltipToggleButton = forwardRef<HTMLButtonElement, TooltipToggleButtonProps>(({ 
        children, 
        description,
        ...props 
    }: PropsWithChildren<TooltipToggleButtonProps>,
    ref
) => {
    return (
        <Tooltip>
            <TooltipTrigger>
                <Toggle {...props} ref={ref}>
                    { children }
                </Toggle>
            </TooltipTrigger>
            <TooltipContent className="max-w-72 bg-accent text-accent-foreground border-accent-foreground shadow-xl">
                {description}
            </TooltipContent>
        </Tooltip>
    );
});
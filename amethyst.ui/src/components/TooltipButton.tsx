import { forwardRef, PropsWithChildren, ReactNode } from "react";
import { Button, ButtonProps, Tooltip, TooltipContent, TooltipTrigger } from "./ui";

export type TooltipButtonProps = {
    description: ReactNode;
    ref?: React.LegacyRef<HTMLButtonElement>;
} & ButtonProps;

export const TooltipButton = forwardRef<HTMLButtonElement, TooltipButtonProps>(({ 
        children, 
        description,
        ...props 
    }: PropsWithChildren<TooltipButtonProps>,
    ref
) => {
    return (
        <Tooltip>
            <TooltipTrigger asChild>
                <Button {...props} ref={ref}>
                    { children }
                </Button>
            </TooltipTrigger>
            <TooltipContent className="max-w-72 bg-accent text-accent-foreground border-accent-foreground shadow-xl">
                {description}
            </TooltipContent>
        </Tooltip>
    );
});
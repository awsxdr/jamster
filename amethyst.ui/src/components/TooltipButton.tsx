import { PropsWithChildren, ReactNode } from "react";
import { Button, ButtonProps, Tooltip, TooltipContent, TooltipTrigger } from "./ui";

export type TooltipButtonProps = {
    description: ReactNode;
} & ButtonProps;

export const TooltipButton = ({ 
        children, 
        description,
        ...props 
    }: PropsWithChildren<TooltipButtonProps>
) => {
    return (
        <Tooltip>
            <TooltipTrigger asChild>
                <Button {...props}>
                    { children }
                </Button>
            </TooltipTrigger>
            <TooltipContent className="max-w-72 bg-accent text-accent-foreground border-accent-foreground shadow-xl">
                {description}
            </TooltipContent>
        </Tooltip>
    );
}
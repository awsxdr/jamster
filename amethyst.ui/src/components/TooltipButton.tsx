import { forwardRef, PropsWithChildren, ReactNode } from "react";
import { Button, ButtonProps, Tooltip, TooltipContent, TooltipTrigger } from "./ui";
import { cn } from "@/lib/utils";

export type TooltipButtonProps = {
    description: ReactNode;
    notify?: boolean;
} & ButtonProps;

export const TooltipButton = forwardRef<HTMLButtonElement, TooltipButtonProps>(({ 
        children, 
        description,
        notify,
        className,
        disabled,
        ...props 
    }: PropsWithChildren<TooltipButtonProps>,
    ref
) => {
    return disabled ? (
        <Button disabled {...props} className={className} ref={ref}>
            { children }
        </Button>
    ) : (
        <Tooltip>
            <TooltipTrigger asChild>
                <div className="relative inline">
                    { notify && (
                        <Button className={cn(className, "animate-ping-small pointer-events-none absolute left-0 top-0")} {...props} ref={ref}>
                            { children }
                        </Button>
                    )}
                    <Button {...props} className={cn(className)} ref={ref}>
                        { children }
                    </Button>
                </div>
            </TooltipTrigger>
            <TooltipContent className="max-w-72 bg-accent text-accent-foreground border-accent-foreground shadow-xl">
                {description}
            </TooltipContent>
        </Tooltip>
    );
});
TooltipButton.displayName = "";

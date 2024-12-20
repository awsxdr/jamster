import { cn } from "@/lib/utils";
import { PropsWithChildren } from "react";

type ClocksBarProps = {
    visible: boolean;
    className?: string;
}

export const ClocksBar = ({ visible, className, children }: PropsWithChildren<ClocksBarProps>) => {
    return (
        <div className={cn("flex justify-around overflow-hidden p-0 h-0 transition-all duration-250", visible ? "h-[25vh]" : "-mt-5", className)}>
            { children }
        </div>
    );
}
import { cn } from "@/lib/utils";
import { PropsWithChildren } from "react";

type ClocksBarProps = {
    visible: boolean;
    className?: string;
}

export const ClocksBar = ({ visible, className, children }: PropsWithChildren<ClocksBarProps>) => {
    return (
        <div className={cn("absolute left-0 right-0 top-0 bottom-0 flex justify-around overflow-hidden hidden", visible ? "block" : "", className)}>
            { children }
        </div>
    );
}
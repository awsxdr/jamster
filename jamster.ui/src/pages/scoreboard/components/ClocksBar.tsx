import { cn } from "@/lib/utils";
import { ReactNode } from "react";

type ClocksBarProps = {
    visible: boolean;
    className?: string;
    topPanel?: ReactNode;
    bottomPanel?: ReactNode;
}

export const ClocksBar = ({ visible, className, topPanel, bottomPanel }: ClocksBarProps) => {
    return (
        <div className={cn("absolute left-0 right-0 top-0 bottom-0 flex justify-around overflow-hidden hidden font-bold", visible ? "block" : "", className)}>
            <div className="h-[40%] flex">
                { topPanel }
            </div>
            <div className="h-[60%] flex">
                { bottomPanel }
            </div>
        </div>
    );
}
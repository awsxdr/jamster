import { cn } from "@/lib/utils";
import { Stage } from "@/types";
import { forwardRef } from "react";

type TimelineItemProps = {
    stage: Stage;
    duration: number;
    scale: number;
    final?: boolean;
}

export const TimelineItem = forwardRef<HTMLDivElement, TimelineItemProps>(({ stage, duration, scale, final }: TimelineItemProps, ref) => {

    const formatTime = (ticks: number) => {
        const totalSeconds = Math.floor(ticks / 1000);
        const seconds = totalSeconds % 60;
        const minutes = Math.floor(totalSeconds / 60);

        return minutes ? `${minutes}:${seconds}` : `${seconds}`;
    }

    const backgroundColor = (() => {
        if (stage === Stage.Jam) return "bg-green-100 dark:bg-green-800";
        else if (stage === Stage.Lineup) return "bg-blue-100 dark:bg-blue-800";
        else if (stage === Stage.Timeout) return "bg-orange-100 dark:bg-orange-800";
        else return "bg-gray-100 dark:bg-gray-800";
    })();

    return (
        <div className="flex">
            <div 
                style={
                    {
                        "--width": final ? "20vw" : `${duration * scale / 1000}px`
                    } as React.CSSProperties
                }
                className={cn(backgroundColor, "border border-gray-400 dark:border-gray-800 w-[--width]")}
            >
                <div className="text-gray-700 dark:text-gray-100 text-xs [writing-mode:vertical-rl] pt-1">
                    {stage.toString()} ({final ? "Running" : formatTime(duration)})
                </div>
            </div>
            <div className="w-0" ref={ref}>
            </div>
        </div>
    )
});
TimelineItem.displayName = "";
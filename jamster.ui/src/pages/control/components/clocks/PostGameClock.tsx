import { usePostGameClockState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";

type PostGameClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">

export const PostGameClock = (props: PostGameClockProps) => {

    const clockState = usePostGameClockState();

    return (
        <Clock
            id="PostGameClock"
            seconds={clockState?.secondsPassed}
            isRunning={clockState?.isRunning ?? false}
            direction="up"
            {...props}
        />
    );
}

import { PostGameClockState } from "@/types/PostGameClockState";
import { Clock, ClockProps } from "./Clock";
import { usePostGameClockState } from "@/hooks";

type PostGameClockProps = Omit<ClockProps<PostGameClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const PostGameClock = (props: PostGameClockProps) => {
    const clockState = usePostGameClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsPassed}
            state={clockState}
            direction="up"
            {...props}
        />
    );
}

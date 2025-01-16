import { Button, Card, Input } from "@/components/ui";
import { EventStreamEvent } from "@/hooks";
import { Trash } from "lucide-react";
import { useEffect, useState } from "react";
import { EventBody } from "./EventBody";

type EventListItemProps = {
    event: EventStreamEvent;
    minTime: number;
    onEventMoved: (eventId: string, newTick: number) => void;
    onEventDeleted: (eventId: string) => void;
}

const TIME_REGEX = /^((?<h>\d+):)?((?<m>\d{1,2}):)?(?<s>\d{1,2})(\.(?<ms>\d+))?$/;

export const EventListItem = ({ event, minTime, onEventMoved, onEventDeleted }: EventListItemProps) => {
    const getTime = (tick: number) => {
        const hours = Math.floor(tick / (60 * 60 * 1000));
        const minutes = Math.floor(tick / (60 * 1000)) % 60;
        const seconds = Math.floor(tick / 1000) % 60;
        const milliseconds = tick % 1000;

        const pad = (value: number, length: number) => value.toString().padStart(length, '0');

        return `${pad(hours, 2)}:${pad(minutes, 2)}:${pad(seconds, 2)}.${pad(milliseconds, 4)}`
    }

    const [timeCode, setTimeCode] = useState(getTime(event.tick - minTime));

    const handleTimeBlur = () => {
        const match = timeCode.match(TIME_REGEX);
        
        if(!match) {
            setTimeCode(getTime(event.tick - minTime));
            return;
        }

        const newTick = 
            parseInt(match.groups?.['h'] ?? '0') * 60 * 60 * 1000
            + parseInt(match.groups?.['m'] ?? '0') * 60 * 1000
            + parseInt(match.groups?.['s'] ?? '0') * 1000
            + parseInt(match.groups?.['ms'].padEnd(4, '0') ?? '0')
            + minTime;

        onEventMoved(event.id, newTick);
    }

    useEffect(() => {
        setTimeCode(getTime(event.tick - minTime));
    }, [event]);

    return (
        <div className="flex gap-2">
            <Card className="p-2 grow flex flex-col gap-2">
                <div className="flex">
                    <div className="grow">{event.type} - {event.id}</div>
                    <Input className="w-auto text-right" value={timeCode} onChange={e => setTimeCode(e.target.value)} onBlur={handleTimeBlur} />
                    <Button size="icon" variant="ghost" onClick={() => onEventDeleted(event.id)}><Trash color="red" /></Button>
                </div>
                { event.body && <EventBody body={event.body} />}
            </Card>
        </div>
    );
}
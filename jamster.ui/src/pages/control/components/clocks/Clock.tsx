import { ChangeEvent, useMemo, useState } from "react";
import { Button, Input } from "@/components/ui";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { Minus, Plus } from "lucide-react";
import { RepeaterButton } from "../RepeaterButton";

export type ClockProps = {
    id?: string;
    name: string;
    editing?: boolean;
    seconds?: number;
    isRunning: boolean;
    direction: "down" | "up";
    startValue?: number;
    className?: string;
    onClockSet?: (seconds: number) => void;
}

export const Clock = ({ id, name, editing, seconds, isRunning, direction, startValue, className, onClockSet }: ClockProps) => {

    const [isEditing, setIsEditing] = useState(false);
    const [editValue, setEditValue] = useState(0);
    const [inputValue, setInputValue] = useState('');

    const calculateTotalSeconds = () => 
        seconds
            ? direction === 'up' ? seconds : ((startValue ?? 0) - seconds)
            : 0;

    const formatTime = (totalSeconds: number) => {

        const nonNegativeTotalSeconds = Math.max(0, totalSeconds);

        const minutes = Math.floor(nonNegativeTotalSeconds / 60);
        const seconds = nonNegativeTotalSeconds % 60;

        return minutes > 0 ? `${minutes}:${seconds.toString().padStart(2, '0')}` : `${seconds}`;
    }

    const time = useMemo(() => {
        if(seconds === undefined) {
            return '0';
        }

        const totalSeconds = 
            isEditing 
                ? editValue
                : calculateTotalSeconds();

        return formatTime(totalSeconds);
    }, [seconds, direction, startValue, isEditing, editValue]);

    const handleAdjustStart = () => {
        setEditValue(calculateTotalSeconds());
        setIsEditing(true);
    }

    const handleAdjustEnd = () => {
        setIsEditing(false);

        onClockSet?.(editValue);
    }

    const handleAdjustUp = () => setEditValue(v => v + 1);
    const handleAdjustDown = () => setEditValue(v => Math.max(0, v - 1));

    const handleSetClicked = () => {
        const parsedInput = parseInput();

        if(parsedInput) {
            onClockSet?.(parsedInput);
            setInputValue('');
        }
    }

    const handleInputChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setInputValue(event.target.value);
    }

    const handleInputBlur = () => {
        const parsedInput = parseInput();

        if(!parsedInput) {
            setInputValue('');
        } else {
            setInputValue(formatTime(parsedInput))
        }
    }

    const parseInput = () => {
        const match = inputValue.match(/^(\d+:[0-5]\d|\d+)$/);

        if(!match) {
            return undefined;
        } else {
            const values = match[0].split(':');

            const parsedSeconds = parseInt(values.length === 2 ? values[1] : values[0]);
            const parsedMinutes = values.length === 2 ? parseInt(values[0]) : 0;

            return parsedMinutes * 60 + parsedSeconds;
        }
    }

    return (
        <Card className={cn("transition-[border]", className, isRunning && "border-t-primary border-t-8")}>
            <CardContent className={cn("flex flex-col gap-2 p-4", isRunning && "mt-[-8px]")}>
                <div className="items-center justify-between flex-col xl:flex-row lg:flex lg:flex-wrap lg:gap-2">
                    <span className="text-base text-center block">{name}</span>
                    <div className="flex flex-col gap-1 grow">
                        {!!editing && (
                            <div className="text-center">
                                <RepeaterButton 
                                    id={id ? `${id}.Name` : undefined}
                                    size="icon" 
                                    variant="secondary" 
                                    className="p-0 h-5 w-full" 
                                    onEditingStart={handleAdjustStart} 
                                    onEditingEnd={handleAdjustEnd}
                                    onClick={handleAdjustUp}
                                >
                                    <Plus className="w-[8px] h-[8px]" />
                                </RepeaterButton>
                            </div>
                        )}
                        <span id={id} className="block text-center xl:text-right h-full text-2xl lg:text-3xl xl:text-4xl">{time}</span>
                        {!!editing && (
                            <div className="text-center">
                                <RepeaterButton 
                                    size="icon" 
                                    variant="secondary" 
                                    className="p-0 h-5 w-full"
                                    onEditingStart={handleAdjustStart} 
                                    onEditingEnd={handleAdjustEnd}
                                    onClick={handleAdjustDown}
                                >
                                    <Minus className="w-[8px] h-[8px]" />
                                </RepeaterButton>
                            </div>
                        )}
                    </div>
                </div>
                {!!editing && (
                    <div className="flex flex-col sm:flex-row gap-2 items-center">
                        <Input placeholder="12:34" className="text-right" value={inputValue} onChange={handleInputChanged} onBlur={handleInputBlur} />
                        <Button variant="secondary" size="sm" onClick={handleSetClicked} disabled={inputValue === ''}>Set</Button>
                    </div>
                )}
            </CardContent>
        </Card>
    )
}
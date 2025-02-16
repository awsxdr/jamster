import { Button, Command, CommandItem, CommandList, Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, ScrollArea } from "@/components/ui";
import { useEffect, useState } from "react";

type Penalty = {
    code: string;
    name: string;
}

const PENALTIES: Penalty[] = [
    { code: "?", name: "Unknown" },
    { code: "A", name: "High block" },
    { code: "B", name: "Back block" },
    { code: "C", name: "Illegal contact" },
    { code: "D", name: "Direction" },
    { code: "E", name: "Leg block" },
    { code: "F", name: "Forearm" },
    { code: "G", name: "Misconduct" },
    { code: "H", name: "Head block" },
    { code: "I", name: "Illegal procedure" },
    { code: "L", name: "Low block" },
    { code: "M", name: "Multiplayer" },
    { code: "N", name: "Interference" },
    { code: "P", name: "Illegal position" },
    { code: "X", name: "Cut" },
];

export type RecordedPenalty = {
    penaltyCode: string;
    period: number;
    jam: number;
}

type PenaltyDialogProps = {
    open: boolean;
    currentPenalty?: RecordedPenalty;
    onOpenChanged?: (open: boolean) => void;
    onAccept?: (penalty: RecordedPenalty) => void;
}

export const PenaltyDialog = ({ open, currentPenalty, onOpenChanged, onAccept }: PenaltyDialogProps) => {

    const [penaltyCode, setPenaltyCode] = useState("?");
    const [period, setPeriod] = useState(1);
    const [jam, setJam] = useState(1);

    useEffect(() => {
        if(currentPenalty) {
            setPenaltyCode(currentPenalty.penaltyCode);
            setPeriod(currentPenalty.period);
            setJam(currentPenalty.jam);
        } else {
            setPenaltyCode("?");
            setPeriod(1);
            setJam(1);
        }

    }, [currentPenalty]);

    const minPeriod = 1;
    const maxPeriod = 2;

    const handlePenaltySelected = (value: string) => {
        setPenaltyCode(value);
    }

    const handleAccept = () => {
        onAccept?.({ penaltyCode, period, jam });
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChanged}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        Add penalty
                    </DialogTitle>
                    <DialogDescription>
                        Pick a penalty to add to the skater
                    </DialogDescription>
                </DialogHeader>
                <div>
                    <div className="flex justify-around">
                        <div className="flex gap-2 items-center">
                            Period
                            <Button size="sm" className="h-5 w-5 p-0" onClick={() => setPeriod(p => Math.max(minPeriod, p - 1))}>-</Button>
                            <span className="text-lg">{period}</span>
                            <Button size="sm" className="w-5 h-5 p-0" onClick={() => setPeriod(p => Math.min(maxPeriod, p + 1))}>+</Button>
                        </div>
                        <div className="flex gap-2 items-center">
                            Jam
                            <Button size="sm" className="h-5 w-5 p-0" onClick={() => setJam(j => Math.max(1, j - 1))}>-</Button>
                            <span className="text-lg">{jam}</span>
                            <Button size="sm" className="w-5 h-5 p-0" onClick={() => setJam(j => j + 1)}>+</Button>
                        </div>
                    </div>
                    <ScrollArea className="h-full max-h-[60vh]">
                        <Command value={penaltyCode}>
                            <CommandList className="h-full max-h-full">
                                { PENALTIES.map(p => (
                                    <CommandItem key={p.code} value={p.code} onSelect={handlePenaltySelected}>
                                        <span className="font-bold">{p.code}</span> - {p.name}
                                    </CommandItem>
                                ))}
                            </CommandList>
                        </Command>
                    </ScrollArea>
                </div>
                <DialogFooter>
                    <DialogClose asChild>
                        <Button variant="secondary">Cancel</Button>
                    </DialogClose>
                    <DialogClose asChild>
                        <Button onClick={handleAccept}>Add</Button>
                    </DialogClose>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
import { Button, Command, CommandItem, CommandList, Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, ScrollArea } from "@/components/ui";
import { useGameStageState, useGameSummaryState } from "@/hooks";
import { Penalty } from "@/types";
import { useEffect, useState } from "react";

type PenaltyDefinition = {
    code: string;
    nameKey: string;
}

const PENALTIES: PenaltyDefinition[] = [
    { code: "?", nameKey: "Unknown" },
    { code: "A", nameKey: "High block" },
    { code: "B", nameKey: "Back block" },
    { code: "C", nameKey: "Illegal contact" },
    { code: "D", nameKey: "Direction" },
    { code: "E", nameKey: "Leg block" },
    { code: "F", nameKey: "Forearm" },
    { code: "G", nameKey: "Misconduct" },
    { code: "H", nameKey: "Head block" },
    { code: "I", nameKey: "Illegal procedure" },
    { code: "L", nameKey: "Low block" },
    { code: "M", nameKey: "Multiplayer" },
    { code: "N", nameKey: "Interference" },
    { code: "P", nameKey: "Illegal position" },
    { code: "X", nameKey: "Cut" },
];

type PenaltyDialogProps = {
    open: boolean;
    currentPenalty?: Penalty;
    onOpenChanged?: (open: boolean) => void;
    onAccept?: (penalty: Penalty) => void;
}

export const PenaltyDialog = ({ open, currentPenalty, onOpenChanged, onAccept }: PenaltyDialogProps) => {

    const [penaltyCode, setPenaltyCode] = useState("?");
    const [period, setPeriod] = useState(1);
    const [jam, setJam] = useState(1);
    const { periodJamCounts } = useGameSummaryState() ?? { periodJamCounts: [] };
    const { periodNumber } = useGameStageState() ?? { periodNumber: 1 };
    
    useEffect(() => {
        if(currentPenalty) {
            setPenaltyCode(currentPenalty.code);
            setPeriod(currentPenalty.period);
            setJam(currentPenalty.jam);
        } else {
            setPenaltyCode("?");
            setPeriod(1);
            setJam(1);
        }

    }, [currentPenalty]);


    const handlePenaltySelected = (value: string) => {
        setPenaltyCode(value);
        onAccept?.({ code: value, period, jam, served: false })
        onOpenChanged?.(false);
    }

    const handleAccept = () => {
        onAccept?.({ code: penaltyCode, period, jam, served: false });
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
                            <Button 
                                size="sm" 
                                className="h-5 w-5 p-0" 
                                disabled={period === 1}
                                onClick={() => setPeriod(p => Math.max(1, p - 1))}
                            >
                                -
                            </Button>
                            <span className="text-lg">{period}</span>
                            <Button 
                                size="sm" 
                                className="w-5 h-5 p-0" 
                                disabled={period >= periodNumber}
                                onClick={() => setPeriod(p => Math.min(periodNumber, p + 1))}
                            >
                                +
                            </Button>
                        </div>
                        <div className="flex gap-2 items-center">
                            Jam
                            <Button 
                                size="sm" 
                                className="h-5 w-5 p-0" 
                                disabled={jam === 1}
                                onClick={() => setJam(j => Math.max(1, j - 1))}
                            >
                                -
                            </Button>
                            <span className="text-lg">{jam}</span>
                            <Button 
                                size="sm" 
                                className="w-5 h-5 p-0" 
                                disabled={jam >= periodJamCounts[period - 1]}
                                onClick={() => setJam(j => j + 1)}
                            >
                                +
                            </Button>
                        </div>
                    </div>
                    <ScrollArea className="h-full max-h-[60vh]">
                        <Command value={penaltyCode}>
                            <CommandList className="h-full max-h-full">
                                { PENALTIES.map(p => (
                                    <CommandItem key={p.code} value={p.code} onSelect={handlePenaltySelected}>
                                        <span className="font-bold">{p.code}</span> - {p.nameKey}
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
                    { currentPenalty && (
                        <DialogClose asChild>
                            <Button onClick={handleAccept} variant="destructive">Delete</Button>
                        </DialogClose>
                    )}
                    <DialogClose asChild>
                        <Button onClick={handleAccept}>{currentPenalty ? "Update" : "Add"}</Button>
                    </DialogClose>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
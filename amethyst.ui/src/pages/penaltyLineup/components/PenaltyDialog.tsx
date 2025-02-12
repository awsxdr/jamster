import { Button, Command, CommandItem, CommandList, Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger, ScrollArea } from "@/components/ui";
import { Pointer } from "lucide-react";

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

type PenaltyDialogProps = {
    open: boolean;
    onOpenChanged?: (open: boolean) => void;
    onPenaltySelected?: (code: string) => void;
}

export const PenaltyDialog = ({ open, onOpenChanged, onPenaltySelected }: PenaltyDialogProps) => {

    return (
        <Dialog open={open} onOpenChange={onOpenChanged}>
            <DialogTrigger asChild>
                <Button className="grow" size="sm">
                    <Pointer />
                </Button>
            </DialogTrigger>
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
                    <ScrollArea className="h-full max-h-[60vh]">
                        <Command>
                            <CommandList className="h-full max-h-full">
                                { PENALTIES.map(p => (
                                    <CommandItem key={p.code} onSelect={() => onPenaltySelected?.(p.code)}>
                                        <span className="font-bold">{p.code}</span> - {p.name}
                                    </CommandItem>
                                ))}
                            </CommandList>
                        </Command>
                    </ScrollArea>
                </div>
            </DialogContent>
        </Dialog>
    );
}
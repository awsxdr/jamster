import { Button, Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, ScrollArea } from "@/components/ui";
import { useI18n } from "@/hooks";
import { Penalty } from "@/types";
import { useEffect, useState } from "react";

type ExpulsionDialogProps = {
    open: boolean;
    expulsionPenalty: Penalty | null;
    penalties: Penalty[];
    onOpenChanged?: (open: boolean) => void;
    onAccept?: (penalty: Penalty | null) => void;
}

export const ExpulsionDialog = ({ open, expulsionPenalty, penalties, onOpenChanged, onAccept }: ExpulsionDialogProps) => {

    const [expulsionPenaltyIndex, setExpulsionPenaltyIndex] = useState(-1);
    const { translate } = useI18n({ prefix: "PenaltyLineup.ExpulsionDialog." });

    useEffect(() => {
        if(!expulsionPenalty) {
            return;
        }

        setExpulsionPenaltyIndex(penalties.findIndex(p => p.period === expulsionPenalty.period && p.jam == expulsionPenalty.jam && p.code == expulsionPenalty.code));
    }, [expulsionPenalty]);

    const handlePenaltySelected = (index: number) => {
        setExpulsionPenaltyIndex(index);
        onAccept?.(index >= 0 ? penalties[index] : null)
        onOpenChanged?.(false);
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChanged}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        { translate("Title") }
                    </DialogTitle>
                    <DialogDescription>
                        { translate("Description") }
                    </DialogDescription>
                </DialogHeader>
                <div className="flex flex-col gap-2">
                    <ScrollArea className="h-full max-h-[60vh]">
                        <div className="flex flex-col">
                            <Button
                                variant={expulsionPenaltyIndex === -1 ? "secondary" : "ghost"}
                                className="justify-start flex flex-row text-base"
                                onClick={() => handlePenaltySelected(-1)}
                            >
                                <span className="italic font-normal">{translate("NoExpulsion")}</span>
                            </Button>
                            { penalties.map((p, i) => (
                                <Button 
                                    key={p.code} 
                                    variant={expulsionPenaltyIndex === i ? "secondary" : "ghost"} 
                                    className="justify-start flex flex-row text-base"
                                    onClick={() => handlePenaltySelected(i)}
                                >
                                    <div className="font-bold">{p.period}-{p.jam} ({p.code})</div>
                                </Button>
                            ))}
                        </div>
                    </ScrollArea>
                </div>
                <DialogFooter>
                    <DialogClose asChild>
                        <Button variant="secondary">{translate("Cancel")}</Button>
                    </DialogClose>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
import { Button, Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, Label, ScrollArea, Switch } from "@/components/ui";
import { useGameSummaryState, useI18n } from "@/hooks";
import { Penalty } from "@/types";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

type PenaltyDefinition = {
    code: string;
    nameKey: string;
}

const PENALTIES: PenaltyDefinition[] = [
    { code: "?", nameKey: "Unknown" },
    { code: "A", nameKey: "HighBlock" },
    { code: "B", nameKey: "BackBlock" },
    { code: "C", nameKey: "IllegalContact" },
    { code: "D", nameKey: "Direction" },
    { code: "E", nameKey: "LegBlock" },
    { code: "F", nameKey: "Forearm" },
    { code: "G", nameKey: "Misconduct" },
    { code: "H", nameKey: "HeadBlock" },
    { code: "I", nameKey: "IllegalProcedure" },
    { code: "L", nameKey: "LowBlock" },
    { code: "M", nameKey: "Multiplayer" },
    { code: "N", nameKey: "Interference" },
    { code: "P", nameKey: "IllegalPosition" },
    { code: "X", nameKey: "Cut" },
];

type PenaltyDialogProps = {
    open: boolean;
    currentPenalty?: Penalty;
    onOpenChanged?: (open: boolean) => void;
    onAccept?: (penalty: Penalty) => void;
    onDelete?: () => void;
}

export const PenaltyDialog = ({ open, currentPenalty, onOpenChanged, onAccept, onDelete }: PenaltyDialogProps) => {

    const [penaltyCode, setPenaltyCode] = useState("?");
    const [totalJam, setTotalJam] = useState(1);
    const [served, setServed] = useState(false);

    const { periodJamCounts } = useGameSummaryState() ?? { periodJamCounts: [] };
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyDialog." });

    useEffect(() => {
        if(!currentPenalty) {
            setPenaltyCode("?");
            return;
        }

        setPenaltyCode(currentPenalty.code);
        setTotalJam(periodJamCounts.slice(0, currentPenalty.period - 1).reduce((total, jamCount) => total + jamCount, currentPenalty.jam));
        setServed(currentPenalty.served);
    }, [currentPenalty]);

    const maxTotalJam = useMemo(() => periodJamCounts.reduce((total, count) => total + count, 0), [periodJamCounts]);

    const { period, jam } = useMemo(() => {
        const currentPeriod = periodJamCounts.reduce((periods, jamCount, index) => [
            ...periods, 
            {
                period: index + 1, 
                startJam: (periods[index - 1]?.endJam ?? 0) + 1,
                endJam: (periods[index - 1]?.endJam ?? 0) + jamCount
            }],
            [] as { period: number, startJam: number, endJam: number }[])
            .filter(p => p.startJam <= totalJam)
            .at(-1);
        
        return {
            period: currentPeriod?.period ?? 1,
            jam: totalJam - (currentPeriod?.startJam ?? 0) + 1
        };
    }, [totalJam]);

    const handlePenaltySelected = (value: string) => {
        setPenaltyCode(value);

        if(!currentPenalty) {
            onAccept?.({ code: value, period, jam, served: false })
            onOpenChanged?.(false);
        }
    }

    const handleAccept = () => {
        onAccept?.({ code: penaltyCode, period, jam, served });
        onOpenChanged?.(false);
    }

    const handleDelete = () => {
        onDelete?.();
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChanged}>
            <DialogContent className="max-h-[100vh] flex flex-col">
                <DialogHeader>
                    <DialogTitle>
                        { currentPenalty ? translate("EditTitle") : translate("AddTitle") }
                    </DialogTitle>
                    <DialogDescription>
                        { currentPenalty ? translate("EditDescription") : translate("AddDescription") }
                    </DialogDescription>
                </DialogHeader>
                <div className="flex justify-around">
                    { currentPenalty && (
                        <div className="flex justify-between w-full items-center">
                            <Button size="icon" disabled={totalJam <= 1} onClick={() => setTotalJam(j => j - 1)}><ChevronLeft /></Button>
                            { translate("PeriodJam").replace("{period}", period.toString()).replace("{jam}", jam.toString()) }
                            <Button size="icon" disabled={totalJam > maxTotalJam} onClick={() => setTotalJam(j => j + 1)}><ChevronRight /></Button>
                        </div>
                    )}
                </div>
                { currentPenalty && (
                    <div className="flex items-center justify-end gap-2">
                        <Label htmlFor="served">{translate("Served")}</Label>
                        <Switch id="served" checked={served} onCheckedChange={setServed} />
                    </div>
                )}
                <ScrollArea className="w-full max-h-full overflow-auto">
                    <div className=" grid grid-cols-[auto_auto_1fr] w-full">
                        { PENALTIES.map(p => (
                            <Button 
                                key={p.code} 
                                variant={penaltyCode === p.code ? "secondary" : "ghost"} 
                                className="justify-start items-center grid col-span-3 col-start-1 grid-cols-subgrid text-base w-full h-auto"
                                onClick={() => handlePenaltySelected(p.code)}
                            >
                                <div className="font-bold col-start-1 text-left">{p.code}</div>
                                <div className="font-normal col-start-2 text-left">{translate(`Penalty.${p.nameKey}.Name`)}</div>
                                <div className="font-light italic text-wrap text-left text-sm col-start-3">{translate(`Penalty.${p.nameKey}.Alternatives`)}</div>
                            </Button>
                        ))}
                    </div>
                </ScrollArea>
                <DialogFooter className="gap-1">
                    <DialogClose asChild>
                        <Button variant="secondary">{translate("Cancel")}</Button>
                    </DialogClose>
                    { currentPenalty && (
                        <>
                            <DialogClose asChild>
                                <Button onClick={handleDelete} variant="destructive">{translate("Delete")}</Button>
                            </DialogClose>
                            <Button onClick={handleAccept}>{translate("Update")}</Button>
                        </>
                    )}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
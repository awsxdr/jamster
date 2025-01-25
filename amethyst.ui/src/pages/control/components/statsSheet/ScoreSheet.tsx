import { useScoreSheetState, useTeamDetailsState, useTimeoutListState } from "@/hooks";
import { ScoreSheetJam, TeamSide, TimeoutListItem } from "@/types";
import { ScoreSheetJamRow } from "./ScoreSheetJamRow";
import { cn } from "@/lib/utils";
import { useMemo } from "react";
import { ScoreSheetTimeoutRow } from "./ScoreSheetTimeoutRow";

type ScoreSheetProps = {
    teamSide: TeamSide;
    descending?: boolean;
    className?: string;
}

type ScoreSheetItem = Extract<ScoreSheetJam, TimeoutListItem> | {
    type: ("jam" | "timeout") & string;
    period: number;
    jam: number;
    jamItem?: ScoreSheetJam & { even: boolean };
    timeoutItem?: TimeoutListItem;
}

export const ScoreSheet = ({ teamSide, descending, className }: ScoreSheetProps) => {

    const scoreSheet = useScoreSheetState(teamSide);

    const team = useTeamDetailsState(teamSide);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['color'] ||  team.team.names['team'] || team.team.names['league'] || '';
    }, [team]);

    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const timeoutItems: ScoreSheetItem[] = timeouts.map(t => ({ ...t, type: "timeout", timeoutItem: t }));
    const jamItems: ScoreSheetItem[] = [...(scoreSheet?.jams ?? [])].map((j, i) => ({ ...j, type: "jam", jamItem: { ...j, even: i % 2 === 0 }}));

    const items = [...timeoutItems, ...jamItems]
        .sort((a, b) => a.period < b.period || a.period === b.period && a.jam < b.jam || a.period === b.period && a.jam === b.jam && a.type === "jam" ? -1 : 1);
    
    const periodLines = useMemo(() => {
        return (
            descending ? [...items].reverse() : items
        ).reduce((result, item) => ({ ...result, [item.period]: [...(result[item.period] ?? []), item]}), {} as {[key: number]: ScoreSheetItem[]});
    }, [items, descending]);

    if(!scoreSheet) {
        return (
            <></>
        );
    }

    const periods = descending ? Object.values(periodLines).reverse() : Object.values(periodLines);

    return (
        <div className="flex flex-col gap-2">
            <div className="text-lg text-center block xl:hidden">{teamName}</div>
            { periods.map(lines => (
                <>
                    <div>Period {lines[0]?.period}</div>
                    <div 
                        className={cn(
                            "border border-t-2 border-r-2 border-black dark:border-gray-400",
                            "grid grid-flow-row grid-cols-[2fr_4fr_1fr_1fr_1fr_1fr_1fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr]",
                            className,
                        )}
                    >
                        { lines.map((line, i) => 
                            line.type === "jam"
                            ? <ScoreSheetJamRow key={i} line={line.jamItem!} even={line.jamItem!.even} />
                            : <ScoreSheetTimeoutRow key={i} timeout={line.timeoutItem!} sheetSide={teamSide} />
                        )}
                    </div>
                </>
            ))}
        </div>
    );
}
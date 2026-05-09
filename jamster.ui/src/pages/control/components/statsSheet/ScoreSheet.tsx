import { useI18n, useScoreSheetState, useTeamDetailsState, useTimeoutListState } from "@/hooks";
import { ScoreSheetJam, TeamSide, TimeoutListItem } from "@/types";
import { ScoreSheetJamRow } from "./ScoreSheetJamRow";
import { cn } from "@/lib/utils";
import { Fragment, useMemo } from "react";
import { ScoreSheetTimeoutRow } from "./ScoreSheetTimeoutRow";
import { JamEditMenu } from "./JamEditMenu";
import { ternary } from "@/utilities/switchex";

type ScoreSheetProps = {
    gameId: string;
    teamSide: TeamSide;
    descending?: boolean;
    showTimeouts?: boolean;
    className?: string;
}

type ScoreSheetItem = {
    type: string;
    period: number;
    jam: number;
    jamItem?: ScoreSheetJam & { even: boolean, totalJamNumber: number, };
    timeoutItem?: TimeoutListItem;
}

const typeOrder = ["jam", "preStarPass", "postStarPass", "timeout"];

export const ScoreSheet = ({ gameId, teamSide, descending, showTimeouts, className }: ScoreSheetProps) => {
    const scoreSheet = useScoreSheetState(teamSide);
    const opponentScoreSheet = useScoreSheetState(teamSide === TeamSide.Home ? TeamSide.Away : TeamSide.Home);

    const { translate } = useI18n({ prefix: "ScoreboardControl.StatsSheet.ScoreSheet." })

    const team = useTeamDetailsState(teamSide);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['color'] ||  team.team.names['team'] || team.team.names['league'] || '';
    }, [team]);

    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const timeoutItems: ScoreSheetItem[] = useMemo(() =>
        showTimeouts
            ? timeouts.map((t, i) => ({ ...t, lineNumber: i, type: "timeout", timeoutItem: t }))
            : [],
    [showTimeouts, timeouts]);

    const jamItems: ScoreSheetItem[] = useMemo(() =>
        [...(scoreSheet?.jams ?? [])]
            .map((j, i) => ({ 
                ...j,
                type: "jam",
                jamItem: { ...j, totalJamNumber: i },
                starPassInJam: j.starPassTrip !== null || opponentScoreSheet?.jams[i]?.starPassTrip !== null,
            }))
            .filter(l => !l.deleted)
            .flatMap(j => 
                j.starPassInJam
                    ? [{...j, type: "preStarPass"}, {...j, type: "postStarPass"}]
                    : [j]
            )
            .reduce((aggregate, item) => ({
                period: item.period,
                index: aggregate.period === item.period ? aggregate.index + 1 : 0,
                items: [...aggregate.items, {
                    ...item,
                    jamItem: {
                        ...item.jamItem,
                        even: aggregate.period === item.period ? (aggregate.index % 2 === 1) : true,
                    }
                }]
            }), { period: 0, index: 0, items: [] as ScoreSheetItem[] })
            .items,
    [scoreSheet?.jams, opponentScoreSheet?.jams]);

    const items = useMemo(() =>
        [...timeoutItems, ...jamItems]
            .sort((a, b) => 
                ternary()
                    .predicate(() => a.period < b.period).then(-1)
                    .predicate(() => a.period === b.period && a.jam < b.jam).then(-1)
                    .predicate(() => a.period === b.period && a.jam === b.jam).then(typeOrder.indexOf(a.type) - typeOrder.indexOf(b.type))
                    .default(1)),
    [timeoutItems, jamItems]);

    const periodLines = useMemo(() => {
        return (
            descending ? [...items].reverse() : items
        ).reduce((result, item) => ({ ...result, [item.period]: [...(result[item.period] ?? []), item]}), {} as Record<number, ScoreSheetItem[]>);
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
            { periods.map((lines, lineNumber) => (
                <Fragment key={`line-${lineNumber}`}>
                    <div key={`period-header-${lineNumber}`}>{ translate("Period") } {lines[0]?.period}</div>
                    <div 
                        key={`period-${lineNumber}`}
                        className={cn(
                            "border-r-2 border-black dark:border-gray-400",
                            "grid grid-flow-row grid-cols-[1fr_2fr_4fr_1fr_1fr_1fr_1fr_1fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr_3fr]",
                            className,
                        )}
                    >
                        { lines.map((line, i) => 
                            line.type === "jam" && line.jamItem ? (
                                <Fragment key={`jam-${i}`}>
                                    <JamEditMenu 
                                        key={`jamEdit-${lineNumber}-column-${i}`} 
                                        gameId={gameId}
                                        teamSide={teamSide}
                                        lineNumber={line.jamItem.totalJamNumber}
                                        starPassTrip={null} 
                                    />
                                    <ScoreSheetJamRow 
                                        key={`jamLine-${lineNumber}-column-${i}`} 
                                        gameId={gameId}
                                        teamSide={teamSide}
                                        lineNumber={line.jamItem.totalJamNumber}
                                        line={line.jamItem} 
                                        even={line.jamItem.even}
                                        className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                    />
                                </Fragment>
                            ) : line.type === "timeout" && line.timeoutItem ? (
                                <ScoreSheetTimeoutRow 
                                    key={`jam-${i}`}
                                    timeout={line.timeoutItem} 
                                    sheetSide={teamSide} 
                                    className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                />
                            ) : line.type === "preStarPass" && line.jamItem ? (
                                <ScoreSheetJamRow 
                                    preStarPass
                                    key={`jam-${i}`}
                                    gameId={gameId}
                                    teamSide={teamSide}
                                    lineNumber={line.jamItem.totalJamNumber}
                                    line={line.jamItem} 
                                    even={line.jamItem.even}
                                    className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                />
                            ) : line.type === "postStarPass" && line.jamItem ? (
                                <Fragment key={`jam-${i}`}>
                                    <JamEditMenu 
                                        key={`jamEdit-${lineNumber}-column-${i}`} 
                                        gameId={gameId}
                                        teamSide={teamSide}
                                        lineNumber={line.jamItem.totalJamNumber}
                                        starPassTrip={line.jamItem.starPassTrip} 
                                        className="row-span-2 self-stretch" 
                                    />
                                    <ScoreSheetJamRow 
                                        postStarPass
                                        gameId={gameId}
                                        teamSide={teamSide}
                                        lineNumber={line.jamItem.totalJamNumber}
                                        key={`jamLine-${lineNumber}-column-${i}`} 
                                        line={line.jamItem} 
                                        even={line.jamItem.even}
                                        className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                    />
                                </Fragment>
                            ) : (<></>)
                        )}
                    </div>
                </Fragment>
            ))}
        </div>
    );
}
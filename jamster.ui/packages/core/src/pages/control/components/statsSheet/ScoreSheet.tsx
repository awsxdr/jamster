import { useEvents, useI18n, useScoreSheetState, useTeamDetailsState, useTimeoutListState } from "@/hooks";
import { ScoreSheetJam, TeamSide, TimeoutListItem } from "@/types";
import { ScoreSheetJamRow } from "./ScoreSheetJamRow";
import { cn } from "@/lib/utils";
import { Fragment, useMemo } from "react";
import { ScoreSheetTimeoutRow } from "./ScoreSheetTimeoutRow";
import { ScoreSheetCalledSet, ScoreSheetInjurySet, ScoreSheetJammerNumberSet, ScoreSheetLeadSet, ScoreSheetLineDeleted, ScoreSheetLostSet, ScoreSheetPivotNumberSet, ScoreSheetStarPassTripSet, ScoreSheetTripScoreSet } from "@/types/events";
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

export const ScoreSheet = ({ gameId, teamSide, descending, showTimeouts, className }: ScoreSheetProps) => {
    const scoreSheet = useScoreSheetState(teamSide);
    const opponentScoreSheet = useScoreSheetState(teamSide === TeamSide.Home ? TeamSide.Away : TeamSide.Home);

    const { translate } = useI18n({ prefix: "ScoreboardControl.StatsSheet.ScoreSheet." })

    const team = useTeamDetailsState(teamSide);
    const { sendEvent } = useEvents();

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['color'] ||  team.team.names['team'] || team.team.names['league'] || '';
    }, [team]);

    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const timeoutItems: ScoreSheetItem[] = showTimeouts
        ? timeouts.map((t, i) => ({ ...t, lineNumber: i, type: "timeout", timeoutItem: t }))
        : [];

    const jamItems: ScoreSheetItem[] = 
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
            .items;

    const typeOrder = ["jam", "preStarPass", "postStarPass", "timeout"];

    const items = [...timeoutItems, ...jamItems]
        .sort((a, b) => 
            ternary()
                .predicate(() => a.period < b.period).then(-1)
                .predicate(() => a.period === b.period && a.jam < b.jam).then(-1)
                .predicate(() => a.period === b.period && a.jam === b.jam).then(typeOrder.indexOf(a.type) - typeOrder.indexOf(b.type))
                .default(1));

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

    const handleJammerNumberSet = (lineNumber: number | undefined, value: string) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetJammerNumberSet(teamSide, lineNumber, value));
    }

    const handlePivotNumberSet = (lineNumber: number | undefined, value: string) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetPivotNumberSet(teamSide, lineNumber, value));
    }

    const handleTripScoreSet = (jamNumber: number | undefined, tripNumber: number, value: number | null) => {
        if(jamNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetTripScoreSet(teamSide, jamNumber, tripNumber, value));
    }

    const handleLostSet = (lineNumber: number | undefined, value: boolean) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetLostSet(teamSide, lineNumber, value));
    }

    const handleLeadSet = (lineNumber: number | undefined, value: boolean) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetLeadSet(teamSide, lineNumber, value));
    }

    const handleCalledSet = (lineNumber: number | undefined, value: boolean) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetCalledSet(teamSide, lineNumber, value));
    }

    const handleInjurySet = (lineNumber: number | undefined, value: boolean) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetInjurySet(lineNumber, value));
    }

    const handleJamDeleted = (lineNumber?: number) => {
        if(lineNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetLineDeleted(lineNumber));
    }

    const handleStarPassTripChanged = (jamNumber: number | undefined, starPassTrip: number | null) => {
        if(jamNumber === undefined) {
            return;
        }

        sendEvent(gameId, new ScoreSheetStarPassTripSet(teamSide, jamNumber, starPassTrip));
    }

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
                                        starPassTrip={null} 
                                        onJamDeleted={() => handleJamDeleted(line.jamItem?.totalJamNumber)}
                                        onStarPassTripChanged={passTrip => handleStarPassTripChanged(line.jamItem?.totalJamNumber, passTrip)} 
                                    />
                                    <ScoreSheetJamRow 
                                        key={`jamLine-${lineNumber}-column-${i}`} 
                                        line={line.jamItem} 
                                        even={line.jamItem.even}
                                        className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                        onJammerNumberSet={v => handleJammerNumberSet(line.jamItem?.totalJamNumber, v)}
                                        onPivotNumberSet={v => handlePivotNumberSet(line.jamItem?.totalJamNumber, v)}
                                        onTripScoreSet={(t, v) => handleTripScoreSet(line.jamItem?.totalJamNumber, t, v)}
                                        onLostSet={v => handleLostSet(line.jamItem?.totalJamNumber, v)}
                                        onLeadSet={v => handleLeadSet(line.jamItem?.totalJamNumber, v)}
                                        onCalledSet={v => handleCalledSet(line.jamItem?.totalJamNumber, v)}
                                        onInjurySet={v => handleInjurySet(line.jamItem?.totalJamNumber, v)}
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
                                    line={line.jamItem} 
                                    even={line.jamItem.even}
                                    className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                    onJammerNumberSet={v => handleJammerNumberSet(line.jamItem?.totalJamNumber, v)}
                                    onPivotNumberSet={v => handlePivotNumberSet(line.jamItem?.totalJamNumber, v)}
                                    onTripScoreSet={(t, v) => handleTripScoreSet(line.jamItem?.totalJamNumber, t, v)}
                                    onLostSet={v => handleLostSet(line.jamItem?.totalJamNumber, v)}
                                    onLeadSet={v => handleLeadSet(line.jamItem?.totalJamNumber, v)}
                                    onCalledSet={v => handleCalledSet(line.jamItem?.totalJamNumber, v)}
                                    onInjurySet={v => handleInjurySet(line.jamItem?.totalJamNumber, v)}
                                />
                            ) : line.type === "postStarPass" && line.jamItem ? (
                                <Fragment key={`jam-${i}`}>
                                    <JamEditMenu 
                                        key={`jamEdit-${lineNumber}-column-${i}`} 
                                        starPassTrip={line.jamItem.starPassTrip} 
                                        className="row-span-2 self-stretch" 
                                        onJamDeleted={() => handleJamDeleted(line.jamItem?.totalJamNumber)}
                                        onStarPassTripChanged={passTrip => handleStarPassTripChanged(line.jamItem?.totalJamNumber, passTrip)}
                                    />
                                    <ScoreSheetJamRow 
                                        postStarPass
                                        key={`jamLine-${lineNumber}-column-${i}`} 
                                        line={line.jamItem} 
                                        even={line.jamItem.even}
                                        className={cn(i === 0 && "border-t-2", i === lines.length - 1 && "border-b-2")}
                                        onJammerNumberSet={v => handleJammerNumberSet(line.jamItem?.totalJamNumber, v)}
                                        onPivotNumberSet={v => handlePivotNumberSet(line.jamItem?.totalJamNumber, v)}
                                        onTripScoreSet={(t, v) => handleTripScoreSet(line.jamItem?.totalJamNumber, t, v)}
                                        onLostSet={v => handleLostSet(line.jamItem?.totalJamNumber, v)}
                                        onLeadSet={v => handleLeadSet(line.jamItem?.totalJamNumber, v)}
                                        onCalledSet={v => handleCalledSet(line.jamItem?.totalJamNumber, v)}
                                        onInjurySet={v => handleInjurySet(line.jamItem?.totalJamNumber, v)}
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
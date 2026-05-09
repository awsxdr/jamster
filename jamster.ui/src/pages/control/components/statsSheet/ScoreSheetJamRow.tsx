import { cn } from "@/lib/utils";
import { JamLineTrip, ScoreSheetJam, TeamSide } from "@/types"
import { EditableCell } from "./EditableCell";
import { CheckCell } from "./CheckCell";
import { ternary } from "@/utilities/switchex";
import { useEvents } from "@/hooks";
import { ScoreSheetCalledSet, ScoreSheetInjurySet, ScoreSheetJammerNumberSet, ScoreSheetLeadSet, ScoreSheetLostSet, ScoreSheetPivotNumberSet, ScoreSheetTripScoreSet } from "@/types/events";
import React from "react";

type RowData = Omit<ScoreSheetJam, "period" | "deleted">;

type ScoreSheetJamRowProps = {
    gameId: string;
    teamSide: TeamSide;
    lineNumber: number;
    line: RowData;
    even: boolean;
    preStarPass?: boolean;
    postStarPass?: boolean;
    className?: string;
}

const tripsEqual = (prev: JamLineTrip, next: JamLineTrip) =>
    prev.score === next.score;

const linesEqual = (prev: RowData, next: RowData) =>
    prev.called === next.called
    && prev.gameTotal === next.gameTotal
    && prev.injury === next.injury
    && prev.jam === next.jam
    && prev.jamTotal === next.jamTotal
    && prev.jammerNumber === next.jammerNumber
    && prev.lead === next.lead
    && prev.lost === next.lost
    && prev.noInitial === next.noInitial
    && prev.pivotNumber === next.pivotNumber
    && prev.starPassTrip === next.starPassTrip
    && prev.trips.length === next.trips.length
    && prev.trips.every((t, i) => tripsEqual(t, next.trips[i]));

export const ScoreSheetJamRow = React.memo(function ScoreSheetJamRow({ gameId, teamSide, lineNumber, line, even, preStarPass, postStarPass, className }: ScoreSheetJamRowProps) {

    const { sendEvent } = useEvents();

    const rowColorClass = even ? "bg-white dark:bg-gray-900" : "bg-green-100 dark:bg-green-900";
    const rowEmphasisColorClass = even ? "bg-green-100 dark:bg-green-900" : "bg-green-200 dark:bg-green-700";
    const rowFixedEmphasisColorClass = "bg-green-200 dark:bg-green-700";
    const rowDisabledColorClass = "";//even ? "bg-gray-100" : "bg-[#ddeedd]";

    const numberCellClass = "text-center border-l border-b border-black dark:border-gray-400 w-full";
    const checkCellClass = "text-center border-l border-b border-black dark:border-gray-400";

    const lineLabel =
        postStarPass 
            ? (line.starPassTrip == null ? "" : line.pivotNumber)
            : line.jammerNumber;

    const isPreThisTeamStarPass = !!preStarPass && line.starPassTrip !== null;
    const isPostThisTeamStarPass = !!postStarPass && line.starPassTrip !== null;
    const isPreOtherTeamStarPass = !!preStarPass && !isPreThisTeamStarPass;
    const isPostOtherTeamStarPass = !!postStarPass && !isPostThisTeamStarPass;
    const isStarPass = preStarPass || postStarPass;

    const lineLost = !postStarPass && line.lost;
    const lineLead = !postStarPass && line.lead;
    const lineCalled = !postStarPass && line.called;
    const lineInjury = line.injury && (isPostThisTeamStarPass || isPreOtherTeamStarPass || !isStarPass);
    const lineNoInitial = 
        isPreThisTeamStarPass && line.starPassTrip === 0 
        || isPreOtherTeamStarPass && line.noInitial
        || isPostThisTeamStarPass && line.noInitial
        || !isStarPass && line.noInitial;
    const starPassSwitchTrip = Math.max(0, (line.starPassTrip ?? 0) - 1);

    const lineTotal = 
        ternary()
            .if(isPreThisTeamStarPass).then(line.trips.slice(0, starPassSwitchTrip).map(t => t.score ?? 0).reduce((t, i) => t + i, 0))
            .if(isPostThisTeamStarPass).then(line.trips.slice(starPassSwitchTrip).map(t => t.score ?? 0).reduce((t, i) => t + i, 0))
            .if(isPostOtherTeamStarPass).then(0)
            .if(line.starPassTrip !== null).then(0)
            .default(line.jamTotal);

    const lineGameTotal =
        isPreThisTeamStarPass 
            ? line.gameTotal - line.jamTotal + lineTotal
            : line.gameTotal;

    const nonNullTripCount = line.trips.filter(t => t.score !== null).length;

    const handleJammerNumberSet = (value: string) => {
        if (postStarPass) {
            sendEvent(gameId, new ScoreSheetPivotNumberSet(teamSide, lineNumber, value));
        } else {
            sendEvent(gameId, new ScoreSheetJammerNumberSet(teamSide, lineNumber, value));
        }
    }

    const handleLostSet = (value: boolean) => {
        if(!postStarPass) {
            sendEvent(gameId, new ScoreSheetLostSet(teamSide, lineNumber, value));
        }
    }

    const handleLeadSet = (value: boolean) => {
        if (!postStarPass) {
            sendEvent(gameId, new ScoreSheetLeadSet(teamSide, lineNumber, value));
        }
    }

    const handleCalledSet = (value: boolean) => {
        if(!postStarPass) {
            sendEvent(gameId, new ScoreSheetCalledSet(teamSide, lineNumber, value));
        }
    }

    const handleInjurySet = (value: boolean) => {
        sendEvent(gameId, new ScoreSheetInjurySet(lineNumber, value));
    }

    const handleTripScoreChanged = (trip: number, value: string) => {
        const parsedValue = parseInt(value);
        const newValue =
            ternary()
                .predicate(() => value.trim() === "").then<number | null>(null)
                .predicate(() => Number.isNaN(parsedValue)).then(0)
                .default(parsedValue);
        
        sendEvent(gameId, new ScoreSheetTripScoreSet(teamSide, lineNumber, trip, newValue));
    }

    return (
        <>
            <div className={cn("col-start-2 relative", numberCellClass, rowEmphasisColorClass, "border-l-2", className)}>
                {postStarPass ? (line.starPassTrip !== null ? "SP" : "SP*") : line.jam}
            </div>
            <EditableCell 
                value={lineLabel} 
                disabled={isPostOtherTeamStarPass}
                onValueChanged={handleJammerNumberSet} 
                className={cn("col-start-3", numberCellClass, rowColorClass, className)} 
            />
            <CheckCell checked={lineLost} disabled={postStarPass} onCheckedChanged={handleLostSet} className={cn("col-start-4", checkCellClass, "border-l-2", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineLead} disabled={postStarPass} onCheckedChanged={handleLeadSet} className={cn("col-start-5", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineCalled} disabled={postStarPass} onCheckedChanged={handleCalledSet} className={cn("col-start-6", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineInjury} disabled={isPreThisTeamStarPass || isPostOtherTeamStarPass} onCheckedChanged={handleInjurySet} className={cn("col-start-7", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineNoInitial} className={cn("col-start-8", checkCellClass, "border-black", "border-r border-black", rowEmphasisColorClass, className)} />
            { isPreThisTeamStarPass && (
                <>
                    {Array.from(new Array(starPassSwitchTrip)).map((_, i) => (
                        <EditableCell 
                            value={line.trips[i]?.score?.toString() ?? ""}
                            key={`trip-${i+2}`} 
                            className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, className)}
                            onValueChanged={v => handleTripScoreChanged(i, v)}
                        />
                    ))}
                    {Array.from(new Array(9 - starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9 + starPassSwitchTrip}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
                    ))}
                </>
            )}
            { isPostThisTeamStarPass && (
                <>
                    {Array.from(new Array(starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
                    ))}
                    {Array.from(new Array(9 - starPassSwitchTrip)).map((_, i) => (
                        <EditableCell 
                            value={line.trips[i + starPassSwitchTrip]?.score?.toString() ?? ""}
                            disabled={i + starPassSwitchTrip > nonNullTripCount}
                            key={`trip-${i+2}`} 
                            className={cn(`col-start-${i + 9 + starPassSwitchTrip}`, numberCellClass, rowColorClass, className)}
                            onValueChanged={v => handleTripScoreChanged(i + starPassSwitchTrip, v)}
                        />
                    ))}
                </>
            )}
            { isPostOtherTeamStarPass && Array.from(new Array(9)).map((_, i) => (
                <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
            ))}
            { (!isStarPass || isPreOtherTeamStarPass) && Array.from(new Array(9)).map((_, i) => (
                <EditableCell 
                    value={line.trips[i]?.score?.toString() ?? ""}
                    disabled={i > nonNullTripCount}
                    key={`trip-${i+2}`} 
                    className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, className)}
                    onValueChanged={v => handleTripScoreChanged(i, v)}
                />
            ))}
            <div className={cn("col-start-18", numberCellClass, "border-l-2", rowColorClass, className)}>{lineTotal}</div>
            <div className={cn("col-start-19", numberCellClass, "border-l-2", rowFixedEmphasisColorClass, className)}>{lineGameTotal}</div>
        </>
    )
},
(prev, next) =>
    prev.lineNumber === next.lineNumber
    && prev.even === next.even
    && prev.gameId === next.gameId
    && prev.teamSide === next.teamSide
    && prev.preStarPass === next.preStarPass
    && prev.postStarPass === next.postStarPass
    && prev.className === next.className
    && linesEqual(prev.line, next.line)
);
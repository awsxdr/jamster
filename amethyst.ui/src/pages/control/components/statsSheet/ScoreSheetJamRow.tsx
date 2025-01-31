import { cn } from "@/lib/utils";
import { ScoreSheetJam } from "@/types"
import { EditableCell } from "./EditableCell";
import { CheckCell } from "./CheckCell";

type ScoreSheetJamRowProps = {
    line: ScoreSheetJam;
    even: boolean;
    preStarPass?: boolean;
    postStarPass?: boolean;
    className?: string;
    onJammerNumberSet?: (jammerNumber: string) => void;
    onPivotNumberSet?: (pivotNumber: string) => void;
    onLostSet?: (value: boolean) => void;
    onLeadSet?: (value: boolean) => void;
    onCalledSet?: (value: boolean) => void;
    onInjurySet?: (value: boolean) => void;
}

export const ScoreSheetJamRow = ({ line, even, preStarPass, postStarPass, className, onJammerNumberSet, onPivotNumberSet, onLostSet, onLeadSet, onCalledSet, onInjurySet }: ScoreSheetJamRowProps) => {

    const rowColorClass = even ? "bg-white dark:bg-gray-900" : "bg-green-100 dark:bg-green-900";
    const rowEmphasisColorClass = even ? "bg-green-100 dark:bg-green-900" : "bg-green-200 dark:bg-green-700";
    const rowFixedEmphasisColorClass = "bg-green-200 dark:bg-green-700";
    const rowDisabledColorClass = "";//even ? "bg-gray-100" : "bg-[#ddeedd]";

    const numberCellClass = "text-center border-l border-b border-black dark:border-gray-400 w-full";
    const checkCellClass = "text-center border-l border-b border-black dark:border-gray-400";

    const lineLabel =
        postStarPass ? (line.starPassTrip == null ? "" : line.pivotNumber)
        : line.jammerNumber;

    const isPreThisTeamStarPass = preStarPass && line.starPassTrip !== null;
    const isPostThisTeamStarPass = postStarPass && line.starPassTrip !== null;
    const isPreOtherTeamStarPass = preStarPass && !isPreThisTeamStarPass;
    const isPostOtherTeamStarPass = postStarPass && !isPostThisTeamStarPass;
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
        isPreThisTeamStarPass ? line.trips.slice(0, starPassSwitchTrip).map(t => t.score ?? 0).reduce((t, i) => t + i, 0)
        : isPostThisTeamStarPass ? line.trips.slice(starPassSwitchTrip).map(t => t.score ?? 0).reduce((t, i) => t + i, 0)
        : isPostOtherTeamStarPass ? 0
        : line.starPassTrip !== null ? 0
        : line.jamTotal;

    const lineGameTotal =
        isPreThisTeamStarPass ? line.gameTotal - line.jamTotal + lineTotal
        : line.gameTotal;

    const handleLostSet = (value: boolean) => {
        if(!postStarPass) {
            onLostSet?.(value);
        }
    }

    const handleLeadSet = (value: boolean) => {
        if (!postStarPass) {
            onLeadSet?.(value);
        }
    }

    const handleCalledSet = (value: boolean) => {
        if(!postStarPass) {
            onCalledSet?.(value);
        }
    }

    const handleInjurySet = (value: boolean) => {
        onInjurySet?.(value);
    }

    return (
        <>
            <div className={cn("col-start-2 relative", numberCellClass, rowEmphasisColorClass, "border-l-2", className)}>
                {postStarPass ? (line.starPassTrip !== null ? "SP" : "SP*") : line.jam}
            </div>
            <EditableCell 
                value={lineLabel} 
                disabled={isPostOtherTeamStarPass}
                onValueChanged={postStarPass ? onPivotNumberSet : onJammerNumberSet} 
                className={cn("col-start-3", numberCellClass, rowColorClass, className)} 
            />
            <CheckCell checked={lineLost} disabled={postStarPass} onCheckedChanged={handleLostSet} className={cn("col-start-4", checkCellClass, "border-l-2", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineLead} disabled={postStarPass} onCheckedChanged={handleLeadSet} className={cn("col-start-5", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineCalled} disabled={postStarPass} onCheckedChanged={handleCalledSet} className={cn("col-start-6", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineInjury} disabled={isPreThisTeamStarPass || isPostOtherTeamStarPass} onCheckedChanged={handleInjurySet} className={cn("col-start-7", checkCellClass, "border-black", rowEmphasisColorClass, className)} />
            <CheckCell checked={lineNoInitial} className={cn("col-start-8", checkCellClass, "border-black", "border-r border-black", rowEmphasisColorClass, className)} />
            { line.starPassTrip != null && preStarPass && (
                <>
                    {Array.from(new Array(starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, className)}>{line.trips[i]?.score}</div>
                    ))}
                    {Array.from(new Array(9 - starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9 + starPassSwitchTrip}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
                    ))}
                </>
            )}
            { line.starPassTrip !== null && postStarPass && (
                <>
                    {Array.from(new Array(starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
                    ))}
                    {Array.from(new Array(9 - starPassSwitchTrip)).map((_, i) => (
                        <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9 + starPassSwitchTrip}`, numberCellClass, rowColorClass, className)}>{line.trips[i + starPassSwitchTrip]?.score}</div>
                    ))}
                </>
            )}
            { line.starPassTrip === null && postStarPass && Array.from(new Array(9)).map((_, i) => (
                <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, rowDisabledColorClass, className)}></div>
            ))}
            { line.starPassTrip === null && preStarPass && Array.from(new Array(9)).map((_, i) => (
                <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, className)}>{line.trips[i]?.score}</div>
            ))}
            {
                !preStarPass && !postStarPass && Array.from(new Array(9)).map((_, i) => (
                    <div key={`trip-${i+2}`} className={cn(`col-start-${i + 9}`, numberCellClass, rowColorClass, className)}>{line.trips[i]?.score}</div>
            ))}
            <div className={cn("col-start-18", numberCellClass, "border-l-2", rowColorClass, className)}>{lineTotal}</div>
            <div className={cn("col-start-19", numberCellClass, "border-l-2", rowFixedEmphasisColorClass, className)}>{lineGameTotal}</div>
        </>
    )
}
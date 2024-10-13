import { useGameState } from "@/hooks";
import { ScaledText } from "./ScaledText";
import { TeamSide } from "./TeamScore";
import { useMemo } from "react";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

type TeamDetailsState = {
    team: Team,
};

type Team = {
    id: string,
    names: StringMap<string>,
    colors: StringMap<DisplayColor>,
    roster: Skater[],
};

type DisplayColor = {
    foreground: string,
    background: string,
};

type Skater = {
    number: string,
    name: string,
    pronouns: string,
    role: SkaterRole,
};

enum SkaterRole
{
    Skater,
    Captain,
    NotSkating,
    BenchStaff,
}

type StringMap<TValue> = {
    [key: string]: TValue | undefined;
};

export const TeamName = ({ side, textClassName }: TeamScoreProps) => {

    const gameState = useGameState();
    const team = gameState.useStateWatch<TeamDetailsState>(`TeamDetailsState_${TeamSide[side]}`);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['scoreboard'] || team.team.names['default'] || '';
    }, [team]);

    return (
        <>
            <ScaledText text={teamName || ''} className={textClassName} />
        </>
    );
}
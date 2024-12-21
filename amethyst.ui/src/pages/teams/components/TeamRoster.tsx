import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { RosterInput } from "./RosterInput";
import { RosterTable } from "./RosterTable";
import { useI18n, useTeamApi } from "@/hooks";
import { SkaterRole, Team } from "@/types";
import { useMemo } from "react";

type TeamRosterProps = {
    team: Team;
    className?: string;
}

export const TeamRoster = ({ team, className }: TeamRosterProps) => {

    const { translate } = useI18n();

    const { setRoster } = useTeamApi();

    const skaterNumbers = useMemo(() => team.roster.map(s => s.number), [team]);

    const handleSkaterAdded = (number: string, name: string) => {
        setRoster(team.id, [
            ...team.roster,
            { number, name, pronouns: '', role: SkaterRole.Skater },
        ])
    }

    return (
        <Card className={className}>
            <CardHeader>
                <CardTitle>{ translate("TeamRoster.Title") }</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-4">
                <RosterInput existingNumbers={skaterNumbers} onSkaterAdded={handleSkaterAdded} />
                <RosterTable team={team} />
            </CardContent>
        </Card>
    )
}
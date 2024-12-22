import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { RosterInput } from "./RosterInput";
import { RosterTable } from "./RosterTable";
import { useI18n, useTeamApi } from "@/hooks";
import { Skater, Team } from "@/types";
import { useMemo } from "react";

type TeamRosterProps = {
    team: Team;
    className?: string;
}

export const TeamRoster = ({ team, className }: TeamRosterProps) => {

    const { translate } = useI18n();

    const { setRoster } = useTeamApi();

    const skaterNumbers = useMemo(() => team.roster.map(s => s.number), [team]);

    const handleSkatersAdded = (skaters: Skater[]) => {
        setRoster(team.id, [
            ...team.roster,
            ...skaters,
        ])
    }

    return (
        <Card className={className}>
            <CardHeader>
                <CardTitle>{ translate("TeamRoster.Title") }</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-4">
                <RosterInput existingNumbers={skaterNumbers} onSkatersAdded={handleSkatersAdded} />
                <RosterTable team={team} />
            </CardContent>
        </Card>
    )
}
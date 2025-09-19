import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { useI18n, useTeamApi } from "@/hooks";
import { ColorInput } from "./ColorInput";
import { ColorsTable } from "./ColorsTable";
import { Team, TeamColor } from "@/types";

type TeamColorsProps = {
    team: Team;
    className?: string;
}

export const TeamColors = ({ team, className }: TeamColorsProps) => {

    const { translate } = useI18n();

    const { setTeam } = useTeamApi();

    const handleColorAdded = (name: string, color: TeamColor) => {
        setTeam(team.id, { ...team, colors: { ...team.colors, [name]: color }});
    }

    return (
        <Card className={className}>
            <CardHeader>
                <CardTitle>{ translate("TeamColors.Title") }</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2">
                <ColorInput existingColors={Object.keys(team.colors)} onColorAdded={handleColorAdded} />
                <ColorsTable team={team} />
            </CardContent>
        </Card>
    );
}
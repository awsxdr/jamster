import { Card, CardContent, CardHeader, CardTitle, Input, Label } from "@/components/ui"
import { useI18n, useTeam, useTeamApi } from "@/hooks";
import { ChangeEvent, useEffect, useState } from "react";

type TeamNamesProps = {
    teamId: string;
    className?: string;
}

export const TeamNames = ({ teamId, className }: TeamNamesProps) => {
    const team = useTeam(teamId);
    const [teamName, setTeamName] = useState("");
    const [leagueName, setLeagueName] = useState("");
    const [scoreboardName, setScoreboardName] = useState("");
    const [overlayName, setOverlayName] = useState("");

    const { setTeam } = useTeamApi();
    const { translate } = useI18n();

    useEffect(() => {
        setTeamName(team?.names["team"] ?? "");
        setScoreboardName(team?.names["scoreboard"] ?? "");
        setOverlayName(team?.names["overlay"] ?? "");
    }, [team]);

    const onTeamNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setTeamName(event.target.value);
    }

    const onLeagueNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setLeagueName(event.target.value);
    }

    const onScoreboardNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setScoreboardName(event.target.value);
    }

    const onOverlayNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setOverlayName(event.target.value);
    }

    const onTeamNameBlur = () => {
        if (teamId && team?.names["team"] !== teamName) {
            setTeam(teamId, {
                    ...team,
                    names: { ...team?.names, "team": teamName },
                    colors: team?.colors ?? {}
            });
        }
    }

    const onLeagueNameBlur = () => {
        if (teamId && team?.names["league"] !== leagueName) {
            setTeam(teamId, {
                ...team,
                names: { ...team?.names, "league": leagueName },
                colors: team?.colors ?? {}
            });
        }
    }

    const onScoreboardNameBlur = () => {
        if (teamId && team?.names["scoreboard"] !== scoreboardName) {
            setTeam(teamId, {
                    ...team,
                    names: { ...team?.names, "scoreboard": scoreboardName },
                    colors: team?.colors ?? {}
            });
        }
    }

    const onOverlayNameBlur = () => {
        if (teamId && team?.names["overlay"] !== overlayName) {
            setTeam(teamId, {
                    ...team,
                    names: { ...team?.names, "overlay": overlayName },
                    colors: team?.colors ?? {}
            });
        }
    }

    return (
        <Card className={className}>
            <CardHeader>
                <CardTitle>{ translate("TeamNames.Title") }</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col grid items-center gap-4">
                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="teamName">{ translate("TeamNames.TeamName") }</Label>
                    <Input value={teamName} id="teamName" onChange={onTeamNameChanged} onBlur={onTeamNameBlur} />
                </div>

                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="leagueName">{ translate("TeamNames.LeagueName") }</Label>
                    <Input value={leagueName} id="leagueName" onChange={onLeagueNameChanged} onBlur={onLeagueNameBlur} />
                </div>

                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="scoreboardName">{ translate("TeamNames.ScoreboardName") }</Label>
                    <Input value={scoreboardName} id="scoreboardName" placeholder={teamName} onChange={onScoreboardNameChanged} onBlur={onScoreboardNameBlur} />
                </div>

                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="overlayName">{ translate("TeamNames.OverlayName") }</Label>
                    <Input value={overlayName} id="overlayName" placeholder={teamName} onChange={onOverlayNameChanged} onBlur={onOverlayNameBlur} />
                </div>

            </CardContent>
        </Card>
    )
}
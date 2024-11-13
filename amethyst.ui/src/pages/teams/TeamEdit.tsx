import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from "@/components/ui/breadcrumb";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { useTeam } from "@/hooks/TeamsHook";
import { ChangeEvent, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";

export const TeamEdit = () => {

    const { teamId } = useParams();
    const team = useTeam(teamId!);
    const { setTeam } = useTeamApi();

    const [teamName, setTeamName] = useState(team?.names["team"] ?? "");

    const displayName = useMemo(() => team?.names["team"] || team?.names["league"] || team?.names["default"] || "", [team]);

    useEffect(() => {
        setTeamName(team?.names["team"] ?? "");
    }, [team]);

    const onTeamNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setTeamName(event.target.value);
    }

    const onTeamNameBlur = () => {
        if (teamId && team?.names["team"] !== teamName) {
            setTeam(
                teamId,
                {
                    names: {
                        ...team?.names,
                        "team": teamName
                    },
                    colors: team?.colors ?? {}
                }
            );
        }
    }

    return (
        <>
            <Breadcrumb className="m-4">
                <BreadcrumbList>
                    <BreadcrumbItem>
                        <BreadcrumbLink asChild>
                            <Link to="/teams">Teams</Link>
                        </BreadcrumbLink>
                    </BreadcrumbItem>
                    <BreadcrumbSeparator />
                    <BreadcrumbItem>
                        <BreadcrumbPage>{ displayName }</BreadcrumbPage>
                    </BreadcrumbItem>
                </BreadcrumbList>
            </Breadcrumb>
            <div className="grid max-w-sm items-center gap-1.5">
                <Label htmlFor="teamName">Team name</Label>
                <Input value={teamName} id="teamName" onChange={onTeamNameChanged} onBlur={onTeamNameBlur} />
            </div>
        </>
    );
}
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from "@/components/ui/breadcrumb";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useI18n } from "@/hooks/I18nHook";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { useTeam } from "@/hooks/TeamsHook";
import { ChangeEvent, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";

export const TeamEdit = () => {

    const { teamId } = useParams();
    const team = useTeam(teamId!);
    const { setTeam } = useTeamApi();

    const { translate } = useI18n();

    const [defaultName, setDefaultName] = useState(team?.names["default"] ?? "");

    const displayName = useMemo(() => team?.names["default"] ?? "", [team]);

    useEffect(() => {
        setDefaultName(team?.names["default"] ?? "");
    }, [team]);

    const onTeamNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setDefaultName(event.target.value);
    }

    const onTeamNameBlur = () => {
        if (teamId && team?.names["default"] !== defaultName) {
            setTeam(
                teamId,
                {
                    names: {
                        ...team?.names,
                        "default": defaultName
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
                            <Link to="/teams">{translate("TeamEdit.Teams")}</Link>
                        </BreadcrumbLink>
                    </BreadcrumbItem>
                    <BreadcrumbSeparator />
                    <BreadcrumbItem>
                        <BreadcrumbPage>{ displayName }</BreadcrumbPage>
                    </BreadcrumbItem>
                </BreadcrumbList>
            </Breadcrumb>
            <div className="grid max-w-sm items-center gap-1.5">
                <Label htmlFor="teamName">{ translate("TeamEdit.TeamName") }</Label>
                <Input value={defaultName} id="teamName" onChange={onTeamNameChanged} onBlur={onTeamNameBlur} />
            </div>
        </>
    );
}
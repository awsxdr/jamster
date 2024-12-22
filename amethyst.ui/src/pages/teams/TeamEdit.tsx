import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from "@/components/ui/breadcrumb";
import { useI18n } from "@/hooks/I18nHook";
import { useTeam } from "@/hooks/TeamsHook";
import { useMemo } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { TeamNames } from "./components/TeamNames";
import { TeamColors } from "./components/TeamColors";
import { TeamRoster } from "./components/TeamRoster";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";

export const TeamEdit = () => {

    const { teamId } = useParams();
    const navigate = useNavigate();

    if(!teamId) {
        navigate('/teams');
        return (<></>);
    }

    const team = useTeam(teamId);

    const { translate } = useI18n();

    const displayName = useMemo(() => team?.names["team"] || team?.names["league"] || "", [team]);

    if(!team) {
        return (<></>);
    }

    return (
        <>
            <div className="flex items-center mt-2">
                <MobileSidebarTrigger className="mx-5" />
                <Breadcrumb className="mx-4">
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
            </div>
            <div className="flex flex-col p-4 gap-2 w-full">
                <div className="flex flex-wrap lg:flex-nowrap gap-2 w-full">
                    <TeamNames teamId={teamId} className="w-full lg:w-1/2" />
                    <TeamColors team={team} className="w-full lg:w-1/2" />
                </div>
                <TeamRoster team={team} />
            </div>
        </>
    );
}
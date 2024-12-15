import { useTeamList } from "@/hooks/TeamsHook";
import { TeamTable } from "./components/TeamTable";
import { Button, Separator } from "@/components/ui";
import { Plus, Trash } from "lucide-react";
import { NewTeamDialog, NewTeamDialogContainer, NewTeamDialogTrigger } from "./components/NewTeamDialog";
import { useState } from "react";
import { TeamColor } from "@/types";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { useI18n } from "@/hooks/I18nHook";

export const TeamsManagement = () => {

    const teams = useTeamList();
    const { createTeam } = useTeamApi();

    const { translate } = useI18n();
    
    const [newTeamDialogOpen, setNewTeamDialogOpen] = useState(false);
    const [selectedTeamIds, setSelectedTeamIds] = useState<string[]>([]);

    const handleNewTeamCreated = (name: string, colorName: string, colors: TeamColor) => {
        createTeam({
            names: {
                default: name,
            },
            colors: {
                [colorName]: colors,
            },
        }).then(() => {
            setNewTeamDialogOpen(false);
        });
    }

    const handleNewTeamCancelled = () => {
        setNewTeamDialogOpen(false);
    }

    return (
        <>
            <NewTeamDialogContainer open={newTeamDialogOpen} onOpenChange={setNewTeamDialogOpen}>
                <div className="flex w-full">
                    <div className="grow">
                        <MobileSidebarTrigger className="m-5" />
                    </div>
                    <div className="flex gap-5 m-5">
                        <NewTeamDialogTrigger>
                            <Button variant="creative">
                                <Plus />
                                { translate("TeamsManagement.AddTeam") }
                            </Button>
                        </NewTeamDialogTrigger>
                        <Button variant="destructive" disabled={selectedTeamIds.length === 0}>
                            <Trash />
                            { translate("TeamsManagement.DeleteTeam") }
                        </Button>
                    </div>
                </div>
                <Separator />
                <TeamTable teams={teams} selectedTeamIds={selectedTeamIds} onSelectedTeamIdsChanged={setSelectedTeamIds} />
                <NewTeamDialog onNewTeamCreated={handleNewTeamCreated} onCancelled={handleNewTeamCancelled} />
            </NewTeamDialogContainer>
        </>
    );
}
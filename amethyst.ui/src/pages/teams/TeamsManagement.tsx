import { useTeamList } from "@/hooks/TeamsHook";
import { TeamTable } from "./components/TeamTable";
import { Button, Separator } from "@/components/ui";
import { Plus } from "lucide-react";
import { NewTeamDialog, NewTeamDialogContainer, NewTeamDialogTrigger } from "./components/NewTeamDialog";
import { useState } from "react";
import { TeamColor } from "@/types";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";

export const TeamsManagement = () => {

    const teams = useTeamList();
    const { createTeam } = useTeamApi();
    
    const [newTeamDialogOpen, setNewTeamDialogOpen] = useState(false);

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
                    <div>
                        <NewTeamDialogTrigger>
                            <Button variant="creative" className="m-5 self-end">
                                <Plus />
                                Add team
                            </Button>
                        </NewTeamDialogTrigger>
                    </div>
                </div>
                <Separator />
                <TeamTable teams={teams} />
                <NewTeamDialog onNewTeamCreated={handleNewTeamCreated} onCancelled={handleNewTeamCancelled} />
            </NewTeamDialogContainer>
        </>
    );
}
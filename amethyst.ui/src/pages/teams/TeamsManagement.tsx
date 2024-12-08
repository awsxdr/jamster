import { useTeamList } from "@/hooks/TeamsHook";
import { TeamTable } from "./components/TeamTable";
import { Button } from "@/components/ui";
import { Plus } from "lucide-react";
import { NewTeamDialog, NewTeamDialogContainer, NewTeamDialogTrigger } from "./components/NewTeamDialog";
import { useState } from "react";
import { TeamColor } from "@/types";
import { useTeamApi } from "@/hooks/TeamApiHook";

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
                <NewTeamDialogTrigger>
                    <Button variant="creative">
                        <Plus />
                    </Button>
                </NewTeamDialogTrigger>
                <TeamTable teams={teams} />
                <NewTeamDialog onNewTeamCreated={handleNewTeamCreated} onCancelled={handleNewTeamCancelled} />
            </NewTeamDialogContainer>
        </>
    );
}
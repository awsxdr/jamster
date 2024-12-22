import { useTeamList } from "@/hooks/TeamsHook";
import { TeamTable } from "./components/TeamTable";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Separator } from "@/components/ui";
import { Plus, Trash } from "lucide-react";
import { NewTeamDialog, NewTeamDialogContainer, NewTeamDialogTrigger } from "./components/NewTeamDialog";
import { useState } from "react";
import { TeamColor } from "@/types";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { useI18n } from "@/hooks/I18nHook";

export const TeamsManagement = () => {

    const teams = useTeamList();
    const { createTeam, deleteTeam } = useTeamApi();

    const { translate } = useI18n();
    
    const [newTeamDialogOpen, setNewTeamDialogOpen] = useState(false);
    const [selectedTeamIds, setSelectedTeamIds] = useState<string[]>([]);

    const handleNewTeamCreated = (name: string, colorName: string, colors: TeamColor) => {
        createTeam({
            names: {
                team: name,
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

    const handleDeleteSelected = () => {
        selectedTeamIds
            .map(rowId => teams[parseInt(rowId)].id)
            .forEach(teamId => {
                deleteTeam(teamId);
            });
        setSelectedTeamIds([]);
    }

    return (
        <>
            <NewTeamDialogContainer open={newTeamDialogOpen} onOpenChange={setNewTeamDialogOpen}>
                <div className="flex w-full items-center mt-1 mb-2 pr-2">
                    <div className="grow">
                        <MobileSidebarTrigger className="mx-5" />
                    </div>
                    <div className="flex gap-2">
                        <NewTeamDialogTrigger>
                            <Button variant="creative">
                                <Plus />
                                { translate("TeamsManagement.AddTeam") }
                            </Button>
                        </NewTeamDialogTrigger>
                        <AlertDialog>
                            <AlertDialogTrigger asChild>
                                <Button variant="destructive" disabled={selectedTeamIds.length === 0}>
                                    <Trash />
                                    { translate("TeamsManagement.DeleteTeam") }
                                </Button>
                            </AlertDialogTrigger>
                            <AlertDialogContent>
                                <AlertDialogHeader>
                                    <AlertDialogTitle>{ translate("TeamManagement.DeleteTeamDialog.Title") }</AlertDialogTitle>
                                    <AlertDialogDescription>{ translate("TeamManagement.DeleteTeamDialog.Description") }</AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter>
                                    <AlertDialogCancel>{ translate("TeamManagement.DeleteTeamDialog.Cancel") }</AlertDialogCancel>
                                    <AlertDialogAction className={buttonVariants({ variant: "destructive" })} onClick={handleDeleteSelected}>
                                        { translate("TeamManagement.DeleteTeamDialog.Confirm") }
                                    </AlertDialogAction>
                                </AlertDialogFooter>
                            </AlertDialogContent>
                        </AlertDialog>
                    </div>
                </div>
                <Separator />
                <TeamTable teams={teams} selectedTeamIds={selectedTeamIds} onSelectedTeamIdsChanged={setSelectedTeamIds} />
                <NewTeamDialog onNewTeamCreated={handleNewTeamCreated} onCancelled={handleNewTeamCancelled} />
            </NewTeamDialogContainer>
        </>
    );
}
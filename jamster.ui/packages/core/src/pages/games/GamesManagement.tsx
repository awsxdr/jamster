import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { NewGameCreated, NewGameDialog, NewGameDialogContainer, NewGameDialogTrigger } from "@/components/NewGameDialog";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Separator } from "@/components/ui"
import { useCreateGame, useGameApi, useGamesList, useI18n, useIsMobile } from "@/hooks"
import { Plus, Trash, Upload } from "lucide-react"
import { useState } from "react";
import { UploadDialog, UploadDialogContainer, UploadDialogTrigger } from "./components/UploadDialog";
import { GameTable } from "./components/GameTable";

export const GamesManagement = () => {

    const { translate } = useI18n();

    const [newGameDialogOpen, setNewGameDialogOpen] = useState(false);
    const [uploadDialogOpen, setUploadDialogOpen] = useState(false);

    const { uploadGame, deleteGame } = useGameApi();
    const createGame = useCreateGame();

    const games = useGamesList();

    const handleGameUploaded = async (file: File) => {
        await uploadGame(file);
        setUploadDialogOpen(false);
    }

    const isMobile = useIsMobile();

    const [selectedGameIds, setSelectedGameIds] = useState<string[]>([]);

    const handleNewGameCreated: NewGameCreated = async (...parameters) => {

        await createGame(...parameters);

        setSelectedGameIds([]);

        setNewGameDialogOpen(false);
    }


    const handleDelete = () => {
        selectedGameIds
            .map(rowId => games[parseInt(rowId)].id)
            .forEach(teamId => {
                deleteGame(teamId);
            });
        setSelectedGameIds([]);
    }

    return (
        <>
            <title>{translate("GamesManagement.Title")} | {translate("Main.Title")}</title>
            <div className="flex w-full items-center mt-1 mb-2 pr-2">
                <div className="grow">
                    <MobileSidebarTrigger className="mx-5" />
                </div>
                <div className="flex gap-2">
                    <NewGameDialogContainer open={newGameDialogOpen} onOpenChange={setNewGameDialogOpen}>
                        <NewGameDialogTrigger>
                            <Button variant="creative" id="GamesManagement.NewGameButton">
                                <Plus />
                                { !isMobile && translate("GamesManagement.NewGame") }
                            </Button>
                        </NewGameDialogTrigger>
                        <NewGameDialog onNewGameCreated={handleNewGameCreated} onCancelled={() => setNewGameDialogOpen(false)} />
                    </NewGameDialogContainer>
                    <UploadDialogContainer open={uploadDialogOpen} onOpenChange={setUploadDialogOpen}>
                        <UploadDialogTrigger>
                            <Button>
                                <Upload />
                                { !isMobile && translate("GamesManagement.Upload") }
                            </Button>
                        </UploadDialogTrigger>
                        <UploadDialog onGameUploaded={handleGameUploaded} onCancelled={() => setUploadDialogOpen(false)} />
                    </UploadDialogContainer>
                    <AlertDialog>
                        <AlertDialogTrigger asChild>
                            <Button variant="destructive" disabled={selectedGameIds.length === 0}>
                                <Trash />
                                { !isMobile && translate("GamesManagement.DeleteGame") }
                            </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                            <AlertDialogHeader>
                                <AlertDialogTitle>{ translate("GamesManagement.DeleteGameDialog.Title") }</AlertDialogTitle>
                                <AlertDialogDescription>{ translate("GamesManagement.DeleteGameDialog.Description") }</AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                                <AlertDialogCancel>{ translate("GamesManagement.DeleteGameDialog.Cancel") }</AlertDialogCancel>
                                <AlertDialogAction className={buttonVariants({ variant: "destructive" })} onClick={handleDelete}>
                                    { translate("GamesManagement.DeleteGameDialog.Confirm") }
                                </AlertDialogAction>
                            </AlertDialogFooter>
                        </AlertDialogContent>
                    </AlertDialog>
                </div>
            </div>
            <Separator />
            <div>
                <GameTable games={games} selectedGameIds={selectedGameIds} onSelectedGameIdsChanged={setSelectedGameIds} />
            </div>
        </>
    )
}
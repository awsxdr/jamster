import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { NewGameDialog, NewGameDialogContainer, NewGameDialogTrigger } from "@/components/NewGameDialog";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Separator } from "@/components/ui"
import { useGameApi, useGamesList, useI18n } from "@/hooks"
import { Plus, Trash, Upload } from "lucide-react"
import { useState } from "react";
import { UploadDialog, UploadDialogContainer, UploadDialogTrigger } from "./components/UploadDialog";
import { GameTable } from "./components/GameTable";

export const GamesManagement = () => {

    const { translate } = useI18n();

    const [newGameDialogOpen, setNewGameDialogOpen] = useState(false);
    const [uploadDialogOpen, setUploadDialogOpen] = useState(false);

    const { uploadGame } = useGameApi();
    const games = useGamesList();

    const handleGameUploaded = async (file: File) => {
        await uploadGame(file);
        setUploadDialogOpen(false);
    }

    return (
        <>
            <div className="flex w-full items-center mt-1 mb-2 pr-2">
                <div className="grow">
                    <MobileSidebarTrigger className="mx-5" />
                </div>
                <div className="flex gap-2">
                    <NewGameDialogContainer open={newGameDialogOpen} onOpenChange={setNewGameDialogOpen}>
                        <NewGameDialogTrigger>
                            <Button variant="creative">
                                <Plus />
                                { translate("GamesManagement.NewGame") }
                            </Button>
                        </NewGameDialogTrigger>
                        <NewGameDialog />
                    </NewGameDialogContainer>
                    <UploadDialogContainer open={uploadDialogOpen} onOpenChange={setUploadDialogOpen}>
                        <UploadDialogTrigger>
                            <Button>
                                <Upload />
                                { translate("GamesManagement.Upload") }
                            </Button>
                        </UploadDialogTrigger>
                        <UploadDialog onGameUploaded={handleGameUploaded} onCancelled={() => setUploadDialogOpen(false)} />
                    </UploadDialogContainer>
                    <AlertDialog>
                        <AlertDialogTrigger asChild>
                            <Button variant="destructive">
                                <Trash />
                                { translate("GamesManagement.DeleteGame") }
                            </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                            <AlertDialogHeader>
                                <AlertDialogTitle>{ translate("GamesManagement.DeleteGameDialog.Title") }</AlertDialogTitle>
                                <AlertDialogDescription>{ translate("GamesManagement.DeleteGameDialog.Description") }</AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                                <AlertDialogCancel>{ translate("GamesManagement.DeleteGameDialog.Cancel") }</AlertDialogCancel>
                                <AlertDialogAction className={buttonVariants({ variant: "destructive" })}>
                                    { translate("GamesManagement.DeleteGameDialog.Confirm") }
                                </AlertDialogAction>
                            </AlertDialogFooter>
                        </AlertDialogContent>
                    </AlertDialog>
                </div>
            </div>
            <Separator />
            <div>
                <GameTable games={games} />
            </div>
        </>
    )
}
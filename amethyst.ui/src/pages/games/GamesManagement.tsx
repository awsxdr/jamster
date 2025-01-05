import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { NewGameDialog, NewGameDialogContainer, NewGameDialogTrigger } from "@/components/NewGameDialog";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Separator } from "@/components/ui"
import { useEvents, useGameApi, useGamesList, useI18n, useIsMobile, useTeamApi } from "@/hooks"
import { Plus, Trash, Upload } from "lucide-react"
import { useState } from "react";
import { UploadDialog, UploadDialogContainer, UploadDialogTrigger } from "./components/UploadDialog";
import { GameTable } from "./components/GameTable";
import { Team, TeamSide } from "@/types";
import { TeamSet } from "@/types/events";

export const GamesManagement = () => {

    const { translate } = useI18n();

    const [newGameDialogOpen, setNewGameDialogOpen] = useState(false);
    const [uploadDialogOpen, setUploadDialogOpen] = useState(false);

    const { createGame, uploadGame, deleteGame } = useGameApi();
    const { getTeam } = useTeamApi();
    const { sendEvent } = useEvents();
    const games = useGamesList();

    const handleGameUploaded = async (file: File) => {
        await uploadGame(file);
        setUploadDialogOpen(false);
    }

    const isMobile = useIsMobile();

    const [selectedGameIds, setSelectedGameIds] = useState<string[]>([]);

    const handleNewGameCreated = async (homeTeamId: string, homeTeamColorIndex: number, awayTeamId: string, awayTeamColorIndex: number, gameName: string) => {
        const gameId = await createGame(gameName);

        const homeTeam = await getTeam(homeTeamId)
        const awayTeam = await getTeam(awayTeamId);

        const getTeamColor = (team: Team, colorIndex: number) => {
            const colorKeys = Object.keys(team.colors);
            if(colorKeys.length === 0) {
                return {
                    shirtColor: '#000000',
                    complementaryColor: '#ffffff',
                };
            }

            if(colorIndex > colorKeys.length) {
                colorIndex = 0;
            }

            return team.colors[colorKeys[colorIndex]]!;
        }

        const homeTeamColor = getTeamColor(homeTeam, homeTeamColorIndex);
        const awayTeamColor = getTeamColor(awayTeam, awayTeamColorIndex);

        const homeGameTeam = {
            names: homeTeam.names,
            color: homeTeamColor,
            roster: homeTeam.roster.map(s => ({ ...s, isSkating: true })),
        };

        const awayGameTeam = {
            names: awayTeam.names,
            color: awayTeamColor,
            roster: awayTeam.roster.map(s => ({ ...s, isSkating: true })),
        };
        
        await sendEvent(gameId, new TeamSet(TeamSide.Home, homeGameTeam));
        await sendEvent(gameId, new TeamSet(TeamSide.Away, awayGameTeam));

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
                            <Button variant="creative">
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
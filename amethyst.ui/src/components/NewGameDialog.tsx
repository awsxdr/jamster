import { ChangeEvent, PropsWithChildren, useEffect, useState } from "react";
import { useI18n } from "@/hooks/I18nHook";
import { useTeamList } from "@/hooks/TeamsHook";
import { Team } from "@/types";
import { Button, Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Input, Label, Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@components/ui";
import { Loader2, RefreshCcw } from "lucide-react";
import { DateTime } from 'luxon';
import { TeamSelect } from "./TeamSelect";

type NewGameDialogContainerProps = {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
}

export const NewGameDialogContainer = ({ children, ...props }: PropsWithChildren<NewGameDialogContainerProps>) => {
    return (
        <Dialog {...props}>
            {children}
        </Dialog>
    );
}

export const NewGameDialogTrigger = ({children}: PropsWithChildren) => {
    return (
        <DialogTrigger asChild>
            {children}
        </DialogTrigger>
    )
}

export type NewGameCreated = (
    homeTeamId: string, 
    homeTeamColorIndex: number,
    awayTeamId: string, 
    awayTeamColorIndex: number,
    gameName: string) => void;

type NewGameDialogProps = {
    onNewGameCreated?: NewGameCreated;
    onCancelled?: () => void;
}

export const NewGameDialog = ({ onNewGameCreated, onCancelled }: NewGameDialogProps) => {

    const { translate } = useI18n();

    const teams = useTeamList();

    const getDisplayName = (team: Team) =>
        team.names["team"] || team.names["league"] || team.names["default"] || "";

    const [homeTeamId, setHomeTeamId] = useState<string>();
    const [awayTeamId, setAwayTeamId] = useState<string>();

    const [isCreating, setIsCreating] = useState(false);

    const [homeTeamColorIndex, setHomeTeamColorIndex] = useState(0);
    const [awayTeamColorIndex, setAwayTeamColorIndex] = useState(0);
    
    const [gameName, setGameName] = useState("");
    const [gameNameDirty, setGameNameDirty] = useState(false);

    const date = DateTime.now().toISODate();

    useEffect(() => {
        if(gameNameDirty) return;

        const getTeamName = (teamId?: string) => {
            const team = teamId && teams.filter(t => t.id === teamId)[0];
            
            if(team) {
                return getDisplayName(team);
            } else {
                return translate("NewGameDialog.UnknownTeam");
            }
        }

        setGameName(`${date} - ${getTeamName(homeTeamId)} ${translate("NewGameDialog.Versus")} ${getTeamName(awayTeamId)}`);
    }, [homeTeamId, awayTeamId, teams, gameNameDirty]);

    const handleTeamNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setGameNameDirty(true);
        setGameName(event.target.value);
    }

    const handleResetTeamName = () => setGameNameDirty(false);

    const clearValues = () => {
        setHomeTeamId(undefined);
        setAwayTeamId(undefined);
        setGameNameDirty(false);
        setIsCreating(false);
    }

    const handleNewGameClicked = () => {
        setIsCreating(true);
        onNewGameCreated?.(homeTeamId!, homeTeamColorIndex, awayTeamId!, awayTeamColorIndex, gameName);
        clearValues();
    }

    const handleCancelClicked = () => {
        clearValues();
        onCancelled?.();
    }

    return (
        <TooltipProvider>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{translate("NewGameDialog.Title")}</DialogTitle>
                    <DialogDescription>{translate("NewGameDialog.Description")}</DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <TeamSelect 
                        titleKey="NewGameDialog.HomeTeam" 
                        teamId={homeTeamId} 
                        exceptIds={awayTeamId ? [awayTeamId] : []}
                        colorIndex={homeTeamColorIndex}
                        onTeamIdChanged={setHomeTeamId}
                        onColorIndexChanged={setHomeTeamColorIndex}
                    />
                    <TeamSelect 
                        titleKey="NewGameDialog.AwayTeam" 
                        teamId={awayTeamId} 
                        exceptIds={homeTeamId ? [homeTeamId] : []}
                        colorIndex={awayTeamColorIndex}
                        onTeamIdChanged={setAwayTeamId}
                        onColorIndexChanged={setAwayTeamColorIndex}
                    />
                    <Label className="text-lg font-bold">{translate("NewGameDialog.GameName")}</Label>
                    <div className="flex">
                        <Input value={gameName} onChange={handleTeamNameChanged} />
                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button variant="ghost" size="icon" onClick={handleResetTeamName} disabled={!gameNameDirty}>
                                    <RefreshCcw />
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                                {translate("NewGameDialog.ResetName")}
                            </TooltipContent>
                        </Tooltip>
                    </div>
                    <DialogFooter>
                        <Button
                            variant="outline"
                            className="mt-4"
                            onClick={handleCancelClicked}
                        >
                            {translate("NewGameDialog.Cancel")}
                        </Button>
                        <Button 
                            variant="default" 
                            type="submit"
                            className="mt-4" 
                            disabled={isCreating || !homeTeamId || !awayTeamId}
                            onClick={handleNewGameClicked}
                        >
                            {isCreating && <Loader2 className="animate-spin" />}
                            {translate("NewGameDialog.Create")}
                        </Button>
                    </DialogFooter>
                </div>
            </DialogContent>
        </TooltipProvider>
    );
}
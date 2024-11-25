import { ChangeEvent, PropsWithChildren, useEffect, useMemo, useState } from "react";
import { useI18n } from "@/hooks/I18nHook";
import { useTeamList } from "@/hooks/TeamsHook";
import { Team } from "@/types";
import { Button, ComboBox, Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Input, Label, Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@components/ui";
import { RefreshCcw } from "lucide-react";
import { DateTime } from 'luxon';

export const NewGameDialogContainer = ({ children }: PropsWithChildren) => {
    return (
        <Dialog>
            {children}
        </Dialog>
    );
}

export const NewGameDialogTrigger = ({children}: PropsWithChildren) => {
    return (
        <DialogTrigger>
            {children}
        </DialogTrigger>
    )
}

type NewGameCreated = (homeTeamId: string, awayTeamId: string, gameName: string) => void;

type NewGameDialogProps = {
    onNewGameCreated?: NewGameCreated
}

export const NewGameDialog = ({ onNewGameCreated }: NewGameDialogProps) => {

    const { translate } = useI18n();

    const teams = useTeamList();

    const getDisplayName = (team: Team) =>
        team.names["team"] || team.names["league"] || team.names["default"] || "";

    const teamItems = useMemo(() =>
        teams.map(team => ({
            value: team.id,
            text: 
                !!team.names["league"] && !!team.names["team"] ? `${team.names["team"]} (${team.names["league"]})`
                : getDisplayName(team),
        })), [teams]);

    const [homeTeamId, setHomeTeamId] = useState<string>();
    const [awayTeamId, setAwayTeamId] = useState<string>();

    const homeTeamItems = useMemo(() => 
        teamItems.filter(team => team.value !== awayTeamId)
    , [teamItems, awayTeamId]);
    const awayTeamItems = useMemo(() => 
        teamItems.filter(team => team.value !== homeTeamId)
    , [teamItems, homeTeamId]);

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
                return translate("...");
            }
        }

        setGameName(`${date} - ${getTeamName(homeTeamId)} ${translate("vs")} ${getTeamName(awayTeamId)}`);
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
    }

    return (
        <TooltipProvider>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{translate("NewGameDialog.Title")}</DialogTitle>
                    <DialogDescription>{translate("NewGameDialog.Description")}</DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <Label>{translate("NewGameDialog.HomeTeam")}</Label>
                    <ComboBox 
                        items={homeTeamItems} 
                        value={homeTeamId ?? ""} 
                        placeholder={translate("NewGameDialog.SelectTeam")} 
                        onValueChanged={setHomeTeamId}
                        dropdownClassName="w-[460px]"
                    />
                    <Label>{translate("NewGameDialog.AwayTeam")}</Label>
                    <ComboBox 
                        items={awayTeamItems} 
                        value={awayTeamId ?? ""} 
                        placeholder={translate("NewGameDialog.SelectTeam")} 
                        onValueChanged={setAwayTeamId}
                        dropdownClassName="w-[460px]"
                    />
                    <Label>{translate("NewGameDialog.GameName")}</Label>
                    <div className="flex">
                        <Input value={gameName} onChange={handleTeamNameChanged} />
                        <Tooltip>
                            <TooltipTrigger>
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
                    <DialogClose asChild>
                            <Button
                                variant="outline"
                                className="mt-4"
                                onClick={clearValues}
                            >
                                Cancel
                            </Button>
                        </DialogClose>
                        <DialogClose asChild>
                            <Button 
                                variant="default" 
                                type="submit"
                                className="mt-4" 
                                disabled={!homeTeamId || !awayTeamId}
                                onClick={() => onNewGameCreated?.(homeTeamId!, awayTeamId!, gameName)}
                            >
                                Create
                            </Button>
                        </DialogClose>
                    </DialogFooter>
                </div>
            </DialogContent>
        </TooltipProvider>
    );
}
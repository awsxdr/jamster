import { ChangeEvent, PropsWithChildren, useEffect, useMemo, useState } from "react";
import { useI18n } from "@/hooks/I18nHook";
import { useTeamList } from "@/hooks/TeamsHook";
import { Team } from "@/types";
import { Button, ComboBox, Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Input, Label, Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@components/ui";
import { Loader2, RefreshCcw } from "lucide-react";
import { DateTime } from 'luxon';
import { cn } from "@/lib/utils";

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
        <DialogTrigger>
            {children}
        </DialogTrigger>
    )
}

type ColorBlockProps = {
    color: string;
}

const ColorBlock = ({ color }: ColorBlockProps) => (
    <span 
        className="display-inline-block w-5 h-5 self-center" 
        style={{ backgroundColor: color}}
    >
    </span>
)

type TeamSelectProps = {
    titleKey: string;
    teamId?: string;
    exceptIds?: string[];
    colorIndex?: number;
    onTeamIdChanged?: (teamId: string) => void;
    onColorIndexChanged?: (colorIndex: number) => void;
}

const TeamSelect = ({ titleKey, teamId, exceptIds, colorIndex, onTeamIdChanged, onColorIndexChanged }: TeamSelectProps) => {
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
        }))
        .filter(team => !(exceptIds?.find(eid => team.value === eid))),
        [teams, exceptIds]);

    const teamColors = useMemo(() => {
        const team = teams.find(team => team.id === teamId);

        if(!team) {
            return [];
        }

        return Object.keys(team.colors)
            .map((key, i) => ({
                value: i.toString(),
                text: key,
                foreground: team.colors[key]?.complementaryColor ?? '#ffffff',
                background: team.colors[key]?.shirtColor ?? '#000000',
            }));
    }, [teams, teamId]);

    useEffect(
        () => onColorIndexChanged?.(0),
        [teamColors, onColorIndexChanged]);

    const handleColorIndexChanged = (key: string) => {
        const index = parseInt(key);
        if(!Number.isNaN(index)) {
            onColorIndexChanged?.(index);
        }
    }
    
    return (
        <>
            <Label className="font-bold text-lg">{translate(titleKey)}</Label>
            <ComboBox 
                items={teamItems} 
                value={teamId ?? ""} 
                placeholder={translate("NewGameDialog.SelectTeam")} 
                onValueChanged={onTeamIdChanged ?? (() => {})}
                dropdownClassName="w-[460px]"
            />
            <span className={cn(
                "flex gap-2 items-baseline transition-all duration-500 h-0 overflow-hidden",
                teamColors.length > 1 ? "h-auto" : ""
            )}>
                <span>Kit</span>
                <ComboBox
                    items={teamColors}
                    value={colorIndex?.toString() ?? ""}
                    placeholder={translate("NewGameDialog.SelectKit")}
                    onValueChanged={handleColorIndexChanged}
                    dropdownClassName="w-[460px]"
                    className="grow"
                    disabled={!teamId || !teamColors}
                    hideSearch
                />
                <ColorBlock color={teamColors[colorIndex ?? 0]?.background ?? '#000000'} />
                <ColorBlock color={teamColors[colorIndex ?? 0]?.foreground ?? '#000000'} />
            </span>
        </>
    )
}

type NewGameCreated = (
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
    }

    const handleNewGameClicked = () => {
        setIsCreating(true);

        onNewGameCreated?.(homeTeamId!, homeTeamColorIndex, awayTeamId!, awayTeamColorIndex, gameName);
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
                        <Button
                            variant="outline"
                            className="mt-4"
                            onClick={handleCancelClicked}
                        >
                            Cancel
                        </Button>
                        <Button 
                            variant="default" 
                            type="submit"
                            className="mt-4" 
                            disabled={isCreating || !homeTeamId || !awayTeamId}
                            onClick={handleNewGameClicked}
                        >
                            { isCreating && <Loader2 className="animate-spin" /> }
                            Create
                        </Button>
                    </DialogFooter>
                </div>
            </DialogContent>
        </TooltipProvider>
    );
}
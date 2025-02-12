import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { History, List, NotebookText, Settings } from "lucide-react";
import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { useCurrentGame, useI18n } from "@/hooks";
import { RulesDialog, RulesDialogContainer, RulesDialogTrigger } from ".";

type SettingsMenuProps = {
    disabled?: boolean;
}

export const SettingsMenu = ({ disabled }: SettingsMenuProps) => {

    const { currentGame } = useCurrentGame();
    const [ searchParams ] = useSearchParams();
    const gameId = searchParams.get('gameId');
    const [rulesDialogOpen, setRulesDialogOpen] = useState(false);

    const { translate } = useI18n();

    if(gameId === null) {
        return (<></>);
    }

    return (
        <RulesDialogContainer open={rulesDialogOpen} onOpenChange={setRulesDialogOpen}>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button size="icon" variant="ghost" disabled={disabled}>
                        <Settings />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56">
                    { currentGame && (
                        <>
                            <DropdownMenuItem disabled={disabled} asChild>
                                <Link to={`/games/${currentGame?.id}`}>
                                    <List />
                                    {translate("ScoreboardControl.SettingsMenu.Settings")}
                                </Link>
                            </DropdownMenuItem>
                            <RulesDialogTrigger asChild>
                                <DropdownMenuItem disabled={disabled}>
                                    <NotebookText />
                                    {translate("ScoreboardControl.SettingsMenu.Rules")}
                                </DropdownMenuItem>
                            </RulesDialogTrigger>
                            <DropdownMenuItem disabled={disabled} asChild>
                                <Link to={`/games/${currentGame?.id}/timeline`}>
                                    <History />
                                    {translate("ScoreboardControl.SettingsMenu.Timeline")}
                                </Link>
                            </DropdownMenuItem>
                        </>
                    )}
                </DropdownMenuContent>
            </DropdownMenu>
            <RulesDialog gameId={gameId} />
        </RulesDialogContainer>
    )
}
import { useState } from "react";
import { Link } from "react-router-dom";
import { History, List, NotebookText, Settings } from "lucide-react";
import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { GameStateContextProvider, useI18n } from "@/hooks";
import { RulesDialog, RulesDialogContainer, RulesDialogTrigger } from ".";

type SettingsMenuProps = {
    gameId: string;
    disabled?: boolean;
}

export const SettingsMenu = ({ gameId, disabled }: SettingsMenuProps) => {

    const [rulesDialogOpen, setRulesDialogOpen] = useState(false);

    const { translate } = useI18n();

    return (
        <GameStateContextProvider gameId={gameId}>
            <RulesDialogContainer open={rulesDialogOpen} onOpenChange={setRulesDialogOpen}>
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button size="icon" variant="ghost" disabled={disabled}>
                            <Settings />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent className="w-56">
                        <DropdownMenuItem disabled={disabled} asChild>
                            <Link to={`/games/${gameId}`}>
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
                            <Link to={`/games/${gameId}/timeline`}>
                                <History />
                                {translate("ScoreboardControl.SettingsMenu.Timeline")}
                            </Link>
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
                <RulesDialog gameId={gameId} />
            </RulesDialogContainer>
        </GameStateContextProvider>
    )
}
import { useState } from "react";
import { Link } from "react-router-dom";
import { History, List, NotebookText, Settings } from "lucide-react";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { GameStateContextProvider, useI18n } from "@/hooks";
import { RulesDialog, RulesDialogContainer, RulesDialogTrigger } from ".";
import { TooltipButton } from "@/components";

type SettingsMenuProps = {
    gameId: string;
    disabled?: boolean;
}

export const SettingsMenu = ({ gameId, disabled }: SettingsMenuProps) => {

    const [rulesDialogOpen, setRulesDialogOpen] = useState(false);

    const { translate } = useI18n({ prefix: "ScoreboardControl.SettingsMenu." });

    return (
        <GameStateContextProvider gameId={gameId}>
            <RulesDialogContainer open={rulesDialogOpen} onOpenChange={setRulesDialogOpen}>
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <TooltipButton description={translate("Tooltip")} size="icon" variant="ghost" disabled={disabled}>
                            <Settings />
                        </TooltipButton>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent className="w-56">
                        <DropdownMenuItem disabled={disabled} asChild>
                            <Link to={`/games/${gameId}`}>
                                <List />
                                {translate("Settings")}
                            </Link>
                        </DropdownMenuItem>
                        <RulesDialogTrigger className="w-full">
                            <DropdownMenuItem disabled={disabled}>
                                <NotebookText />
                                {translate("Rules")}
                            </DropdownMenuItem>
                        </RulesDialogTrigger>
                        <DropdownMenuItem disabled={disabled} asChild>
                            <Link to={`/games/${gameId}/timeline`}>
                                <History />
                                {translate("Timeline")}
                            </Link>
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
                <RulesDialog gameId={gameId} />
            </RulesDialogContainer>
        </GameStateContextProvider>
    )
}
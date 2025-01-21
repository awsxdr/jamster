import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { useCurrentGame, useI18n } from "@/hooks";
import { History, Keyboard, List, Settings } from "lucide-react";
import { Link } from "react-router-dom";
import { ShortcutConfigurationDialog, ShortcutConfigurationDialogContainer, ShortcutConfigurationDialogTrigger } from "./ShortcutConfigurationDialog";

type SettingsMenuProps = {
    disabled?: boolean;
}

export const SettingsMenu = ({ disabled }: SettingsMenuProps) => {

    const { currentGame } = useCurrentGame();

    const { translate } = useI18n();

    return (
        <ShortcutConfigurationDialogContainer>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button size="icon" variant="ghost" disabled={disabled}>
                        <Settings />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56">
                    { currentGame && (
                        <>
                            <ShortcutConfigurationDialogTrigger>
                                <DropdownMenuItem disabled={disabled}>
                                    <Keyboard />
                                    {translate("ScoreboardControl.SettingsMenu.Inputs")}
                                </DropdownMenuItem>
                            </ShortcutConfigurationDialogTrigger>
                            <DropdownMenuItem disabled={disabled} asChild>
                                <Link to={`/games/${currentGame?.id}`}>
                                    <List />
                                    {translate("ScoreboardControl.SettingsMenu.Settings")}
                                </Link>
                            </DropdownMenuItem>
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
            <ShortcutConfigurationDialog />
        </ShortcutConfigurationDialogContainer>
    )
}
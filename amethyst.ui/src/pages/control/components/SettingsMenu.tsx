import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { useCurrentGame } from "@/hooks";
import { List, Settings } from "lucide-react";
import { Link } from "react-router-dom";

type SettingsMenuProps = {
    disabled?: boolean;
}

export const SettingsMenu = ({ disabled }: SettingsMenuProps) => {

    const { currentGame } = useCurrentGame();

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button size="icon" variant="ghost" disabled={disabled}>
                    <Settings />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                { currentGame &&
                    <DropdownMenuItem disabled={disabled} asChild>
                        <Link to={`/games/${currentGame?.id}`}>
                            <List />
                            Game settings
                        </Link>
                    </DropdownMenuItem>
                }
            </DropdownMenuContent>
        </DropdownMenu>
    )
}
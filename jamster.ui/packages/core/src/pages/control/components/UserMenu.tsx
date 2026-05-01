import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui";
import { useI18n } from "@/hooks";
import { Keyboard, LogOut, UserRound } from "lucide-react";
import { LoginDialog, LoginDialogContainer, LoginDialogTrigger } from "./LoginDialog";
import { useUserLogin } from "@/hooks/UserLogin";
import { ShortcutConfigurationDialog, ShortcutConfigurationDialogContainer, ShortcutConfigurationDialogTrigger } from "./ShortcutConfigurationDialog";
import { TooltipButton } from "@/components";

type UserMenuProps = {
    disabled?: boolean;
}

export const UserMenu = ({ disabled }: UserMenuProps) => {

    const { userName, logout } = useUserLogin();
    const { translate } = useI18n({ prefix: "ScoreboardControl.UserMenu." });

    const handleLogOut = () => {
        logout();
    }

    if (userName) {
        return (
            <ShortcutConfigurationDialogContainer>
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" disabled={disabled}>
                            <UserRound />
                            <span className="hidden lg:inline">
                                {translate("Welcome").replace("{userName}", userName)}
                            </span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent className="w-56">
                        <DropdownMenuGroup className="block lg:hidden">
                            <DropdownMenuLabel>Hi, {userName}!</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                        </DropdownMenuGroup>
                        <ShortcutConfigurationDialogTrigger className="w-full">
                            <DropdownMenuItem disabled={disabled}>
                                <Keyboard />
                                {translate("Inputs")}
                            </DropdownMenuItem>
                        </ShortcutConfigurationDialogTrigger>
                        <DropdownMenuSeparator />
                        <DropdownMenuGroup>
                            <DropdownMenuItem disabled={disabled} onClick={handleLogOut}>
                                <LogOut />
                                {translate("Logout")}
                            </DropdownMenuItem>
                        </DropdownMenuGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
                <ShortcutConfigurationDialog />
            </ShortcutConfigurationDialogContainer>
        );
    } else {
        return (
            <LoginDialogContainer>
                <LoginDialogTrigger>
                    <TooltipButton description={translate("Login.Tooltip")} variant="secondary" disabled={disabled}>
                        <UserRound />
                        {translate("Login")}
                    </TooltipButton>
                </LoginDialogTrigger>
                <LoginDialog />
            </LoginDialogContainer>
        );
    }
}
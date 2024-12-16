import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui";
import { useI18n, useUserSettings } from "@/hooks";
import { LogOut, UserRound } from "lucide-react";
import { LoginDialog, LoginDialogContainer, LoginDialogTrigger } from "./LoginDialog";

type UserMenuProps = {
    disabled?: boolean;
}

export const UserMenu = ({ disabled }: UserMenuProps) => {

    const { userName, logout } = useUserSettings();
    const { translate } = useI18n();

    const handleLogOut = () => {
        logout();
    }

    if (userName) {
        return (
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button variant="ghost" disabled={disabled}>
                        <UserRound />
                        <span className="hidden lg:inline">
                            {translate("UserMenu.Welcome").replace("{userName}", userName)}
                        </span>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56">
                    <DropdownMenuGroup className="block lg:hidden">
                        <DropdownMenuLabel>Hi, {userName}!</DropdownMenuLabel>
                        <DropdownMenuSeparator />
                    </DropdownMenuGroup>
                    <DropdownMenuGroup>
                        <DropdownMenuItem disabled={disabled} onClick={handleLogOut}>
                            <LogOut />
                            {translate("UserMenu.Logout")}
                        </DropdownMenuItem>
                    </DropdownMenuGroup>
                </DropdownMenuContent>
            </DropdownMenu>
        );
    } else {
        return (
            <LoginDialogContainer>
                <LoginDialogTrigger>
                    <Button variant="secondary" disabled={disabled}>
                        <UserRound />
                        {translate("UserMenu.Login")}
                    </Button>
                </LoginDialogTrigger>
                <LoginDialog />
            </LoginDialogContainer>
        );
    }
}
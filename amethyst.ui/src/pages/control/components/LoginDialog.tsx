import { Button, Dialog, DialogClose, DialogContent, DialogFooter, DialogHeader, DialogTitle, Input, Label } from "@/components/ui";
import { useI18n } from "@/hooks/I18nHook";
import { useUserSettings } from "@/hooks/UserSettings";
import { DialogDescription, DialogTrigger } from "@radix-ui/react-dialog";
import { ChangeEvent, PropsWithChildren, useEffect, useState } from "react";

export const LoginDialogContainer = ({ children }: PropsWithChildren) => {
    return (
        <Dialog>
            {children}
        </Dialog>
    );
}

export const LoginDialogTrigger = ({ children }: PropsWithChildren) => {
    return (
        <DialogTrigger asChild>
            { children }
        </DialogTrigger>
    );
}

export const LoginDialog = () => {

    const { translate } = useI18n();
    const [ userName, setUserName ] = useState("");

    const { userName: currentUserName, login } = useUserSettings();

    useEffect(() => {
        setUserName(currentUserName ?? "");
    }, [currentUserName]);

    const handleUserNameChange = (event: ChangeEvent<HTMLInputElement>) => {
        setUserName(event.target.value);
    }

    const handleLoginClicked = () => {
        login(userName);
    }

    return (
        <DialogContent>
            <form>
                <DialogHeader>
                    <DialogTitle>{translate("LoginDialog.Title")}</DialogTitle>
                    <DialogDescription>{translate("LoginDialog.Description")}</DialogDescription>
                </DialogHeader>
                <Label>{translate("LoginDialog.Name")}</Label>
                <Input value={userName} onChange={handleUserNameChange} />
                <DialogFooter>
                    <DialogClose asChild>
                        <Button
                            variant="outline"
                            className="mt-4"
                        >
                            {translate("LoginDialog.Cancel")}
                        </Button>
                    </DialogClose>
                    <DialogClose asChild>
                        <Button 
                            variant="default" 
                            type="submit"
                            className="mt-4" 
                            disabled={!userName}
                            onClick={handleLoginClicked}
                        >
                            {translate("LoginDialog.Login")}
                        </Button>
                    </DialogClose>
                </DialogFooter>
            </form>
    </DialogContent>
    );
}
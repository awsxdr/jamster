import { Button, Dialog, DialogClose, DialogContent, DialogFooter, DialogHeader, DialogTitle, Input, Label } from "@/components/ui";
import { useI18n } from "@/hooks/I18nHook";
import { useUserLogin } from "@/hooks";
import { DialogDescription, DialogTrigger } from "@radix-ui/react-dialog";
import { ChangeEvent, FormEvent, PropsWithChildren, useEffect, useState } from "react";

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

    const { userName: currentUserName, login } = useUserLogin();

    useEffect(() => {
        setUserName(currentUserName ?? "");
    }, [currentUserName]);

    const handleUserNameChange = (event: ChangeEvent<HTMLInputElement>) => {
        setUserName(event.target.value);
    }

    const handleLoginClicked = (e: FormEvent) => {
        e.preventDefault();
        login(userName);
    }

    return (
        <DialogContent>
            <form onSubmit={handleLoginClicked}>
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
                        >
                            {translate("LoginDialog.Login")}
                        </Button>
                    </DialogClose>
                </DialogFooter>
            </form>
    </DialogContent>
    );
}
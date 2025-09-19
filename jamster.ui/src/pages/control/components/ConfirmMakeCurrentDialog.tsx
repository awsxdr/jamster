import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/components/ui/alert-dialog";
import { PropsWithChildren } from "react";

type ConfirmMakeCurrentDialogProps = {
    onAccept?: () => void;
};

export const ConfirmMakeCurrentDialog = ({ onAccept, children }: PropsWithChildren<ConfirmMakeCurrentDialogProps>) => {
    return (
        <>
            <AlertDialog>
                <AlertDialogTrigger asChild>
                    {children}
                </AlertDialogTrigger>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Switch game?</AlertDialogTitle>
                        <AlertDialogDescription>
                            Are you sure you want to change the current game? This will change all displays to use this game.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction onClick={onAccept}>Switch</AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </>
    )
}
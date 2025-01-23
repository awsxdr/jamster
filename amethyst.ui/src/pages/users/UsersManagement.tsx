import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Separator } from "@/components/ui";
import { Download, Loader2, Trash, Upload } from "lucide-react";
import { useState } from "react";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { useI18n } from "@/hooks/I18nHook";
import { UserListContextProvider, useUserApi, useUserList } from "@/hooks";
import { UserTable } from "./components/UserTable";

export const UsersManagement = () => (
    <UserListContextProvider>
        <UsersManagementInternal />
    </UserListContextProvider>
)

const UsersManagementInternal = () => {
    const users = useUserList()
    const { downloadUsers, deleteUser } = useUserApi();
    const [ isPreparingExport, setIsPreparingExport ] = useState(false);

    const { translate } = useI18n({ prefix: "UsersManagement." });
    
    const [selectedUserNames, setSelectedUserNames] = useState<string[]>([]);

    const handleDeleteSelected = () => {
        selectedUserNames
            .map(rowId => users[parseInt(rowId)].userName)
            .forEach(userName => {
                deleteUser(userName);
            });
            setSelectedUserNames([]);
    }

    const handleDownloadClicked = () => {
        setIsPreparingExport(true);
        downloadUsers(selectedUserNames.map(rowId => users[parseInt(rowId)].userName))
            .finally(() => setIsPreparingExport(false));
    }

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <div className="flex w-full items-center mt-1 mb-2 pr-2">
                <div className="grow">
                    <MobileSidebarTrigger className="mx-5" />
                </div>
                <div className="flex gap-2">
                    <Button variant="secondary">
                        <Download />
                        { translate("Import")}
                    </Button>
                    <Button variant="secondary" disabled={isPreparingExport || selectedUserNames.length === 0} onClick={handleDownloadClicked}>
                        { isPreparingExport
                            ? <Loader2 className="animate-spin" />
                            : <Upload />
                        }
                        
                        { translate("Export")}
                    </Button>
                    <AlertDialog>
                        <AlertDialogTrigger asChild>
                            <Button variant="destructive" disabled={selectedUserNames.length === 0}>
                                <Trash />
                                { translate("DeleteUser") }
                            </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                            <AlertDialogHeader>
                                <AlertDialogTitle>{ translate("DeleteUserDialog.Title") }</AlertDialogTitle>
                                <AlertDialogDescription>{ translate("DeleteUserDialog.Description") }</AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                                <AlertDialogCancel>{ translate("DeleteUserDialog.Cancel") }</AlertDialogCancel>
                                <AlertDialogAction className={buttonVariants({ variant: "destructive" })} onClick={handleDeleteSelected}>
                                    { translate("DeleteUserDialog.Confirm") }
                                </AlertDialogAction>
                            </AlertDialogFooter>
                        </AlertDialogContent>
                    </AlertDialog>
                </div>
            </div>
            <Separator />
            <UserTable users={users} selectedUserNames={selectedUserNames} onSelectedUserNamesChanged={setSelectedUserNames} />
        </>
    );
}
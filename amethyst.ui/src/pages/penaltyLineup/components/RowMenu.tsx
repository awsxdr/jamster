import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { Bandage, NotebookPen } from "lucide-react";
import { PropsWithChildren } from "react";

type RowMenuProps = {
    disableNotes?: boolean;
    onInjuryAdded?: () => void;
}

export const RowMenu = ({ disableNotes, onInjuryAdded, children }: PropsWithChildren<RowMenuProps>) => {
    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                { children }
            </DropdownMenuTrigger>
            <DropdownMenuContent>
                <DropdownMenuItem onClick={onInjuryAdded}>
                    <Bandage />
                    Add injury
                </DropdownMenuItem>
                <DropdownMenuItem disabled={disableNotes}>
                    <NotebookPen />
                    Notes
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}


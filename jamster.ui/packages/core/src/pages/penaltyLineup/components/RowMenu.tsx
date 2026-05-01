import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { useI18n } from "@/hooks";
import { Ban, Bandage, NotebookPen } from "lucide-react";
import { PropsWithChildren } from "react";

type RowMenuProps = {
    injuryActive?: boolean;
    disableNotes?: boolean;
    onInjuryAdded?: () => void;
    onInjuryRemoved?: () => void;
}

export const RowMenu = ({ injuryActive, disableNotes, onInjuryAdded, onInjuryRemoved, children }: PropsWithChildren<RowMenuProps>) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.RowMenu." });

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                { children }
            </DropdownMenuTrigger>
            <DropdownMenuContent>
                { !injuryActive && (
                    <DropdownMenuItem onClick={onInjuryAdded}>
                        <Bandage />
                        { translate("AddInjury" ) }
                    </DropdownMenuItem>
                )}
                { injuryActive && (
                    <DropdownMenuItem onClick={onInjuryRemoved}>
                        <Ban />
                        { translate("RemoveInjury") }
                    </DropdownMenuItem>                    
                )}
                <DropdownMenuItem disabled={disableNotes}>
                    <NotebookPen />
                    { translate("Notes") }
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}


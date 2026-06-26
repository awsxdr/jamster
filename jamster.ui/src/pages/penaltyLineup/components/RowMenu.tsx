import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui";
import { useI18n } from "@/hooks";
import { Ban, Bandage, NotebookPen, Replace } from "lucide-react";
import { PropsWithChildren } from "react";

export type RowMenuProps = {
    inBox?: boolean;
    injuryActive?: boolean;
    disableNotes?: boolean;
    onInjuryAdded?: () => void;
    onInjuryRemoved?: () => void;
}

export const RowMenu = ({ inBox, injuryActive, disableNotes, onInjuryAdded, onInjuryRemoved, children }: PropsWithChildren<RowMenuProps>) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.RowMenu." });

    console.log("RowMenu", inBox);

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                { children }
            </DropdownMenuTrigger>
            <DropdownMenuContent>
                { inBox && (
                    <DropdownMenuItem>
                        <Replace />
                        { translate("Substitute") }
                    </DropdownMenuItem>
                )}
                { !injuryActive && (
                    <DropdownMenuItem onClick={onInjuryAdded}>
                        <Bandage />
                        { translate("AddInjury") }
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


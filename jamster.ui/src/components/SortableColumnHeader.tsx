import { Column } from "@tanstack/react-table";
import { Button } from "./ui";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import { switchex } from "@/utilities/switchex";

type SortableColumnHeaderProps<TColumn> = {
    column: Column<TColumn, string>;
    header: string;
}

export const SortableColumnHeader = <TColumn,>({ column, header }: SortableColumnHeaderProps<TColumn>) => {

    const handleClick = () => {
        column.toggleSorting(column.getIsSorted() === 'asc')
    }

    return (
        <Button variant="ghost" onClick={handleClick}>
            { header }
            { 
                switchex(column.getIsSorted())
                    .case('asc').then(<ArrowDown />)
                    .case('desc').then(<ArrowUp />)
                    .default(<ArrowUpDown />)
            }
        </Button>
    );
}

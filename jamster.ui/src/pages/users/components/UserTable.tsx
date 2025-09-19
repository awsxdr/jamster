import { SortableColumnHeader } from "@/components";
import { Checkbox, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui";
import { useI18n } from "@/hooks";
import { User } from "@/types";
import { CheckedState } from "@radix-ui/react-checkbox";
import { ColumnDef, flexRender, getCoreRowModel, getSortedRowModel, SortingState, useReactTable } from "@tanstack/react-table";
import { useEffect, useState } from "react";

type UserTableProps = {
    users: User[];
    selectedUserNames: string[];
    onSelectedUserNamesChanged: (selectedUserNames: string[]) => void;
}

export const UserTable = ({ users, selectedUserNames, onSelectedUserNamesChanged }: UserTableProps) => {
    const { translate } = useI18n({ prefix: "UsersManagement.UserTable." });
    const [sorting, setSorting] = useState<SortingState>([{desc: false, id: 'userName'}]);

    const columns: ColumnDef<User, string>[] = [
        {
            id: 'userName',
            accessorFn: u => u.userName,
            header: ({column})=> (<SortableColumnHeader column={column} header={translate("UserName")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell}) => cell.renderValue()
        },
    ];
    
    const table = useReactTable({
        data: users,
        columns,
        getCoreRowModel: getCoreRowModel(),
        onSortingChange: setSorting,
        getSortedRowModel: getSortedRowModel(),
        state: {
            sorting,
        },
    });

    useEffect(() => {
        table?.getRowModel().rows.forEach(row => row.toggleSelected(!!selectedUserNames?.find(name => name === row.id)));
    }, [table, selectedUserNames]);

    const handleCheckedChanged = (name: string) => (checkedState: CheckedState) => {
        if(!selectedUserNames || !onSelectedUserNamesChanged) {
            return;
        }

        if (checkedState === true) {
            onSelectedUserNamesChanged([...selectedUserNames, name]);
        } else {
            onSelectedUserNamesChanged(selectedUserNames.filter(n => n !== name));
        }
    }

    return (
        <Table>
            <TableHeader>
                {
                    table.getHeaderGroups().map(headerGroup => (
                        <TableRow key={headerGroup.id}>
                            <TableHead key="userCheck" className="text-right w-8">&nbsp;</TableHead>
                            {
                                headerGroup.headers.map(header => (
                                    <TableHead key={header.id}>
                                        {header.isPlaceholder
                                            ? null
                                            : flexRender(
                                                header.column.columnDef.header,
                                                header.getContext()
                                            )
                                        }
                                    </TableHead>
                                ))
                            }
                        </TableRow>
                    ))
                }
            </TableHeader>
            <TableBody>
                {
                    table.getRowModel().rows?.length 
                        ? (
                            table.getRowModel().rows.map(row => (
                                <TableRow key={row.id} data-state={row.getIsSelected() && "selected"}>
                                    <TableCell>
                                        <Checkbox checked={row.getIsSelected()} onCheckedChange={handleCheckedChanged(row.id)} />
                                    </TableCell>
                                    {row.getVisibleCells().map(cell => (
                                        <TableCell key={cell.id}>
                                            { flexRender(cell.column.columnDef.cell, cell.getContext()) }
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        )
                        : (
                            <TableRow>
                                <TableCell colSpan={columns.length} className='h-24 text-center'>
                                    { translate("NoResults") }
                                </TableCell>
                            </TableRow>
                        )
                }
            </TableBody>
        </Table>
    );
}
import { Column, ColumnDef, flexRender, getCoreRowModel, getSortedRowModel, SortingState, useReactTable } from '@tanstack/react-table';

import { Team } from "@/types";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { useI18n } from '@/hooks/I18nHook';
import { Button } from '@/components/ui/button';
import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { DateTime } from 'luxon';

type TeamTableProps = {
    teams: Team[];
}

type SortableColumnHeaderProps = {
    column: Column<Team, string>;
    header: string;
}

const SortableColumnHeader = ({ column, header }: SortableColumnHeaderProps) => {

    const handleClick = () => {
        column.toggleSorting(column.getIsSorted() === 'asc')
    }

    return (
        <Button variant="ghost" onClick={handleClick}>
            { header }
            { 
                column.getIsSorted() === 'asc' ? <ArrowUp />
                : column.getIsSorted() === 'desc' ? <ArrowDown />
                : <ArrowUpDown />
            }
        </Button>
    );
}

export const TeamTable = ({ teams }: TeamTableProps) => {

    const { translate } = useI18n();
    const [sorting, setSorting] = useState<SortingState>([]);

    const columns: ColumnDef<Team, string>[] = [
        {
            id: 'teamName',
            accessorFn: t => t.names["team"] || t.names["league"] || t.names["default"] || "Team",
            header: ({column})=> (<SortableColumnHeader column={column} header={translate("Team name")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/teams/${row.original.id}`}>{cell.renderValue()}</Link>)
        },
        {
            id: 'lastUpdated',
            accessorKey: 'lastUpdateTime',
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("Last updated")} />),
            sortingFn: 'datetime',
            cell: ({cell}) => (<span>{DateTime.fromISO(cell.getValue()).toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS, { locale: navigator.language })}</span>)
        },
    ];
    
    const table = useReactTable({
        data: teams,
        columns,
        getCoreRowModel: getCoreRowModel(),
        onSortingChange: setSorting,
        getSortedRowModel: getSortedRowModel(),
        state: {
            sorting,
        },
    });

    return (
        <Table>
            <TableHeader>
                {
                    table.getHeaderGroups().map(headerGroup => (
                        <TableRow key={headerGroup.id}>
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
                                { translate("No results") }
                            </TableCell>
                        </TableRow>
                    )
                }
            </TableBody>
        </Table>
    );
}
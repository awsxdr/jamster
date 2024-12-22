import { Column, ColumnDef, flexRender, getCoreRowModel, getSortedRowModel, SortingState, useReactTable } from '@tanstack/react-table';

import { Team } from "@/types";
import { Checkbox, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui';
import { useI18n } from '@/hooks/I18nHook';
import { Button } from '@/components/ui/button';
import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { DateTime } from 'luxon';
import { CheckedState } from '@radix-ui/react-checkbox';
import { useIsMobile } from '@/hooks';

type TeamTableProps = {
    teams: Team[];
    selectedTeamIds?: string[];
    onSelectedTeamIdsChanged?: (selectedTeamIds: string[]) => void;
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

export const TeamTable = ({ teams, selectedTeamIds, onSelectedTeamIdsChanged }: TeamTableProps) => {

    const { translate } = useI18n();
    const [sorting, setSorting] = useState<SortingState>([]);
    const isMobile = useIsMobile();

    const columns: ColumnDef<Team, string>[] = [
        {
            id: 'teamName',
            accessorFn: t => t.names["team"] ?? "",
            header: ({column})=> (<SortableColumnHeader column={column} header={translate("TeamTable.TeamName")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/teams/${row.original.id}`}>{cell.renderValue() || <span className='italic'>{ translate("TeamTable.NoName") }</span>}</Link>)
        },
        {
            id: 'leagueName',
            accessorFn: t => t.names["league"] ?? "",
            header: ({column})=> (<SortableColumnHeader column={column} header={translate("TeamTable.LeagueName")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/teams/${row.original.id}`}>{cell.renderValue()}</Link>)
        },
        ...(isMobile ? [] : [
        {
            id: 'lastUpdated',
            accessorKey: 'lastUpdateTime',
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("TeamTable.LastUpdated")} />),
            sortingFn: 'datetime',
            cell: ({cell}) => (<span>{DateTime.fromISO(cell.getValue()).toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS, { locale: navigator.language })}</span>)
        } as ColumnDef<Team, string>]),
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

    useEffect(() => {
        table?.getRowModel().rows.forEach(row => row.toggleSelected(!!selectedTeamIds?.find(id => id === row.id)));
    }, [table, selectedTeamIds]);

    const handleCheckedChanged = (id: string) => (checkedState: CheckedState) => {
        if(!selectedTeamIds || !onSelectedTeamIdsChanged) {
            return;
        }

        if (checkedState === true) {
            onSelectedTeamIdsChanged([...selectedTeamIds, id]);
        } else {
            onSelectedTeamIdsChanged(selectedTeamIds.filter(i => i !== id));
        }
    }

    return (
        <Table>
            <TableHeader>
                {
                    table.getHeaderGroups().map(headerGroup => (
                        <TableRow key={headerGroup.id}>
                            <TableHead key="teamCheck" className="text-right">&nbsp;</TableHead>
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
                                { translate("TeamTable.NoResults") }
                            </TableCell>
                        </TableRow>
                    )
                }
            </TableBody>
        </Table>
    );
}
import { SortableColumnHeader } from "@/components/SortableColumnHeader";
import { Checkbox, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui"
import { GameStateContextProvider, useGameApi, useI18n } from "@/hooks";
import { GameInfo, GameStageState, Stage, StringMap, TeamDetailsState, TeamScoreState } from "@/types"
import { CheckedState } from "@radix-ui/react-checkbox";
import { ColumnDef, flexRender, getCoreRowModel, getSortedRowModel, Row, SortingState, useReactTable } from "@tanstack/react-table"
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

type GameTableRowProps = {
    row: Row<GameInfo>;
    onCheckedChange?: (id: string, checked: CheckedState) => void;
}

const GameTableRow = ({ row, onCheckedChange }: GameTableRowProps) => {
    return (
        <TableRow key={row.id} data-state={row.getIsSelected() && "selected"}>
            <TableCell>
                <Checkbox checked={row.getIsSelected()} onCheckedChange={checked => onCheckedChange?.(row.id, checked)} />
            </TableCell>
            { row.getVisibleCells().map(cell => (
                <TableCell key={cell.id}>
                    { flexRender(cell.column.columnDef.cell, cell.getContext()) }
                </TableCell>
            ))}
        </TableRow>
    );
}

type GameItem = {
    id: string;
    name: string;
    homeTeam: string;
    awayTeam: string;
    status: string;
}

type GameTableProps = {
    games: GameInfo[];
}

export const GameTable = ({ games }: GameTableProps) => {

    const [sorting, setSorting] = useState<SortingState>([]);
    const { translate } = useI18n();

    const { getGameState } = useGameApi();

    const [gameItems, setGameItems] = useState<GameItem[]>([]);

    const getTeamName = (names: StringMap<string>) => names["team"] ?? names["league"] ?? "";

    const getStageName = (stage: Stage, homeTeamScore: number, awayTeamScore: number) =>
        stage === Stage.BeforeGame ? "Upcoming"
        : stage === Stage.AfterGame ? `Finished (${homeTeamScore} - ${awayTeamScore})`
        : `In progress (${homeTeamScore} - ${awayTeamScore})`

    useEffect(() => {
        Promise.all(games.map(async (game): Promise<GameItem> => {

            const homeTeamScore = await getGameState<TeamScoreState>(game.id, "TeamScoreState_Home");
            const awayTeamScore = await getGameState<TeamScoreState>(game.id, "TeamScoreState_Away");
            const stage = await getGameState<GameStageState>(game.id, "GameStageState");
            const stageName = getStageName(stage.stage, homeTeamScore.score, awayTeamScore.score);

            return {
                id: game.id,
                name: game.name,
                homeTeam: getTeamName((await getGameState<TeamDetailsState>(game.id, "TeamDetailsState_Home")).team.names),
                awayTeam: getTeamName((await getGameState<TeamDetailsState>(game.id, "TeamDetailsState_Away")).team.names),
                status: stageName
            };
        })).then(setGameItems);
    }, [games]);

    const columns: ColumnDef<GameItem, string>[] = [
        {
            id: 'gameName',
            accessorFn: g => g.name,
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.GameName")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        },
        {
            id: 'homeTeam',
            accessorFn: g => g.homeTeam,
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.HomeTeam")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        },
        {
            id: 'awayTeam',
            accessorFn: g => g.awayTeam,
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.AwayTeam")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        },
        {
            id: 'stage',
            accessorFn: g => g.status,
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.Stage")} />),
            sortingFn: 'alphanumeric',
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        },
    ];

    const table = useReactTable({
        data: gameItems,
        columns,
        getCoreRowModel: getCoreRowModel(),
        onSortingChange: setSorting,
        getSortedRowModel: getSortedRowModel(),
        state: {
            sorting
        },
    });

    return (
        <Table>
            <TableHeader>
                {
                    table.getHeaderGroups().map(headerGroup => (
                        <TableRow key={headerGroup.id}>
                            <TableHead key="gameCheck" className="text-right"></TableHead>
                            {
                                headerGroup.headers.map(header => (
                                    <TableHead key={header.id}>
                                        { header.isPlaceholder || flexRender(header.column.columnDef.header, header.getContext()) }
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
                            <GameStateContextProvider gameId={row.id}>
                                <GameTableRow row={row} />
                            </GameStateContextProvider>
                        ))
                    ) : (
                        <TableRow>
                            <TableCell colSpan={columns.length} className="h-24 text-center italic">
                                { translate("GameTable.NoGames") }
                            </TableCell>
                        </TableRow>
                    )
                }
            </TableBody>
        </Table>
    )
}
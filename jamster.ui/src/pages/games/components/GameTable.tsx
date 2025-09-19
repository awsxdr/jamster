import { SortableColumnHeader } from "@/components/SortableColumnHeader";
import { Checkbox, Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui"
import { useGameApi, useI18n } from "@/hooks";
import { GameInfo, GameStageState, Stage, StringMap, TeamDetailsState, TeamScoreState } from "@/types"
import { switchex } from "@/utilities/switchex";
import { CheckedState } from "@radix-ui/react-checkbox";
import { ColumnDef, createColumnHelper, flexRender, getCoreRowModel, getSortedRowModel, SortingState, useReactTable } from "@tanstack/react-table"
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

type StageItem = {
    homeScore: number;
    awayScore: number;
    stage: string;
}

type GameItem = {
    id: string;
    name: string;
    homeTeam: string;
    awayTeam: string;
    stage: StageItem;
}

type GameTableProps = {
    games: GameInfo[];
    selectedGameIds: string[];
    onSelectedGameIdsChanged: (selectedTeamIds: string[]) => void;
}

export const GameTable = ({ games, selectedGameIds, onSelectedGameIdsChanged }: GameTableProps) => {

    const [sorting, setSorting] = useState<SortingState>([]);
    const { translate } = useI18n();

    const { getGameState } = useGameApi();

    const [gameItems, setGameItems] = useState<GameItem[]>([]);

    const getTeamName = (names: StringMap<string>) => names["team"] ?? names["league"] ?? "";

    const getStageName = ({ stage: { stage, homeScore, awayScore }}: GameItem) =>
        switchex(stage)
            .case("0").then(translate("GameTable.Status.Upcoming"))
            .case("2").then(translate("GameTable.Status.Finished").replace("{homeScore}", homeScore.toString()).replace("{awayScore}", awayScore.toString()))
            .default(translate("GameTable.Status.InProgress").replace("{homeScore}", homeScore.toString()).replace("{awayScore}", awayScore.toString()));

    const getStageValue = (stage: Stage) => 
        switchex(stage)
            .case(Stage.BeforeGame).then(0)
            .case(Stage.AfterGame).then(2)
            .default(1);

    useEffect(() => {
        Promise.all(games.map(async (game): Promise<GameItem> => {

            const homeTeamScore = await getGameState<TeamScoreState>(game.id, "TeamScoreState_Home");
            const awayTeamScore = await getGameState<TeamScoreState>(game.id, "TeamScoreState_Away");
            const stage = await getGameState<GameStageState>(game.id, "GameStageState");

            return {
                id: game.id,
                name: game.name,
                homeTeam: getTeamName((await getGameState<TeamDetailsState>(game.id, "TeamDetailsState_Home")).team.names),
                awayTeam: getTeamName((await getGameState<TeamDetailsState>(game.id, "TeamDetailsState_Away")).team.names),
                stage: { 
                    stage: getStageValue(stage.stage).toString(), 
                    homeScore: homeTeamScore.score, 
                    awayScore: awayTeamScore.score 
                },
            };
        })).then(setGameItems);
    }, [games]);

    const handleCheckedChanged = (id: string) => (checkedState: CheckedState) => {
        if (checkedState === true) {
            onSelectedGameIdsChanged([...selectedGameIds, id]);
        } else {
            onSelectedGameIdsChanged(selectedGameIds.filter(i => i !== id));
        }
    }

    const columnHelper = createColumnHelper<GameItem>();

    const columns: ColumnDef<GameItem, string>[] = [
        columnHelper.display({
            id: 'selected',
            cell: ({row}) => (<Checkbox checked={row.getIsSelected()} onCheckedChange={handleCheckedChanged(row.id)} />),
            
        }),
        columnHelper.accessor('name', {
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.GameName")} />),
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>)
        }),
        columnHelper.accessor('homeTeam', {
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.HomeTeam")} />),
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        }),
        columnHelper.accessor('awayTeam', {
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.AwayTeam")} />),
            cell: ({cell, row}) => (<Link to={`/games/${row.original.id}`}>{cell.renderValue()}</Link>),
        }),
        columnHelper.accessor('stage.stage', {
            header: ({column}) => (<SortableColumnHeader column={column} header={translate("GameTable.Stage")} />),
            cell: ({row}) => (<Link to={`/games/${row.original.id}`}>{getStageName(row.original)}</Link>),
        }),
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

    useEffect(() => {
        table?.getColumn('stage_stage')?.toggleSorting(false);
    }, []);

    useEffect(() => {
        table?.getRowModel().rows.forEach(row => row.toggleSelected(!!selectedGameIds?.find(id => id === row.id)));
    }, [table, selectedGameIds]);

    return (
        <Table>
            <TableHeader>
                {
                    table.getHeaderGroups().map(headerGroup => (
                        <TableRow key={headerGroup.id}>
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
                                <TableRow key={row.id} data-state={row.getIsSelected() && "selected"}>
                                    { row.getVisibleCells().map(cell => (
                                        <TableCell key={cell.id}>
                                            { flexRender(cell.column.columnDef.cell, cell.getContext()) }
                                        </TableCell>
                                    ))}
                                </TableRow>
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
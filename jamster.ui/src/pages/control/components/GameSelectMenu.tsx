import { useEffect, useState } from "react";
import { Select, SelectContent, SelectGroup, SelectItem, SelectLabel, SelectTrigger } from "@/components/ui";
import { GameInfo, GameStageState, Stage } from "@/types";
import { useGameApi, useI18n } from "@/hooks";
import { SelectValue } from "@radix-ui/react-select";
import { switchex } from "@/utilities/switchex";

enum GameGroup {
    Current,
    Running,
    Upcoming,
    Finished,
}

type GameListItem = {
    value: string;
    text: string;
    group: GameGroup;
}

type GameSelectMenuGroupProps = {
    items: GameListItem[];
    group: GameGroup;
    title: string;
}

const GameSelectMenuGroup = ({ items, group, title }: GameSelectMenuGroupProps) => {

    const filteredItems = items.filter(i => i.group === group);

    return (
        filteredItems.length > 0
            ? (
                <SelectGroup className="max-w-[90vw]">
                    <SelectLabel>{title}</SelectLabel>
                    { filteredItems.map(i => <SelectItem key={i.value} value={i.value} className="text-wrap w-full">{i.text}</SelectItem>) }
                </SelectGroup>
            ) : (
                <></>
            )
    );
}

type GameSelectMenuProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    selectedGameId?: string;
    disabled?: boolean;
    onSelectedGameIdChanged: (gameId: string) => void;
};

export const GameSelectMenu = ({ games, currentGame, selectedGameId, disabled, onSelectedGameIdChanged }: GameSelectMenuProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.GameSelectMenu." });
    const { getGameState } = useGameApi();

    const getGameGroup = async (id: string) => {
        if(id === currentGame?.id) {
            return GameGroup.Current;
        }

        const state = await getGameState<GameStageState>(id, "GameStageState")
        
        return (
            switchex(state.stage)
                .case(Stage.BeforeGame).then(GameGroup.Upcoming)
                .case(Stage.AfterGame).then(GameGroup.Finished)
                .default(GameGroup.Running)
        );
    }

    const [items, setItems] = useState<GameListItem[]>([]);

    useEffect(() => {
        (async () => {
            const listItems = await Promise.all(games.map(async game => ({ 
                value: game.id, 
                text: game.name,
                group: await getGameGroup(game.id),
            })));

            setItems(listItems);
        })();

    }, [games, currentGame]);

    return (
        <div className="flex justify-center w-3xl max-w-[100vw]">
            <Select
                value={selectedGameId}
                disabled={disabled}
                onValueChange={onSelectedGameIdChanged}
            >
                <SelectTrigger id="ScoreboardControl.GameSelectMenu">
                    <SelectValue placeholder={translate("SelectGame")} className="overflow-hidden text-ellipsis" />
                </SelectTrigger>
                <SelectContent className="w-150 max-w-[90vw]">
                    <GameSelectMenuGroup items={items} group={GameGroup.Current} title={translate("Current")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Running} title={translate("Running")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Upcoming} title={translate("Upcoming")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Finished} title={translate("Finished")} />
                </SelectContent>
            </Select>
        </div>
    );
}
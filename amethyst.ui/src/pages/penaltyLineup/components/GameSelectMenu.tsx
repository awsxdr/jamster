import { useEffect, useState } from "react";
import { Select, SelectContent, SelectGroup, SelectItem, SelectLabel, SelectTrigger } from "@/components/ui";
import { GameInfo, GameStageState, Stage } from "@/types";
import { useGameApi, useI18n } from "@/hooks";
import { SelectValue } from "@radix-ui/react-select";

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
            <SelectGroup>
                <SelectLabel>{title}</SelectLabel>
                { filteredItems.map(i => <SelectItem key={i.value} value={i.value} className="text-wrap max-w-full">{i.text}</SelectItem>) }
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
            state.stage === Stage.BeforeGame ? GameGroup.Upcoming
            : state.stage === Stage.AfterGame ? GameGroup.Finished
            : GameGroup.Running
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
        <div className="flex grow justify-center">
            <Select
                value={selectedGameId}
                disabled={disabled}
                onValueChange={onSelectedGameIdChanged}
            >
                <SelectTrigger className="max-w-xl">
                    <SelectValue placeholder={translate("SelectGame")} />
                </SelectTrigger>
                <SelectContent className="max-w-xl">
                    <GameSelectMenuGroup items={items} group={GameGroup.Current} title={translate("Current")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Running} title={translate("Running")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Upcoming} title={translate("Upcoming")} />
                    <GameSelectMenuGroup items={items} group={GameGroup.Finished} title={translate("Finished")} />
                </SelectContent>
            </Select>
        </div>
    );
}
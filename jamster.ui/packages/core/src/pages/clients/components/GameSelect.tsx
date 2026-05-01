import { useEffect, useState } from "react";
import { Label, Select, SelectContent, SelectGroup, SelectItem, SelectLabel, SelectTrigger } from "@/components/ui";
import { GameStageState, Stage } from "@/types";
import { useCurrentGame, useGameApi, useGamesList, useI18n } from "@/hooks";
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

type GameSelectGroupProps = {
    items: GameListItem[];
    group: GameGroup;
    title: string;
}

const GameSelectGroup = ({ items, group, title }: GameSelectGroupProps) => {

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

type GameSelectProps = {
    selectedGameId: string;
    disabled?: boolean;
    onSelectedGameIdChanged: (gameId: string) => void;
};

export const GameSelect = ({ selectedGameId, disabled, onSelectedGameIdChanged }: GameSelectProps) => {

    const { translate } = useI18n({ prefix: "Clients.GameSelect." });
    const { getGameState } = useGameApi();
    const { currentGame } = useCurrentGame();
    const games = useGamesList();

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
        <div className="flex flex-col gap-1">
            <Label>{translate("Label")}</Label>
            <Select
                value={selectedGameId}
                disabled={disabled}
                onValueChange={onSelectedGameIdChanged}
            >
                <SelectTrigger>
                    <SelectValue className="overflow-hidden text-ellipsis" />
                </SelectTrigger>
                <SelectContent className="w-150 max-w-[90vw]">
                    <SelectItem value="current">{translate("FollowCurrent")}</SelectItem>
                    <GameSelectGroup items={items} group={GameGroup.Current} title={translate("Current")} />
                    <GameSelectGroup items={items} group={GameGroup.Running} title={translate("Running")} />
                    <GameSelectGroup items={items} group={GameGroup.Upcoming} title={translate("Upcoming")} />
                    <GameSelectGroup items={items} group={GameGroup.Finished} title={translate("Finished")} />
                </SelectContent>
            </Select>
        </div>
    );
}
import { ComboBox } from "@/components/ui/combobox";
import { GameInfo } from "@/types";
import { useMemo } from "react";

type GameSelectMenuProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    selectedGameId?: string;
    onSelectedGameIdChanged: (gameId: string) => void;
};

export const GameSelectMenu = ({ games, currentGame, selectedGameId, onSelectedGameIdChanged }: GameSelectMenuProps) => {

    console.log("GameSelectMenu", currentGame);

    const items = useMemo(() => 
        games.map(game => ({ value: game.id, text: `${game.name}${(game.id === currentGame?.id ? ' (Current)' : '')}`})),
        [games, currentGame]);

    return (
        <>
            <ComboBox 
                items={items}
                value={selectedGameId ?? ""}
                placeholder="Select game..."
                onValueChanged={onSelectedGameIdChanged}
            />
        </>
    );
}
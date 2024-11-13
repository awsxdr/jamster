import { ComboBox } from "@/components/ui/combobox";
import { useI18n } from "@/hooks/I18nHook";
import { GameInfo } from "@/types";
import { useMemo } from "react";

type GameSelectMenuProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    selectedGameId?: string;
    onSelectedGameIdChanged: (gameId: string) => void;
};

export const GameSelectMenu = ({ games, currentGame, selectedGameId, onSelectedGameIdChanged }: GameSelectMenuProps) => {

    const { translate, language } = useI18n();

    const items = useMemo(() => 
        games.map(game => ({ value: game.id, text: `${game.name}${(game.id === currentGame?.id ? ` (${translate('Current')})` : '')}`})),
        [games, currentGame, language]);

    return (
        <>
            <ComboBox 
                items={items}
                value={selectedGameId ?? ""}
                placeholder={translate("Select game...")}
                onValueChanged={onSelectedGameIdChanged}
                className="sm:w-[200px] md:w-[250px] lg:w-[400px]"
                dropdownClassName="sm:w-[200px] md:w-[250px] lg:w-[400px]"
            />
        </>
    );
}
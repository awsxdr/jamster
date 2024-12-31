import { ComboBox } from "@/components/ui/combobox";
import { useI18n } from "@/hooks/I18nHook";
import { GameInfo } from "@/types";
import { useMemo } from "react";

type GameSelectMenuProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    selectedGameId?: string;
    disabled?: boolean;
    onSelectedGameIdChanged: (gameId: string) => void;
};

export const GameSelectMenu = ({ games, currentGame, selectedGameId, disabled, onSelectedGameIdChanged }: GameSelectMenuProps) => {

    const { translate, language } = useI18n();

    const items = useMemo(() => 
        games.map(game => ({ value: game.id, text: `${game.name}${(game.id === currentGame?.id ? ` (${translate('GameSelectMenu.Current')})` : '')}`})),
        [games, currentGame, language]);

    return (
        <div className="flex grow align-center text-center">
            <ComboBox 
                items={items}
                value={selectedGameId ?? ""}
                placeholder={translate("GameSelectMenu.SelectGame")}
                onValueChanged={onSelectedGameIdChanged}
                className="grow w-0"
                dropdownClassName="w-[200px] md:w-[250px] lg:w-[400px] xl:w-[600px]"
                disabled={disabled}
            />
        </div>
    );
}
import { useI18n, useTeamList } from "@/hooks";
import { Team } from "@/types";
import { useEffect, useMemo } from "react";
import { ComboBox, Label } from "./ui";
import { ColorBlock } from "./ColorBlock";
import { cn } from "@/lib/utils";

type TeamSelectProps = {
    titleKey: string;
    teamId?: string;
    exceptIds?: string[];
    colorIndex?: number;
    onTeamIdChanged?: (teamId: string) => void;
    onColorIndexChanged?: (colorIndex: number) => void;
}

export const TeamSelect = ({ titleKey, teamId, exceptIds, colorIndex, onTeamIdChanged, onColorIndexChanged }: TeamSelectProps) => {
    const { translate } = useI18n();

    const teams = useTeamList();
    
    const getDisplayName = (team: Team) =>
        team.names["team"] || team.names["league"] || team.names["default"] || "";

    const teamItems = useMemo(() =>
        teams.map(team => ({
            value: team.id,
            text: 
                !!team.names["league"] && !!team.names["team"] ? `${team.names["team"]} (${team.names["league"]})`
                : getDisplayName(team),
        }))
        .filter(team => !(exceptIds?.find(eid => team.value === eid))),
        [teams, exceptIds]);

    const teamColors = useMemo(() => {
        const team = teams.find(team => team.id === teamId);

        if(!team) {
            return [];
        }

        return Object.keys(team.colors)
            .map((key, i) => ({
                value: i.toString(),
                text: key,
                foreground: team.colors[key]?.complementaryColor ?? '#ffffff',
                background: team.colors[key]?.shirtColor ?? '#000000',
            }));
    }, [teams, teamId]);

    useEffect(
        () => onColorIndexChanged?.(0),
        [teamColors, onColorIndexChanged]);

    const handleColorIndexChanged = (key: string) => {
        const index = parseInt(key);
        if(!Number.isNaN(index)) {
            onColorIndexChanged?.(index);
        }
    }
    
    return (
        <>
            <Label className="font-bold text-lg">{translate(titleKey)}</Label>
            <ComboBox 
                items={teamItems} 
                value={teamId ?? ""} 
                placeholder={translate("NewGameDialog.SelectTeam")} 
                onValueChanged={onTeamIdChanged ?? (() => {})}
                dropdownClassName="w-[460px]"
            />
            <span className={cn(
                "flex gap-2 items-baseline transition-all duration-500 h-0 overflow-hidden",
                teamColors.length > 1 ? "h-auto" : ""
            )}>
                <span>Kit</span>
                <ComboBox
                    items={teamColors}
                    value={colorIndex?.toString() ?? ""}
                    placeholder={translate("NewGameDialog.SelectKit")}
                    onValueChanged={handleColorIndexChanged}
                    dropdownClassName="w-[460px]"
                    className="grow"
                    disabled={!teamId || !teamColors}
                    hideSearch
                />
                <ColorBlock color={teamColors[colorIndex ?? 0]?.background ?? '#000000'} />
                <ColorBlock color={teamColors[colorIndex ?? 0]?.foreground ?? '#000000'} />
            </span>
        </>
    )
}

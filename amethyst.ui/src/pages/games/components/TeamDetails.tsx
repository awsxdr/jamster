import { Color, GameSkater, GameTeam } from "@/types";
import { GameRosterTable } from "./GameRosterTable";
import { Button, Card, CardContent, Collapsible, CollapsibleContent, CollapsibleTrigger, Input, Label, Separator } from "@/components/ui";
import { ColorSelectButton } from "@/pages/teams/components/ColorSelectButton";
import { ChangeEvent, FocusEvent, PropsWithChildren, useMemo, useState } from "react";
import { ChevronsDown, ChevronsUp } from "lucide-react";
import { GameRosterInput } from "./GameRosterInput";
import { useI18n } from "@/hooks";

type SectionProps = {
    header?: string;
}

const Section = ({ header, children }: PropsWithChildren<SectionProps>) => {

    const [open, setOpen] = useState(true);

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <CollapsibleTrigger asChild>
                <Button variant="ghost" className="w-full flex">
                    <div className="grow">{header}</div>
                    { open ? <ChevronsUp /> : <ChevronsDown /> }
                </Button>
            </CollapsibleTrigger>
            <CollapsibleContent className="flex flex-col gap-2">
                { children }
            </CollapsibleContent>
        </Collapsible>
    );
}

type TeamDetailsProps = {
    team: GameTeam;
    className?: string;
    onTeamChanged?: (team: GameTeam) => void;
}

export const TeamDetails = ({ team, className, onTeamChanged }: TeamDetailsProps) => {

    const [teamName, setTeamName] = useState(team.names['team'] ?? '');
    const [leagueName, setLeagueName] = useState(team.names['league'] ?? '');
    const [scoreboardName, setScoreboardName] = useState(team.names['scoreboard'] ?? '');
    const [overlayName, setOverlayName] = useState(team.names['overlay'] ?? '');
    const [colorName, setColorName] = useState(team.names['color'] ?? '');
    const [shirtColor, setShirtColor] = useState(Color.rgbToHsl(Color.parseRgb(team.color.shirtColor) ?? { red: 0, green: 0, blue: 0 }));
    const [complementaryColor, setComplementaryColor] = useState(Color.rgbToHsl(Color.parseRgb(team.color.complementaryColor) ?? { red: 1, green: 1, blue: 1 }));
    const roster = useMemo(() => [...team.roster].sort((a, b) => a.number.localeCompare(b.number)), [team]);

    const { translate } = useI18n();

    const setName = (nameKey: string, name: string) => {
        onTeamChanged?.({
            ...team,
            names: {
                ...team.names,
                [nameKey]: name,
            },
        });
    }

    const handleNameBlur = (e: FocusEvent<HTMLInputElement>, nameKey: string) => {
        const value = e.target.value;

        if(team.names[nameKey] === value) {
            return;
        }

        setName(nameKey, value);
    }

    const handleNameChange = (setFunc: (value: string) => void) => (e: ChangeEvent<HTMLInputElement>) => {
        setFunc(e.target.value);
    }

    const handleShirtColorClose = () => {
        onTeamChanged?.({
            ...team,
            color: {
                ...team.color,
                shirtColor: Color.rgbToString(Color.hslToRgb(shirtColor)),
            }
        });
    }

    const handleComplementaryColorClose = () => {
        onTeamChanged?.({
            ...team,
            color: {
                ...team.color,
                complementaryColor: Color.rgbToString(Color.hslToRgb(complementaryColor)),
            }
        });
    }

    const handleSkatersAdded = (skaters: GameSkater[]) => {
        onTeamChanged?.({
            ...team,
            roster: [
                ...team.roster,
                ...skaters
            ],
        });
    }

    const handleSkaterChanged = (rosterIndex: number, skater: GameSkater) => {
        onTeamChanged?.({
            ...team,
            roster: roster.map((s, i) => i === rosterIndex ? skater : s)
        });
    }

    const handleSkaterDeleted = (rosterIndex: number) => {
        onTeamChanged?.({
            ...team,
            roster: roster.filter((_, i) => i !== rosterIndex),
        });
    }

    return (
        <Card className={className}>
            <CardContent className="pt-4 flex flex-col gap-4">
                <Section header={translate("TeamDetails.TeamSection")}>
                    <Label>{translate("TeamDetails.TeamName")}</Label>
                    <Input value={teamName} onChange={handleNameChange(setTeamName)} onBlur={e => handleNameBlur(e, "team")} />
                    <Label>{translate("TeamDetails.LeagueName")}</Label>
                    <Input value={leagueName} onChange={handleNameChange(setLeagueName)} onBlur={e => handleNameBlur(e, "league")} />
                    <Label>{translate("TeamDetails.ScoreboardName")}</Label>
                    <Input value={scoreboardName} placeholder={teamName || leagueName || colorName} onChange={handleNameChange(setScoreboardName)} onBlur={e => handleNameBlur(e, "scoreboard")} />
                    <Label>{translate("TeamDetails.OverlayName")}</Label>
                    <Input value={overlayName} placeholder={teamName || leagueName || colorName} onChange={handleNameChange(setOverlayName)} onBlur={e => handleNameBlur(e, "overlay")} />
                </Section>
                <Separator />
                <Section header={translate("TeamDetails.ColorSection")}>
                    <div>
                        <Label>{translate("TeamDetails.ColorName")}</Label>
                        <Input value={colorName} onChange={handleNameChange(setColorName)} onBlur={e => handleNameBlur(e, "color")} />
                    </div>
                    <div className="w-full flex items-center">
                        <div className="grow md:grow-0 md:min-w-72 inline-block">{translate("TeamDetails.ShirtColor")}</div>
                        <ColorSelectButton color={shirtColor} onColorChanged={setShirtColor} onClose={handleShirtColorClose} />
                    </div>
                    <div className="w-full flex items-center">
                        <div className="grow md:grow-0 md:min-w-72 inline-block">{translate("TeamDetails.ComplementaryColor")}</div>
                        <ColorSelectButton color={complementaryColor} onColorChanged={setComplementaryColor} onClose={handleComplementaryColorClose} />
                    </div>
                </Section>
                <Separator />
                <Section header={translate("TeamDetails.SkatersSection")}>
                    <GameRosterInput existingNumbers={roster.map(s => s.number)} onSkatersAdded={handleSkatersAdded} />
                    <GameRosterTable roster={roster} onSkaterChanged={handleSkaterChanged} onSkaterDeleted={handleSkaterDeleted} />
                </Section>
            </CardContent>
        </Card>
    );
}
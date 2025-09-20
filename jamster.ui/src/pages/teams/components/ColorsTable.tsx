import { ChangeEvent, MouseEvent, useEffect, useState } from "react";
import { Button, Card, CardContent, Checkbox, Form, FormControl, FormField, FormItem, FormMessage, Input } from "@/components/ui"
import { useI18n, useTeamApi, useTeamColorMap } from "@/hooks";
import { cn } from "@/lib/utils";
import { Color, HslColor, StringMap, Team, TeamColor } from "@/types";
import { Check, Pencil, Trash, X } from "lucide-react"
import { ColorSelectButton } from "@/components";
import { useColorInputSchema } from "./ColorInput";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

type ColorItem = TeamColor & {
    name: string;
    selected: boolean;
}

type ColorRowProps = {
    id?: string;
    color: ColorItem;
    disableEdit?: boolean;
    disableSelect?: boolean;
    existingColors: string[];
    onEditStart?: () => void;
    onEditEnd?: () => void;
    onSelectedChanged?: (selected: boolean) => void;
    onColorChanged?: (name: string, color: TeamColor) => void;
}

const ColorRow = ({ id, color, disableEdit, disableSelect, existingColors, onEditStart, onEditEnd, onSelectedChanged, onColorChanged }: ColorRowProps) => {
    const [isEditing, setIsEditing] = useState(false);

    const [shirtColor, setShirtColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 1 });
    const [complementaryColor, setComplementaryColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 1 });

    const formSchema = useColorInputSchema(existingColors);
    const colorMap = useTeamColorMap();

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: color.name,
        }
    });

    useEffect(() => {
        form.setFocus('name', { shouldSelect: true });
    }, [isEditing]);

    useEffect(() => {
        form.setValue('name', color.name);
        setShirtColor(Color.rgbToHsl(Color.parseRgb(color.shirtColor) ?? { red: 0, green: 0, blue: 0 }));
        setComplementaryColor(Color.rgbToHsl(Color.parseRgb(color.complementaryColor) ?? { red: 0, green: 0, blue: 0 }));
    }, [color]);

    const handleNameChanged = (e: ChangeEvent<HTMLInputElement>, baseOnChange: (...event: unknown[]) => void) => {
        baseOnChange(e);

        const kitColorName = e.target.value;

        const normalizedKitColorName = kitColorName.trim().toLowerCase();

        const predefinedKitColor = colorMap[normalizedKitColorName];

        if(!predefinedKitColor) {
            return;
        }

        const hslShirtColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.shirtColor) ?? { red: 0, green: 0, blue: 0});
        const hslComplementaryColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.complementaryColor) ?? { red: 0, green: 0, blue: 0});

        setShirtColor(hslShirtColor);
        setComplementaryColor(hslComplementaryColor);
    }

    const handleEditClick = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsEditing(true);
        onEditStart?.();
    }

    const handleAcceptEdit = ({ name }: { name: string }) => {
        setIsEditing(false);
        onEditEnd?.();
        onColorChanged?.(name, {
            shirtColor: Color.rgbToString(Color.hslToRgb(shirtColor)),
            complementaryColor: Color.rgbToString(Color.hslToRgb(complementaryColor)),
        });
        form.reset();
    }

    const handleRejectEdit = () => {
        setIsEditing(false);
        onEditEnd?.();
        setShirtColor(Color.rgbToHsl(Color.parseRgb(color.shirtColor) ?? { red: 0, green: 0, blue: 0 }));
        setComplementaryColor(Color.rgbToHsl(Color.parseRgb(color.complementaryColor) ?? { red: 0, green: 0, blue: 0 }));
        form.reset();
    }

    return (
        <div id={id} className={cn("flex w-full flex-nowrap gap-2")}>
            <Form {...form}>
                <form onSubmit={form.handleSubmit(handleAcceptEdit)} className="flex w-full flex-nowrap items-center gap-1">
                    <div className="flex w-8 text-nowrap justify-center">
                        <Checkbox checked={color.selected} onCheckedChange={onSelectedChanged} disabled={disableSelect} />
                    </div>
                    <div className="flex grow text-nowrap flex-nowrap gap-2 items-center">
                        { isEditing ? (
                            <FormField control={form.control} name="name" render={({field}) => (
                                <FormItem className="w-full">
                                    <FormControl>
                                        <Input {...field} onChange={e => handleNameChanged(e, field.onChange)} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                        ) : (
                            <span className="inline-block text-nowrap">{color.name}</span>
                        )}
                    </div>
                    <ColorSelectButton 
                        color={shirtColor} 
                        onColorChanged={setShirtColor}
                        className="disabled:opacity-100"
                        disabled={!isEditing} 
                    />
                    <ColorSelectButton 
                        color={complementaryColor} 
                        onColorChanged={setComplementaryColor}
                        className="disabled:opacity-100"
                        disabled={!isEditing} 
                    />
                    <div className="flex w-20 text-nowrap justify-end">
                        { isEditing ? (
                            <div className="flex gap-0.5">
                                <Button size="icon" variant="creative" type="submit">
                                    <Check />
                                </Button>
                                <Button size="icon" variant="destructive" onClick={handleRejectEdit}>
                                    <X />
                                </Button>
                            </div>
                        ) : (
                            <Button size="icon" variant="ghost" disabled={disableEdit} onClick={handleEditClick}>
                                <Pencil />
                            </Button>
                        )}
                    </div>
                </form>
            </Form>
        </div>
    );
}

type ColorsTableProps = {
    id?: string;
    team: Team;
}

export const ColorsTable = ({ id, team }: ColorsTableProps) => {

    const { translate } = useI18n();
    const [isEditing, setIsEditing] = useState(false);

    const { setTeam } = useTeamApi();

    const [colorItems, setColorItems] = useState<ColorItem[]>([]);

    useEffect(() => {
        const isDefined = (value: [string, TeamColor | undefined]): value is [string, TeamColor] => 
            value !== undefined;

        const colors: [string, TeamColor][] = Object.entries(team.colors).filter(isDefined);
        colors.sort(([a], [b]) => a.localeCompare(b));

        setColorItems(colors.map(([key, color]) => ({
            ...color,
            name: key,
            selected: false,
        })));
    }, [team]);

    const handleEditStart = () => {
        setIsEditing(true);
    }

    const handleEditEnd = () => {
        setIsEditing(false);
    }

    const selectedColors = colorItems.filter(i => i.selected);

    const handleDeleteSelected = () => {
        setTeam(team.id, {
            ...team,
            colors: colorItems.filter(i => !i.selected).reduce((o, i) => ({...o, [i.name]: i as TeamColor }), {} as StringMap<TeamColor>)
        });
    }

    const handleColorChanged = (index: number, name: string, color: TeamColor) => {
        setTeam(team.id, {
            ...team,
            colors: {
                ...colorItems.filter((_, i) => i !== index).reduce((o, i) => ({...o, [i.name]: i as TeamColor }), {} as StringMap<TeamColor>),
                [name]: color
            },
        });
    }

    return (
        <Card className="pt-4" id={id}>
            <CardContent className="flex flex-col gap-4">
                <div className="flex w-full justify-end">
                    <Button variant="destructive" disabled={selectedColors.length < 1 || selectedColors.length === colorItems.length} onClick={handleDeleteSelected}>
                        <Trash />
                        { translate("ColorsTable.DeleteColor") }
                    </Button>
                </div>
                { colorItems.map((color, i) => 
                    <ColorRow 
                        key={i}
                        id={id?.concat(`.${i}`)}
                        color={color} 
                        existingColors={Object.keys(team.colors).filter(c => c !== color.name)} 
                        disableEdit={isEditing}
                        disableSelect={isEditing}
                        onSelectedChanged={selected => setColorItems(colorItems.map((s, i2) => i2 == i ? { ...s, selected } : s))}
                        onEditStart={handleEditStart} 
                        onEditEnd={handleEditEnd}
                        onColorChanged={(name, color) => handleColorChanged(i, name, color)}
                    />) 
                }
            </CardContent>
        </Card>
    )
}
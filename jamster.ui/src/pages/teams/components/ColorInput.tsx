import { Button, Form, FormControl, FormField, FormItem, FormLabel, FormMessage, Input } from "@/components/ui";
import { useI18n, useTeamColorMap } from "@/hooks"
import { Color, HslColor, TeamColor } from "@/types";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ColorSelectButton } from "../../../components/ColorSelectButton";
import { Plus } from "lucide-react";
import { ChangeEvent, useMemo, useState } from "react";

export const useColorInputSchema = (existingColors: string[]) => {
    const { translate } = useI18n();

    const notExistingNameRegex = useMemo(() => new RegExp(`^(?!(${existingColors.join('|')})$).*$`), [existingColors]);

    return z.object({
        name: z.string()
            .min(1, {
                message: translate("ColorInput.NameRequired"),
            })
            .regex(notExistingNameRegex, {
                message: translate("ColorInput.NameExists"),
            }),
    });
}

type ColorInputProps = {
    existingColors: string[];
    onColorAdded?: (name: string, color: TeamColor) => void;
}

export const ColorInput = ({ existingColors, onColorAdded }: ColorInputProps) => {

    const { translate } = useI18n();

    const formSchema = useColorInputSchema(existingColors);
    const colorMap = useTeamColorMap();

    const [shirtColor, setShirtColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 0 });
    const [complementaryColor, setComplementaryColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 1 });

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: ''
        }
    });

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

    const handleSubmit = ({name}: { name: string }) => {
        form.setFocus('name');
        onColorAdded?.(name, { 
            shirtColor: Color.rgbToString(Color.hslToRgb(shirtColor)), 
            complementaryColor: Color.rgbToString(Color.hslToRgb(complementaryColor)),
        });
        setShirtColor({ hue: 0, saturation: 0, lightness: 0 });
        setComplementaryColor({ hue: 0, saturation: 0, lightness: 1 });
        form.reset();
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(handleSubmit)} className="flex w-full gap-2">
                <FormField control={form.control} name="name" render={({field}) => (
                    <FormItem className="grow">
                        <FormLabel>{translate("ColorInput.Name")}</FormLabel>
                        <FormControl className="grow">
                            <Input {...field} onChange={e => handleNameChanged(e, field.onChange)} />
                        </FormControl>
                        <FormMessage />
                    </FormItem>
                )} />
                <FormItem>
                    <div className="h-6"></div>
                    <ColorSelectButton color={shirtColor} onColorChanged={setShirtColor} />
                </FormItem>
                <FormItem>
                    <div className="h-6"></div>
                    <ColorSelectButton color={complementaryColor} onColorChanged={setComplementaryColor} />
                </FormItem>
                <FormItem>
                    <div className="h-6"></div>
                    <Button type="submit" variant="creative">
                        <Plus />
                        { translate("ColorInput.AddColor") }
                    </Button>
                </FormItem>
            </form>
        </Form>
    );
}
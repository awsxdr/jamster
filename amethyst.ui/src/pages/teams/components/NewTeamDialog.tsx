import { Button, Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Form, FormControl, FormField, FormItem, FormLabel, FormMessage, Input, TooltipProvider } from "@/components/ui";
import { Color, HslColor, TeamColor } from "@/types";
import { PropsWithChildren, useEffect, useState } from "react";
import { ColorSelectButton } from "@/components";
import { Loader2 } from "lucide-react";
import { useI18n } from "@/hooks/I18nHook";
import { DialogDescription } from "@radix-ui/react-dialog";
import { useTeamColorMap } from "@/hooks";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

type NewTeamDialogContainerProps = {
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
}

export const NewTeamDialogContainer = ({ children, ...props }: PropsWithChildren<NewTeamDialogContainerProps>) => {
    return (
        <Dialog {...props}>
            {children}
        </Dialog>
    );
}

export const NewTeamDialogTrigger = ({ children }: PropsWithChildren) => {
    return (
        <DialogTrigger asChild>
            {children}
        </DialogTrigger>
    );
}

type NewTeamCreated = (name: string, colorName: string, colors: TeamColor) => void;

const useNewTeamSchema = () => {
    const { translate } = useI18n();

    return z.object({
        teamName: z.string()
            .min(1, { message: translate("NewTeamDialog.TeamNameRequired") }),
            shirtColorName: z.string()
            .min(1, { message: translate("NewTeamDialog.KitColorRequired") }),
    });
}

type NewTeamDialogProps = {
    onNewTeamCreated?: NewTeamCreated;
    onCancelled?: () => void;
}

export const NewTeamDialog = ({ onNewTeamCreated, onCancelled }: NewTeamDialogProps) => {

    const { translate } = useI18n();
    const colorMap = useTeamColorMap();

    const [shirtColor, setShirtColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 0 });
    const [complementaryColor, setComplementaryColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 1 });
    const [isCreating, setIsCreating] = useState(false);

    const formSchema = useNewTeamSchema();

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            teamName: '',
            shirtColorName: '',
        },
    });

    const resetForm = () => {
        form.reset();
        form.clearErrors();
        setShirtColor({ hue: 0, saturation: 0, lightness: 0 });
        setComplementaryColor({ hue: 0, saturation: 0, lightness: 1 });
    }

    const handleSubmit = ({teamName, shirtColorName}: {teamName: string, shirtColorName: string}) => {
        setIsCreating(true);
        onNewTeamCreated?.(
            teamName, 
            shirtColorName, 
            { 
                shirtColor: Color.rgbToString(Color.hslToRgb(shirtColor)), 
                complementaryColor: Color.rgbToString(Color.hslToRgb(complementaryColor)),
            })
        resetForm();
        setIsCreating(false);
    }

    useEffect(() => {
        const normalizedKitColorName = form.getValues().shirtColorName.trim().toLowerCase();

        const predefinedKitColor = colorMap[normalizedKitColorName];

        if(!predefinedKitColor) {
            return;
        }

        const hslShirtColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.shirtColor) ?? { red: 0, green: 0, blue: 0});
        const hslComplementaryColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.complementaryColor) ?? { red: 0, green: 0, blue: 0});

        setShirtColor(hslShirtColor);
        setComplementaryColor(hslComplementaryColor);
    }, [form.watch('shirtColorName')]);

    const handleCancelClicked = () => {
        resetForm();
        onCancelled?.();
    }

    return (
        <TooltipProvider>
            <DialogContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(handleSubmit)}>
                        <DialogHeader>
                            <DialogTitle>{translate("NewTeamDialog.Title")}</DialogTitle>
                            <DialogDescription>{translate("NewTeamDialog.Description")}</DialogDescription>
                        </DialogHeader>
                        <div className="grid gap-4 py-4">
                            <FormField control={form.control} name="teamName" render={({field}) => (
                                <FormItem>
                                    <FormLabel>{translate("NewTeamDialog.TeamName")}</FormLabel>
                                    <FormControl>
                                        <Input {...field} disabled={isCreating} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                            <FormField control={form.control} name="shirtColorName" render={({field}) => (
                                <FormItem>
                                    <FormLabel>{translate("NewTeamDialog.KitColor")}</FormLabel>
                                    <FormControl>
                                        <div className="flex gap-2">
                                            <Input {...field} className="grow" disabled={isCreating} />
                                            <ColorSelectButton 
                                                color={shirtColor} 
                                                title={translate("NewTeamDialog.KitColorTitle")} 
                                                description={translate("NewTeamDialog.KitColorDescription")} 
                                                disabled={isCreating}
                                                onColorChanged={setShirtColor} 
                                            />
                                            <ColorSelectButton 
                                                color={complementaryColor} 
                                                title={translate("NewTeamDialog.ComplementaryColorTitle")}
                                                description={translate("NewTeamDialog.ComplementaryColorDescription")}
                                                disabled={isCreating}
                                                onColorChanged={setComplementaryColor} 
                                            />
                                        </div>
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                        </div>
                        <DialogFooter>
                            <div className="flex flex-row-reverse gap-2">
                                <FormItem>
                                    <Button 
                                        variant="creative" 
                                        type="submit"
                                        className="mt-4"
                                        disabled={isCreating}
                                    >
                                        { isCreating && <Loader2 className="animate-spin" /> }
                                        {translate("NewTeamDialog.Create")}
                                    </Button>
                                </FormItem>
                                <Button
                                    variant="outline"
                                    className="mt-4"
                                    disabled={isCreating}
                                    onClick={handleCancelClicked}
                                >
                                    {translate("NewTeamDialog.Cancel")}
                                </Button>
                            </div>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </TooltipProvider>
    )
}
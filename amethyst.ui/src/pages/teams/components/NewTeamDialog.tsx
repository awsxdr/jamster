import { Button, Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Input, Label, TooltipProvider } from "@/components/ui";
import { Color, HslColor, TeamColor } from "@/types";
import { ChangeEvent, PropsWithChildren, useEffect, useState } from "react";
import { ColorSelectButton } from "./ColorSelectButton";
import { Loader2 } from "lucide-react";
import { useI18n } from "@/hooks/I18nHook";
import { DialogDescription } from "@radix-ui/react-dialog";
import { useTeamColorMap } from "@/hooks";

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

type NewTeamDialogProps = {
    onNewTeamCreated?: NewTeamCreated;
    onCancelled?: () => void;
}

export const NewTeamDialog = ({ onNewTeamCreated, onCancelled }: NewTeamDialogProps) => {

    const { translate } = useI18n();
    const colorMap = useTeamColorMap();

    const [teamName, setTeamName] = useState("");
    const [shirtColor, setShirtColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 0 });
    const [complementaryColor, setComplementaryColor] = useState<HslColor>({ hue: 0, saturation: 0, lightness: 1 });
    const [kitColorName, setKitColorName] = useState("");
    const [isCreating, setIsCreating] = useState(false);

    const clearValues = () => {
        setTeamName('');
        setKitColorName('');
        setShirtColor({ hue: 0, saturation: 0, lightness: 0 });
        setComplementaryColor({ hue: 0, saturation: 0, lightness: 1 });
    }

    const handleTeamNameChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setTeamName(event.target.value);
    }

    const handleKitColorChanged = (event: ChangeEvent<HTMLInputElement>) => {
        setKitColorName(event.target.value);
    }

    useEffect(() => {

        const normalizedKitColorName = kitColorName.trim().toLowerCase();

        const predefinedKitColor = colorMap[normalizedKitColorName];

        if(!predefinedKitColor) {
            return;
        }

        const hslShirtColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.shirtColor) ?? { red: 0, green: 0, blue: 0});
        const hslComplementaryColor = Color.rgbToHsl(Color.parseRgb(predefinedKitColor.complementaryColor) ?? { red: 0, green: 0, blue: 0});

        setShirtColor(hslShirtColor);
        setComplementaryColor(hslComplementaryColor);
    }, [kitColorName]);

    const handleCreateClicked = () => {
        setIsCreating(true);
        onNewTeamCreated?.(
            teamName, 
            kitColorName, 
            {
                shirtColor: Color.rgbToString(Color.hslToRgb(shirtColor)), 
                complementaryColor: Color.rgbToString(Color.hslToRgb(complementaryColor))
            });
        clearValues();
    }

    const handleCancelClicked = () => {
        clearValues();
        onCancelled?.();
    }

    return (
        <TooltipProvider>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{translate("NewTeamDialog.Title")}</DialogTitle>
                    <DialogDescription>{translate("NewTeamDialog.Description")}</DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <Label>{translate("NewTeamDialog.TeamName")}</Label>
                    <Input value={teamName} onChange={handleTeamNameChanged} />
                    <Label>{translate("NewTeamDialog.KitColor")}</Label>
                    <div className="flex gap-2">
                        <Input className="grow" value={kitColorName} onChange={handleKitColorChanged} />
                        <ColorSelectButton 
                            color={shirtColor} 
                            title={translate("NewTeamDialog.KitColorTitle")} 
                            description={translate("NewTeamDialog.KitColorDescription")} 
                            onColorChanged={setShirtColor} 
                        />
                        <ColorSelectButton 
                            color={complementaryColor} 
                            title={translate("NewTeamDialog.ComplementaryColorTitle")}
                            description={translate("NewTeamDialog.ComplementaryColorDescription")} 
                            onColorChanged={setComplementaryColor} 
                        />
                    </div>
                </div>
                <DialogFooter>
                    <Button
                        variant="outline"
                        className="mt-4"
                        onClick={handleCancelClicked}
                    >
                        {translate("NewTeamDialog.Cancel")}
                    </Button>
                    <Button 
                        variant="default" 
                        type="submit"
                        className="mt-4"
                        disabled={isCreating || !teamName || !kitColorName}
                        onClick={handleCreateClicked}
                    >
                        { isCreating && <Loader2 className="animate-spin" /> }
                        {translate("NewTeamDialog.Create")}
                    </Button>
                    </DialogFooter>
            </DialogContent>
        </TooltipProvider>
    )
}
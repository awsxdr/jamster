import { useGameStageState } from "@/hooks"
import { useI18n } from "@/hooks/I18nHook";
import { Button } from "@/components/ui";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { IntermissionClock, JamClock, LineupClock, PeriodClock, TimeoutClock } from "./clocks";

export const ClocksContainer = () => {
    const gameStage = useGameStageState();
    const { translate } = useI18n();

    const [isEditing, setIsEditing] = useState(false);

    const handleEditClicked = () => {
        setIsEditing(v => !v);
    }

    return (
        <div className="w-full flex gap-2 items-center">
            <div className="w-full flex flex-wrap md:flex-nowrap gap-2 justify-between">
                <PeriodClock
                    editing={isEditing} 
                    name={`${translate('ClocksContainer.Period')} ${gameStage?.periodNumber ?? 0}`} 
                />
                <JamClock
                    editing={isEditing} 
                    name={`${translate('ClocksContainer.Jam')} ${gameStage?.jamNumber ?? 0}`} 
                />
                <LineupClock
                    editing={isEditing}
                    name={translate('ClocksContainer.Lineup')}
                />
                <TimeoutClock 
                    editing={isEditing} 
                    name={translate('ClocksContainer.Timeout')} 
                />
                <IntermissionClock
                    editing={isEditing} 
                    name={translate('ClocksContainer.Intermission')} 
                />
            </div>
            <Button 
                size="icon" 
                variant="ghost" 
                className={cn("border-2 border-transparent", isEditing && "border-primary")} 
                onClick={handleEditClicked}
            >
                <Pencil />
            </Button>
        </div>
    )
}
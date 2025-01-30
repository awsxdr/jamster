import { DisplaySide, TeamSide } from "@/types";
import { ScoreSheet } from "./ScoreSheet";
import { Button, Card, CardContent, CardHeader, Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui";
import { ArrowDown01, ArrowUp10, ChevronRight } from "lucide-react";
import { useState } from "react";
import { useI18n } from "@/hooks";
import { SCOREBOARD_TEXT_PADDING_CLASS_NAME } from "@/pages/scoreboard/Scoreboard";
import { cn } from "@/lib/utils";
import { TooltipButton } from "@/components/TooltipButton";

type ScoreSheetContainerProps = {
    gameId: string;
    displaySide: DisplaySide;
}

export const ScoreSheetContainer = ({ gameId, displaySide }: ScoreSheetContainerProps) => {

    const [open, setOpen] = useState(true);
    const [descending, setDescending] = useState(true);

    const { translate } = useI18n({ prefix: "ScoreSheetContainer." });

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <Card className="w-full">
                <CardHeader className={cn(SCOREBOARD_TEXT_PADDING_CLASS_NAME, "flex flex-row relative")}>
                    <div className="grow text-xl">
                        {translate("Header")}
                        { open &&
                            <TooltipButton variant="ghost" size="icon" description="Sort jams" onClick={() => setDescending(v => !v)}>
                                { descending ? <ArrowUp10 /> : <ArrowDown01 /> }
                            </TooltipButton>
                        }
                    </div>
                    <CollapsibleTrigger asChild>
                        <Button className="transition group/collapsible absolute top-0 right-1 md:top-1 md:right-2" variant="ghost" size="icon">
                            <ChevronRight className="transition duration-300 ease-in-out group-data-[state=open]/collapsible:rotate-90" />
                        </Button>
                    </CollapsibleTrigger>
                </CardHeader>
                <CollapsibleContent>
                    <CardContent className={cn(SCOREBOARD_TEXT_PADDING_CLASS_NAME, "pt-0 md:pt-0 lg:pt-0 xl:pt-0")}>
                        <div className="w-full flex flex-col xl:grid grid-flow-col auto-cols-fr gap-2" data-state->
                            { displaySide !== DisplaySide.Away && <ScoreSheet gameId={gameId} teamSide={TeamSide.Home} descending={descending} /> }
                            { displaySide !== DisplaySide.Home && <ScoreSheet gameId={gameId} teamSide={TeamSide.Away} descending={descending} /> }
                        </div>
                    </CardContent>
                </CollapsibleContent>
            </Card>
        </Collapsible>
    );
}
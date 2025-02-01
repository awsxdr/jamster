import { GameStateContextProvider, useGameApi, useI18n } from "@/hooks";
import { useNavigate, useParams } from "react-router-dom";
import { Button, Card, CardContent } from "@/components/ui";
import { MobileSidebarTrigger } from "@/components";
import { EditBreadcrumbs } from "./components/EditBreadcrumbs";
import { GameTeams } from "./components/GameTeams";
import { ArrowDownToLine } from "lucide-react";

export const GameEdit = () => {

    const { gameId } = useParams();
    const navigate = useNavigate();
    const { translate } = useI18n();
    const { downloadStatsbook } = useGameApi();

    if(!gameId) {
        navigate('/games');
        return (<></>);
    }

    return (
        <>
            <title>{translate("GameEdit.Title")} | {translate("Main.Title")}</title>
            <GameStateContextProvider gameId={gameId}>
                <>
                    <div className="flex items-center mt-2">
                        <MobileSidebarTrigger className="mx-5" />
                        <EditBreadcrumbs gameId={gameId} />
                    </div>
                    <Card className="m-0 sm:m-1 lg:m-2 xl:m-5">
                        <CardContent className="flex flex-col gap-1 lg:gap-2 xl:gap-5 p-1 lg:p-2 xl:p-5">
                            <div className="w-full text-right">
                                <Button variant="secondary" onClick={() => downloadStatsbook(gameId)}>
                                    <ArrowDownToLine />
                                    Export statsbook
                                </Button>
                            </div>
                            <div>
                                <GameTeams gameId={gameId} />
                            </div>
                        </CardContent>
                    </Card>
                </>
            </GameStateContextProvider>
        </>
    );
}
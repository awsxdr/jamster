import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from "@/components/ui";
import { useGameApi, useI18n } from "@/hooks";
import { GameInfo } from "@/types";
import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";

type EditBreadcrumbsProps = {
    gameId: string;
}

export const EditBreadcrumbs = ({ gameId }: EditBreadcrumbsProps) => {
    const { translate } = useI18n();
    const { getGame } = useGameApi();
    const [ gameInfo, setGameInfo ] = useState<GameInfo>();

    useEffect(() => {
        (async () => {
            const gameInfo = await getGame(gameId);
            setGameInfo(gameInfo);
        })();
    }, [gameId]);

    const displayName = useMemo(() => gameInfo?.name ?? "", [gameInfo]);

    return (
        <Breadcrumb className="mx-4">
            <BreadcrumbList>
                <BreadcrumbItem>
                    <BreadcrumbLink asChild>
                        <Link to="/games">{translate("GameEdit.Games")}</Link>
                    </BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator />
                <BreadcrumbItem>
                    <BreadcrumbPage>{ displayName }</BreadcrumbPage>
                </BreadcrumbItem>
            </BreadcrumbList>
        </Breadcrumb>
    )
}
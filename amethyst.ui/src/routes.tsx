import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { CurrentGameScoreboard } from "./pages/scoreboard";
import { MainMenu } from "./pages/menu";
import { TitlePage } from "./pages/title";
import { ScoreboardControl } from "./pages/control";
import { TeamsManagement } from "./pages/teams/TeamsManagement";
import { TeamEdit } from "./pages/teams/TeamEdit";
import { WorkInProgress } from "./pages/WorkInProgress/WorkInProgress";
import { GamesManagement } from "./pages/games/GamesManagement";
import { GameEdit } from "./pages/games/GameEdit";
import { Overlay } from "./pages/overlay/Overlay";
import { ScoreboardSettings } from "./pages/scoreboardSettings/ScoreboardSettings";

export const Routes = () => {
    const router = createBrowserRouter([
        {
            path: '/',
            element: <MainMenu content={<TitlePage />} />
        },
        {
            path: '/scoreboard',
            element: <CurrentGameScoreboard />
        },
        {
            path: '/control',
            element: <MainMenu content={<ScoreboardControl />} />
        },
        {
            path: '/teams',
            element: <MainMenu content={<TeamsManagement />} />,
        },
        {
            path: '/teams/:teamId',
            element: <MainMenu content={<TeamEdit />} />,
        },
        {
            path: '/games',
            element: <MainMenu content={<GamesManagement />} />
        },
        {
            path: '/games/:gameId',
            element: <MainMenu content={<GameEdit />} />
        },
        {
            path: '/overlay',
            element: <Overlay />
        },
        {
            path: '/stats',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/settings/display',
            element: <MainMenu content={<ScoreboardSettings />} />
        },
        {
            path: '/settings/overlay',
            element: <MainMenu content={<WorkInProgress />} />
        },
    ]);
    
    return (
        <RouterProvider router={router} />
    );
}
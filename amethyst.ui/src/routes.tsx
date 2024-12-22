import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { CurrentGameScoreboard } from "./pages/scoreboard";
import { MainMenu } from "./pages/menu";
import { TitlePage } from "./pages/title";
import { ScoreboardControl } from "./pages/control";
import { TeamsManagement } from "./pages/teams/TeamsManagement";
import { TeamEdit } from "./pages/teams/TeamEdit";
import { WorkInProgress } from "./pages/WorkInProgress/WorkInProgress";
import { GameManagement } from "./pages/games/GameManagement";

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
            element: <MainMenu content={<GameManagement />} />
        },
        {
            path: '/overlay',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/stats',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/settings/display',
            element: <MainMenu content={<WorkInProgress />} />
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
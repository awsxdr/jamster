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
import { OverlaySettings } from "./pages/overlaySettings/OverlaySettings";
import { Timeline } from "./pages/timeline/Timeline";
import { UsersManagement } from "./pages/users/UsersManagement";

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
            path: '/sbo',
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
            path: '/games/:gameId/timeline',
            element: <MainMenu content={<Timeline />} />
        },
        {
            path: '/help',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/overlay',
            element: <Overlay />
        },
        {
            path: '/penalties',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/plt',
            element: <MainMenu content={<WorkInProgress />} />
        },
        {
            path: '/rulesets',
            element: <MainMenu content={<WorkInProgress />} />
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
            element: <MainMenu content={<OverlaySettings />} />
        },
        {
            path: '/users',
            element: <MainMenu content={<UsersManagement />} />
        },
    ]);
    
    return (
        <RouterProvider router={router} />
    );
}
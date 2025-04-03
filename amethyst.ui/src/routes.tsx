import { RouterProvider } from "react-router-dom";
import { CurrentGameScoreboard } from "./pages/scoreboard";
import { MainMenu } from "./pages/menu";
import { TitlePage } from "./pages/title";
import { ScoreboardControl } from "./pages/control";
import { TeamsManagement, TeamEdit } from "./pages/teams";
import { WorkInProgress } from "./pages/WorkInProgress/WorkInProgress";
import { GamesManagement } from "./pages/games/GamesManagement";
import { GameEdit } from "./pages/games/GameEdit";
import { Overlay } from "./pages/overlay/Overlay";
import { Settings } from "./pages/settings";
import { Timeline } from "./pages/timeline/Timeline";
import { UsersManagement } from "./pages/users/UsersManagement";
import { PenaltyLineup } from "./pages/penaltyLineup";
import { createClientActivityRouter } from "./hooks";
import { ClientActivity } from "./types";
import { PenaltyWhiteboard } from "./pages/penaltyWhiteboard";

export const Routes = () => {
    const router = createClientActivityRouter([
        {
            path: '/',
            element: <MainMenu content={<TitlePage />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/scoreboard',
            element: <CurrentGameScoreboard />,
            activity: ClientActivity.Scoreboard,
        },
        {
            path: '/sbo',
            element: <MainMenu content={<ScoreboardControl />} />,
            activity: ClientActivity.ScoreboardOperator,
        },
        {
            path: '/teams',
            element: <MainMenu content={<TeamsManagement />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/teams/:teamId',
            element: <MainMenu content={<TeamEdit />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/games',
            element: <MainMenu content={<GamesManagement />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/games/:gameId',
            element: <MainMenu content={<GameEdit />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/games/:gameId/rules',
            element: <MainMenu content={<WorkInProgress />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/games/:gameId/timeline',
            element: <MainMenu content={<Timeline />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/help',
            element: <MainMenu content={<WorkInProgress />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/overlay',
            element: <Overlay />,
            activity: ClientActivity.StreamOverlay,
        },
        {
            path: '/penalties',
            element: <PenaltyWhiteboard />,
            activity: ClientActivity.PenaltyWhiteboard,
        },
        {
            path: '/plt',
            element: <MainMenu content={<PenaltyLineup />} />,
            activity: ClientActivity.PenaltyLineupControl,
        },
        {
            path: '/rulesets',
            element: <MainMenu content={<WorkInProgress />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/stats',
            element: <MainMenu content={<WorkInProgress />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/settings',
            element: <MainMenu content={<Settings />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/users',
            element: <MainMenu content={<UsersManagement />} />,
            activity: ClientActivity.Other,
        },
    ]);
    
    return (
        <RouterProvider router={router} />
    );
}
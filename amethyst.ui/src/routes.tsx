import { RouterProvider } from "react-router-dom";
import { Clients, GameEdit, GamesManagement, MainMenu, Overlay, PenaltyLineup, PenaltyWhiteboard, Scoreboard, ScoreboardControl, Settings, TeamEdit, TeamsManagement, Timeline, TitlePage, UsersManagement } from "@/pages";
import { createClientActivityRouter } from "@/hooks";
import { ClientActivity } from "@/types";
import { WorkInProgress } from "./pages/WorkInProgress/WorkInProgress";

export const Routes = () => {
    const router = createClientActivityRouter([
        {
            path: '/',
            element: <MainMenu content={<TitlePage />} />,
            activity: ClientActivity.Other,
        },
        {
            path: '/scoreboard',
            element: <Scoreboard />,
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
        {
            path: '/clients',
            element: <MainMenu content={<Clients />} />,
            activity: ClientActivity.Other,
        },
    ]);
    
    return (
        <RouterProvider router={router} />
    );
}
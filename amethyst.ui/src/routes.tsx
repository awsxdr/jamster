import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { CurrentGameScoreboard } from "./pages/scoreboard";
import { MainMenu } from "./pages/menu";
import { TitlePage } from "./pages/title";
import { ScoreboardControl } from "./pages/control";

export const Routes = () => {
    const router = createBrowserRouter([
        {
            path: '/',
            element: <MainMenu content={<TitlePage />} />
        },
        {
            path: '/control',
            element: <MainMenu content={<ScoreboardControl />} />
        },
        {
            path: '/scoreboard',
            element: <CurrentGameScoreboard />
        },
    ]);
    
    return (
        <RouterProvider router={router} />
    );
}
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { CurrentGameScoreboard } from "./pages/scoreboard";

export const router = createBrowserRouter([
    {
        path: '/scoreboard',
        element: <CurrentGameScoreboard />
    }
]);

export const Routes = () => {
    return (
        <RouterProvider router={router} />
    );
}
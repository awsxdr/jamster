import { GamesListContextProvider, SystemStateContextProvider } from "@/hooks";
import { Routes } from "./routes";
import { TeamListContextProvider } from "./hooks/TeamsHook";

const App = () => {
  return (
    <>
      <SystemStateContextProvider>
        <GamesListContextProvider>
          <TeamListContextProvider>
            <Routes />
          </TeamListContextProvider>
        </GamesListContextProvider>
      </SystemStateContextProvider>
    </>
  )
}

export default App

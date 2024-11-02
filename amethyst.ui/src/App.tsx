import { GamesListContextProvider, SystemStateContextProvider } from "@/hooks";
import { Routes } from "./routes";

const App = () => {
  return (
    <>
      <SystemStateContextProvider>
        <GamesListContextProvider>
          <Routes />
        </GamesListContextProvider>
      </SystemStateContextProvider>
    </>
  )
}

export default App

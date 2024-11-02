import { CurrentGameScoreboard } from "./pages/scoreboard";
import { SystemStateContextProvider } from "@/hooks";

const App = () => {
  return (
    <>
      <SystemStateContextProvider>
        <CurrentGameScoreboard />
      </SystemStateContextProvider>
    </>
  )
}

export default App

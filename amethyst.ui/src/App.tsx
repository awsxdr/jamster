import { GameStateContextProvider } from "@hooks/StateHook";
import { Scoreboard } from "./pages/Scoreboard";

const App = () => {

  return (
    <>
      <GameStateContextProvider gameId="51e24679-1bc5-4852-bf1f-313661e9b020">
        <Scoreboard />
      </GameStateContextProvider>
    </>
  )
}

export default App

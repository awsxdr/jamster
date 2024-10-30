import { GameStateContextProvider } from "@hooks/StateHook";
import { Scoreboard } from "./pages/Scoreboard";
import { SystemStateContextProvider, useSystemState } from "./hooks";

const CurrentGameScoreboard = () => {

  const currentGame = useSystemState().useCurrentGame();

  return (
    <GameStateContextProvider gameId={currentGame}>
      <Scoreboard />
    </GameStateContextProvider>
  );
};

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

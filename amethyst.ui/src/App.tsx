import { SystemStateContextProvider } from "@/hooks";
import { Routes } from "./routes";

const App = () => {
  return (
    <>
      <SystemStateContextProvider>
        <Routes />
      </SystemStateContextProvider>
    </>
  )
}

export default App

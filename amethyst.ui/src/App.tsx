import { ConfigurationContextProvider, GamesListContextProvider, SystemStateContextProvider, UserLoginContextProvider, UserSettingsContextProvider } from "@/hooks";
import { Routes } from "./routes";
import { TeamListContextProvider } from "./hooks/TeamsHook";
import { ShortcutsContextProvider } from "./hooks/InputControls";

const App = () => {
  return (
    <>
      <SystemStateContextProvider>
        <GamesListContextProvider>
          <TeamListContextProvider>
            <ConfigurationContextProvider>
              <UserLoginContextProvider>
                <UserSettingsContextProvider>
                  <ShortcutsContextProvider>
                    <Routes />
                  </ShortcutsContextProvider>
                </UserSettingsContextProvider>
              </UserLoginContextProvider>
            </ConfigurationContextProvider>
          </TeamListContextProvider>
        </GamesListContextProvider>
      </SystemStateContextProvider>
    </>
  )
}

export default App

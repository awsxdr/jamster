import { Routes } from "./routes";
import {
    ClientConnectionContextProvider,
    ConfigurationContextProvider, 
    GamesListContextProvider, 
    SystemStateContextProvider, 
    UserLoginContextProvider, 
    UserSettingsContextProvider, 
    TeamListContextProvider, 
    ShortcutsContextProvider 
} from "@awsxdr/jamster.ui.shared";

const App = () => {
    return (
        <>
            <ClientConnectionContextProvider>
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
            </ClientConnectionContextProvider>
        </>
    )
}

export default App

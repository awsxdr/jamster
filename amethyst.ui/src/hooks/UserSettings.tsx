import { createContext, PropsWithChildren, useContext, useState } from "react";

export enum DisplaySide {
    Home = 'Home',
    Away = 'Away',
    Both = 'Both',
}

type UserSettingsProviderState = {
    showClockControls: boolean;
    setShowClockControls: (value: boolean) => void;
    showScoreControls: boolean;
    setShowScoreControls: (value: boolean) => void;
    showStatsControls: boolean;
    setShowStatsControls: (value: boolean) => void;
    showLineupControls: boolean;
    setShowLineupControls: (value: boolean) => void;
    showClocks: boolean;
    setShowClocks: (value: boolean) => void;
    displaySide: DisplaySide;
    setDisplaySide: (value: DisplaySide) => void;
}

const initialState: UserSettingsProviderState = {
    showClockControls: true,
    showScoreControls: true,
    showStatsControls: true,
    showLineupControls: true,
    showClocks: true,
    displaySide: DisplaySide.Both,
    setShowClockControls: () => {},
    setShowScoreControls: () => {},
    setShowStatsControls: () => {},
    setShowLineupControls: () => {},
    setShowClocks: () => {},
    setDisplaySide: () => {},
}

const UserSettingsProviderContext = createContext<UserSettingsProviderState>(initialState);

export const UserSettingsProvider = ({ children }: PropsWithChildren) => {

    const [showClockControls, setShowClockControls] = useState(initialState.showClockControls);
    const [showScoreControls, setShowScoreControls] = useState(initialState.showScoreControls);
    const [showStatsControls, setShowStatsControls] = useState(initialState.showStatsControls);
    const [showLineupControls, setShowLineupControls] = useState(initialState.showLineupControls);
    const [showClocks, setShowClocks] = useState(initialState.showClocks);
    const [displaySide, setDisplaySide] = useState(initialState.displaySide);

    return (
        <UserSettingsProviderContext.Provider value={{
            showClockControls, setShowClockControls,
            showScoreControls, setShowScoreControls,
            showStatsControls, setShowStatsControls,
            showLineupControls, setShowLineupControls,
            showClocks, setShowClocks,
            displaySide, setDisplaySide,
        }}>
            {children}
        </UserSettingsProviderContext.Provider>
    );
};

export const useUserSettings = () => {
    const context = useContext(UserSettingsProviderContext);

    if (context === undefined) {
        throw new Error('useUserSettings must be used inside a UserSettingsProvider');
    }

    return context;
}
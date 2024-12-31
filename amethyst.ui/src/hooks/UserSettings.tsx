import { StringMap } from "@/types";
import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react";

export enum DisplaySide {
    Home = 'Home',
    Away = 'Away',
    Both = 'Both',
}

export type UserSettings = {
    showClockControls: boolean;
    showScoreControls: boolean;
    showStatsControls: boolean;
    showLineupControls: boolean;
    showClocks: boolean;
    showTimeoutList: boolean;
    displaySide: DisplaySide;
    shortcuts: StringMap<string>;
}

type SettingsMap = StringMap<UserSettings>;

const DEFAULT_SETTINGS: UserSettings = {
    showClockControls: true,
    showScoreControls: true,
    showStatsControls: true,
    showLineupControls: true,
    showClocks: true,
    showTimeoutList: true,
    displaySide: DisplaySide.Both,
    shortcuts: {},
}

type UserSettingsProviderState = {
    userSettings: UserSettings;
    userName?: string;
    setUserSettings: (settingsFactory: (currentSettings: UserSettings) => UserSettings) => void;
    login: (userName: string) => void;
    logout: () => void;
}

const DEFAULT_STATE: UserSettingsProviderState = {
    userSettings: DEFAULT_SETTINGS,
    setUserSettings: () => {},
    login: () => {},
    logout: () => {},
}

const getStoredSettings = (): SettingsMap => {
    const settingsJson = localStorage.getItem('amethyst.userSettings');

    if(!settingsJson) {
        return { [""]: DEFAULT_SETTINGS };
    }

    const settings = JSON.parse(settingsJson) as SettingsMap;

    if (!settings) {
        return { [""]: DEFAULT_SETTINGS };
    }

    return settings;
}

const UserSettingsProviderContext = createContext<UserSettingsProviderState>(DEFAULT_STATE);

export const UserSettingsProvider = ({ children }: PropsWithChildren) => {

    const [userName, setUserName] = useState<string>();
    const [settings, setSettings] = useState<SettingsMap>();

    const userSettings = useMemo(() => settings?.[userName ?? ""] ?? DEFAULT_SETTINGS, [settings, userName]);

    const setUserSettings = (settingsFactory: (currentSettings: UserSettings) => UserSettings) => {
        setSettings(current => ({ ...current, [userName ?? ""]: settingsFactory(current?.[userName ?? ""] ?? DEFAULT_SETTINGS) }));
    }

    const login = (userName: string) => {
        setUserName(userName);
    }

    const logout = () => {
        setUserName(undefined);
    }

    useEffect(() => {
        setSettings(getStoredSettings());
    }, []);

    useEffect(() => {
        if(!settings) {
            return;
        }

        localStorage.setItem(
            'amethyst.userSettings',
             JSON.stringify(settings));
    }, [settings]);

    return (
        <UserSettingsProviderContext.Provider value={{ userSettings, userName, setUserSettings, login, logout }}>
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
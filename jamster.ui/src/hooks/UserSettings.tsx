import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { useConfiguration, useHubConnection, userApi } from ".";
import { useUserLogin } from "./UserLogin";
import * as uuid from 'uuid';

type CallbackHandle = string;
type UserConfigurationChanged<TConfiguration> = (value: TConfiguration) => void;

type UserSettingsProviderState = {
    watchUserConfiguration: <TConfiguration,>(userName: string, configurationName: string, onChanged: UserConfigurationChanged<TConfiguration>) => CallbackHandle;
    unwatchUserConfiguration: (userName: string, configurationName: string, handle: CallbackHandle) => void;
}

const DEFAULT_STATE: UserSettingsProviderState = {
    watchUserConfiguration: () => { throw new Error('watchUserConfiguration called before context created'); },
    unwatchUserConfiguration: () => { throw new Error('unwatchUserConfiguration called before context created'); },
}

const UserSettingsContext = createContext<UserSettingsProviderState>(DEFAULT_STATE);

export const useCurrentUserConfiguration = <TConfiguration,>(configurationName: string, defaultValue: TConfiguration) => {
    const context = useContext(UserSettingsContext);
    const [value, setValue] = useState<TConfiguration>(defaultValue);
    const [isConfigurationLoaded, setIsConfigurationLoaded] = useState(false);
    const { userName } = useUserLogin();
    const { configuration: baseConfiguration, setConfiguration: setBaseConfiguration } = useConfiguration<TConfiguration>(configurationName);

    const safeBaseConfiguration = useMemo(() => baseConfiguration ?? defaultValue, [baseConfiguration, defaultValue]);

    if (context === undefined) {
        throw new Error('useUserSettings must be used inside a UserSettingsProvider');
    }

    useEffect(() => {
        (async () => {
            const initialValue = await userApi.getConfiguration<TConfiguration>(userName, configurationName);
            setIsConfigurationLoaded(true);
            setValue(initialValue);
        })();
    }, [userName]);

    useEffect(() => {
        if(!userName) {
            return;
        }

        const handle = context.watchUserConfiguration<TConfiguration>(userName, configurationName, setValue);

        return () => context.unwatchUserConfiguration(userName, configurationName, handle);
    }, [userName, configurationName]);

    const setConfiguration = useCallback((configuration: TConfiguration) => {
        userApi.setConfiguration(userName, configurationName, configuration);
    }, [userName, configurationName]);

    return {
        configuration: userName ? value : safeBaseConfiguration,
        isConfigurationLoaded,
        setConfiguration: userName ? setConfiguration : setBaseConfiguration,
    };
}

type ConfigurationNotifier = Record<CallbackHandle, (genericConfiguration: object) => void>;
type ConfigurationNotifierMap = Record<string, ConfigurationNotifier>;
type UserConfigurationNotifierMap = Record<string, ConfigurationNotifierMap>;

export const UserSettingsContextProvider = ({ children }: PropsWithChildren) => {
    const userConfigurationNotifiersRef = useRef<UserConfigurationNotifierMap>({});

    const handleConnectionDisconnect = async () => {
        if(!connection) {
            return;
        }

        Object.keys(userConfigurationNotifiersRef.current).forEach(userName => {
            Object.keys(userConfigurationNotifiersRef.current[userName]).forEach(configurationType => {
                connection.invoke("UnwatchUserConfiguration", userName, configurationType);
            });
        });
    }

    const { connection } = useHubConnection("users", handleConnectionDisconnect);
    const connectionRef = useRef(connection);
    connectionRef.current = connection;

    const watchUserConfiguration = useCallback(<TConfiguration,>(userName: string, configurationName: string, onChange: UserConfigurationChanged<TConfiguration>) => {
        const newId = uuid.v4();

        if (!userConfigurationNotifiersRef.current[userName]?.[configurationName]) {
            connectionRef.current?.invoke("WatchUserConfiguration", userName, configurationName);
        }

        userConfigurationNotifiersRef.current[userName] = {
            ...(userConfigurationNotifiersRef.current[userName] ?? {}),
            [configurationName]: {
                ...(userConfigurationNotifiersRef.current[userName]?.[configurationName] ?? {}),
                [newId]: genericConfiguration => onChange(genericConfiguration as TConfiguration),
            },
        };

        return newId;
    }, []);

    const unwatchUserConfiguration = useCallback((userName: string, configurationName: string, handle: CallbackHandle) => {
        if(!userConfigurationNotifiersRef.current[userName]?.[configurationName]?.[handle]) {
            console.warn("Attempt to unwatch user configuration with invalid handle", handle);
            return;
        }

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { [handle]: _, ...newNotifier } = userConfigurationNotifiersRef.current[userName][configurationName];

        userConfigurationNotifiersRef.current[userName] = {
            ...userConfigurationNotifiersRef.current[userName],
            [configurationName]: newNotifier
        }
    }, []);

    useEffect(() => {
        if (!connection) {
            return;
        }

        const registerWatchers = () =>
            Object.keys(userConfigurationNotifiersRef.current).forEach(userName => {
                Object.keys(userConfigurationNotifiersRef.current[userName]).forEach(configurationType => {
                    connection.invoke("WatchUserConfiguration", userName, configurationType);
                });
            });

        registerWatchers();
        connection.onreconnected(registerWatchers);
    }, [connection]);

    const notify = useCallback((userName: string, configurationName: string, configuration: object) => {
        Object.values(userConfigurationNotifiersRef.current[userName][configurationName])?.forEach(n => n(configuration));
    }, []);

    useEffect(() => {
        connection?.on("UserConfigurationChanged", notify);

        return () => connection?.off("UserConfigurationChanged", notify);
    }, [connection, notify]);

    const context = useMemo(
        () => ({ watchUserConfiguration, unwatchUserConfiguration }),
        [watchUserConfiguration, unwatchUserConfiguration]
    );

    return (
        <UserSettingsContext.Provider value={context}>
            {children}
        </UserSettingsContext.Provider>
    );
};

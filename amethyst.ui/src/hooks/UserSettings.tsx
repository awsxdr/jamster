import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useConfiguration, useHubConnection, useUserApi } from ".";
import { HubConnection } from "@microsoft/signalr";
import { useUserLogin } from "./UserLogin";
import * as uuid from 'uuid';

type CallbackHandle = string;
type UserConfigurationChanged<TConfiguration> = (value: TConfiguration) => void;

type UserSettingsProviderState = {
    watchUserConfiguration: <TConfiguration,>(userName: string, configurationName: string, onChanged: UserConfigurationChanged<TConfiguration>) => CallbackHandle;
    unwatchUserConfiguration: (userName: string, configurationName: string, handle: CallbackHandle) => void;
    connection?: HubConnection;
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
    const userApi = useUserApi();
    const { userName } = useUserLogin();
    const { configuration: baseConfiguration, setConfiguration: setBaseConfiguration } = useConfiguration<TConfiguration>(configurationName);

    const safeBaseConfiguration = useMemo(() => baseConfiguration ?? defaultValue, [baseConfiguration]);

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
    }, [userName, configurationName, userApi]);

    return {
        configuration: userName ? value : safeBaseConfiguration,
        isConfigurationLoaded,
        setConfiguration: userName ? setConfiguration : setBaseConfiguration,
    };
}

type ConfigurationNotifier = { [handle: CallbackHandle]: (genericConfiguration: object) => void };
type ConfigurationNotifierMap = { [configurationName: string]: ConfigurationNotifier };
type UserConfigurationNotifierMap = { [userName: string]: ConfigurationNotifierMap };

export const UserSettingsContextProvider = ({ children }: PropsWithChildren) => {
    const [notifiers, setNotifiers] = useState<UserConfigurationNotifierMap>({});

    const handleConnectionDisconnect = async () => {
        if(!connection) {
            return;
        }

        Object.keys(notifiers).forEach(userName => {
            Object.keys(notifiers[userName]).forEach(configurationType => {
                connection.invoke("UnwatchUserConfiguration", userName, configurationType);
            });
        });
    }

    const { connection } = useHubConnection("users", handleConnectionDisconnect);

    const watchUserConfiguration = <TConfiguration,>(userName: string, configurationName: string, onChange: UserConfigurationChanged<TConfiguration>) => {
        const newId = uuid.v4();

        setNotifiers(n => ({
            ...n,
            [userName]: {
                ...(n[userName] ?? {}),
                [configurationName]: {
                    ...(n[userName]?.[configurationName] ?? {}),
                    [newId]: genericConfiguration => onChange(genericConfiguration as TConfiguration)
                }
            }
        }));

        return newId;
    }

    const unwatchUserConfiguration = (userName: string, configurationName: string, handle: CallbackHandle) => {
        setNotifiers(n => {
            if(!n[userName]?.[configurationName]?.[handle]) {
                console.warn("Attempt to unwatch user configuration with invalid handle", handle);
            }

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [handle]: _, ...newNotifier } = n[userName][configurationName];

            return {
                ...n,
                [userName]: {
                    ...n[userName],
                    [configurationName]: newNotifier
                }
            }
        });
    }

    useEffect(() => {
        if (!connection) {
            return;
        }

        Object.keys(notifiers).forEach(userName => {
            Object.keys(notifiers[userName]).forEach(configurationType => {
                connection!.invoke("WatchUserConfiguration", userName, configurationType);
            });
        });
    }, [connection, notifiers]);

    const notify = useCallback((userName: string, configurationName: string, configuration: object) => {
        Object.values(notifiers[userName][configurationName])?.forEach(n => n(configuration));
    }, [notifiers]);

    useEffect(() => {
        connection?.on("UserConfigurationChanged", notify);

        return () => connection?.off("UserConfigurationChanged", notify);
    }, [connection, notify]);

    return (
        <UserSettingsContext.Provider value={{ watchUserConfiguration, unwatchUserConfiguration }}>
            {children}
        </UserSettingsContext.Provider>
    );
};

import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react";
import { HubConnection } from "@microsoft/signalr";
import { useConfigurationApi, useHubConnection } from ".";
import { v4 as uuidv4 } from 'uuid';

type ConfigurationContextProps = {
    watchConfiguration: <TConfiguration,>(configurationName: string, onChanged: ConfigurationChanged<TConfiguration>) => CallbackHandle;
    unwatchConfiguration: (configurationName: string, handle: CallbackHandle) => void;
    connection?: HubConnection;
}

const ConfigurationContext = createContext<ConfigurationContextProps>({
    watchConfiguration: () => { throw new Error('watchConfiguration called before context created'); },
    unwatchConfiguration: () => { throw new Error('unwatchConfiguration called before context created'); },
});

export const useConfiguration = <TConfiguration,>(configurationName: string) => {
    const context = useContext(ConfigurationContext);
    const [value, setValue] = useState<TConfiguration>();
    const [isConfigurationLoaded, setIsConfigurationLoaded] = useState(false);
    const { getConfiguration, setConfiguration } = useConfigurationApi();

    if (context === undefined) {
        throw new Error('useConfiguration must be used inside a UserSettingsProvider');
    }

    useEffect(() => {
        (async () => {
            const initialValue = await getConfiguration<TConfiguration>(configurationName);
            setIsConfigurationLoaded(true);
            setValue(initialValue);
        })();
    }, []);

    useEffect(() => {
        const handle = context.watchConfiguration<TConfiguration>(configurationName, setValue);

        return () => context.unwatchConfiguration(configurationName, handle);
    }, [configurationName, setValue]);

    const setCurrentConfiguration = useCallback((configuration: TConfiguration) => {
        setConfiguration(configurationName, configuration);
    }, [configurationName, setConfiguration]);

    return { 
        configuration: value,
        isConfigurationLoaded,
        setConfiguration: setCurrentConfiguration 
    };
}

type ConfigurationChanged<TConfiguration> = (value: TConfiguration) => void;

type CallbackHandle = string;
type ConfigurationNotifier = { [handle: CallbackHandle]: (genericConfiguration: object) => void };
type ConfigurationNotifierMap = { [key: string]: ConfigurationNotifier };

export const ConfigurationContextProvider = ({ children }: PropsWithChildren) => {
    const [notifiers, setNotifiers] = useState<ConfigurationNotifierMap>({});

    const handleConnectionDisconnect = async () => {
        if(!connection) return;

        Object.keys(notifiers).forEach(stateName => {
            connection.invoke("UnwatchConfiguration", stateName);
        });
    }

    const { connection } = useHubConnection("Configuration", handleConnectionDisconnect);

    const watchConfiguration = <TConfiguration,>(configurationName: string, onChange: ConfigurationChanged<TConfiguration>) => {
        const newId = uuidv4();

        setNotifiers(n => ({
            ...n,
            [configurationName]: {
                ...(n[configurationName] ?? {}),
                [newId]: genericConfiguration => onChange(genericConfiguration as TConfiguration)
            }
        }));

        return newId;
    }

    const unwatchConfiguration = (configurationName: string, handle: CallbackHandle) => {
        setNotifiers(n => {
            if(!n[configurationName]?.[handle]) {
                console.warn("Attempt to unwatch configuration with invalid handle", handle);
            }

            const { [handle]: _, ...newNotifier } = n[configurationName];

            return {
                ...n,
                [configurationName]: newNotifier,
            };
        });
    }

    useEffect(() => {
        if (!connection) {
            return;
        }

        Object.keys(notifiers).forEach(configurationName => {
            connection!.invoke("WatchConfiguration", configurationName);
        });
    }, [connection, notifiers]);

    const notify = useCallback((configurationName: string, configuration: object) => {
        Object.values(notifiers[configurationName])?.forEach(n => n(configuration));
    }, [notifiers]);

    useEffect(() => {
        connection?.on("ConfigurationChanged", notify);
    }, [connection, notify]);

    return (
        <ConfigurationContext.Provider value={{ watchConfiguration, unwatchConfiguration, connection }}>
            {children}
        </ConfigurationContext.Provider>
    )
}
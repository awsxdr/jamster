import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { configurationApi, useHubConnection } from ".";
import { v4 as uuidv4 } from 'uuid';

type ConfigurationContextProps = {
    watchConfiguration: <TConfiguration,>(configurationName: string, onChanged: ConfigurationChanged<TConfiguration>) => CallbackHandle;
    unwatchConfiguration: (configurationName: string, handle: CallbackHandle) => void;
}

const ConfigurationContext = createContext<ConfigurationContextProps>({
    watchConfiguration: () => { throw new Error('watchConfiguration called before context created'); },
    unwatchConfiguration: () => { throw new Error('unwatchConfiguration called before context created'); },
});

export const useConfiguration = <TConfiguration,>(configurationName: string) => {
    const context = useContext(ConfigurationContext);
    const [value, setValue] = useState<TConfiguration>();
    const [isConfigurationLoaded, setIsConfigurationLoaded] = useState(false);

    if (context === undefined) {
        throw new Error('useConfiguration must be used inside a UserSettingsProvider');
    }

    useEffect(() => {
        (async () => {
            const initialValue = await configurationApi.getConfiguration<TConfiguration>(configurationName);
            setIsConfigurationLoaded(true);
            setValue(initialValue);
        })();
    }, []);

    useEffect(() => {
        const handle = context.watchConfiguration<TConfiguration>(configurationName, setValue);
        return () => context.unwatchConfiguration(configurationName, handle);
    }, [configurationName, setValue, context.watchConfiguration, context.unwatchConfiguration]);

    const setCurrentConfiguration = useCallback((configuration: TConfiguration) => {
        configurationApi.setConfiguration(configurationName, configuration);
    }, [configurationName]);

    return { 
        configuration: value,
        isConfigurationLoaded,
        setConfiguration: setCurrentConfiguration 
    };
}

type ConfigurationChanged<TConfiguration> = (value: TConfiguration) => void;

type CallbackHandle = string;
type ConfigurationNotifier = Record<CallbackHandle, (genericConfiguration: object) => void>;
type ConfigurationNotifierMap = Record<string, ConfigurationNotifier>;

export const ConfigurationContextProvider = ({ children }: PropsWithChildren) => {
    const configurationNotifiersRef = useRef<ConfigurationNotifierMap>({});

    const handleConnectionDisconnect = async () => {
        if(!connection) return;

        Object.keys(configurationNotifiersRef.current).forEach(stateName => {
            connection.invoke("UnwatchConfiguration", stateName);
        });
    }

    const { connection } = useHubConnection("Configuration", handleConnectionDisconnect);
    const connectionRef = useRef(connection);
    connectionRef.current = connection;

    useEffect(() => {
        if (!connection) return;

        const registerWatchers = () =>
            Object.keys(configurationNotifiersRef.current).forEach(c => connection.invoke("WatchConfiguration", c));

        registerWatchers();
        connection.onreconnected(registerWatchers);
    }, [connection]);

    const watchConfiguration = useCallback(<TConfiguration,>(configurationName: string, onChange: ConfigurationChanged<TConfiguration>) => {
        const newId = uuidv4();

        if (!Object.keys(configurationNotifiersRef.current).includes(configurationName)) {
            connectionRef.current?.invoke("WatchConfiguration", configurationName);
        }

        configurationNotifiersRef.current[configurationName] = {
            ...(configurationNotifiersRef.current[configurationName] ?? {}),
            [newId]: genericConfiguration => onChange(genericConfiguration as TConfiguration)
        };

        return newId;
    }, []);

    const unwatchConfiguration = useCallback((configurationName: string, handle: CallbackHandle) => {
        if(!configurationNotifiersRef.current[configurationName]?.[handle]) {
            console.warn("Attempt to unwatch configuration with invalid handle", handle);
            return;
        }

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { [handle]: _, ...newNotifier } = configurationNotifiersRef.current[configurationName];

        configurationNotifiersRef.current[configurationName] = newNotifier;
    }, []);

    const notify = useCallback((configurationName: string, configuration: object) => {
        Object.values(configurationNotifiersRef.current[configurationName])?.forEach(n => n(configuration));
    }, []);

    useEffect(() => {
        connection?.on("ConfigurationChanged", notify);

        return () => connection?.off("ConfigurationChanged", notify);
    }, [connection, notify]);

    const context = useMemo(
        () => ({ watchConfiguration, unwatchConfiguration }),
        [watchConfiguration, unwatchConfiguration]
    );
    
    return (
        <ConfigurationContext.Provider value={context}>
            {children}
        </ConfigurationContext.Provider>
    )
}
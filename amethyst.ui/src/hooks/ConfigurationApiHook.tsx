import { API_URL } from "@/constants";

type ConfigurationApi = {
    getConfiguration: <TConfiguration,>(configurationName: string) => Promise<TConfiguration>;
    setConfiguration: <TConfiguration,>(configurationName: string, configuration: TConfiguration) => Promise<void>;
}

export const useConfigurationApi: () => ConfigurationApi = () => {
    const getConfiguration = async <TConfiguration,>(configurationName: string) => {
        const response = await fetch(`${API_URL}/api/configurations/${configurationName}`);
        return (await response.json()) as TConfiguration;
    }

    const setConfiguration = async <TConfiguration,>(configurationName: string, configuration: TConfiguration) => {
        await fetch(
            `${API_URL}/api/configurations/${configurationName}`,
            {
                method: 'POST',
                body: JSON.stringify(configuration),
                headers: {
                    "Content-Type": "application/json"
                },
            }
        );
    }

    return {
        getConfiguration,
        setConfiguration,
    };
}
import { API_URL } from "@/constants";
import { User, UserConfigurations } from "@/types";
import { useConfigurationApi } from "./ConfigurationApiHook";

type UserApi = {
    getUsers: () => Promise<User[]>;
    downloadUsers: (userNames: string[]) => Promise<void>;
    getUser: (userName: string) => Promise<UserConfigurations>;
    createUser: (userName: string) => Promise<void>;
    deleteUser: (userName: string) => Promise<void>;
    getConfiguration: <TConfiguration,>(userName: string | undefined, configurationName: string) => Promise<TConfiguration>;
    setConfiguration: <TConfiguration,>(userName: string | undefined, configurationName: string, configuration: TConfiguration) => Promise<void>;
}

export const useUserApi: () => UserApi = () => {

    const { getConfiguration: getBaseConfiguration, setConfiguration: setBaseConfiguration } = useConfigurationApi();

    const getUsers = async () => {
        const response = await fetch(`${API_URL}/api/users`);
        return (await response.json()) as User[];
    }

    const downloadUsers = async (userNames: string[]) => {
        const requestUrl = `${API_URL}/api/users/file?${userNames.map(u => `userNames=${encodeURIComponent(u)}`).join("&")}`;
        const response = await fetch(requestUrl);
        const blobUrl = URL.createObjectURL(await response.blob());

        const downloadLink = document.createElement('a');
        downloadLink.href = blobUrl;
        downloadLink.download = "amethyst-user-export.json";
        document.body.appendChild(downloadLink);
        downloadLink.click();
        downloadLink.remove();

        URL.revokeObjectURL(blobUrl);
    }

    const getUser = async (userName: string) => {
        const response = await fetch(`${API_URL}/api/users/${encodeURIComponent(userName)}`);
        return (await response.json()) as UserConfigurations;
    }

    const createUser = async (userName: string) => {
        await fetch(
            `${API_URL}/api/users`,
            {
                method: 'POST',
                body: JSON.stringify({ userName }),
                headers: {
                    "Content-Type": "application/json"
                },
            }
        );
    }

    const deleteUser = async (userName: string) => {
        await fetch(
            `${API_URL}/api/users/${encodeURIComponent(userName)}`,
            {
                method: "DELETE"
            }
        );
    }

    const getConfiguration = async <TConfiguration,>(userName: string | undefined, configurationName: string) => {

        if(!userName) {
            return await getBaseConfiguration<TConfiguration>(configurationName);
        }

        const response = await fetch(`${API_URL}/api/users/${encodeURIComponent(userName)}/configuration/${configurationName}`);
        return (await response.json()) as TConfiguration;
    }

    const setConfiguration = async <TConfiguration,>(userName: string | undefined, configurationName: string, configuration: TConfiguration) => {

        if(!userName) {
            await setBaseConfiguration(configurationName, configuration);
            return;
        }

        await fetch(
            `${API_URL}/api/users/${encodeURIComponent(userName)}/configuration/${configurationName}`,
            {
                method: 'PUT',
                body: JSON.stringify(configuration),
                headers: {
                    "Content-Type": "application/json"
                },
            }
        );
    }

    return {
        getUsers,
        downloadUsers,
        getUser,
        createUser,
        deleteUser,
        getConfiguration,
        setConfiguration,
    };
}
import { API_URL } from "@/constants";
import { CustomScreen } from "@/types";

type ScreensApi = {
    getScreens: () => Promise<CustomScreen[]>;
}

const getScreens = async () => {
    const response = await fetch(`${API_URL}/api/v1/screens`);
    const screens = (await response.json()) as CustomScreen[];
    return screens.map(s => ({ ...s, url: `${API_URL}${s.url}`}));
}

export const screensApi: ScreensApi = {
    getScreens,
};

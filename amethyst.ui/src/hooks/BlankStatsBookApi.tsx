import { API_URL } from "@/constants";

type BlankStatsBookApi = {
    blankStatsBookConfigured: () => Promise<boolean>;
    setBlankStatsBook: (blankStatsBook: File) => Promise<void>;
}

export const useBlankStatsBookApi = (): BlankStatsBookApi => {
    const blankStatsBookConfigured = async () => {
        const response = await fetch(`${API_URL}/api/blankStatsBook`, { method: 'HEAD' });

        switch (response.status) {
            case 200: return true;
            case 404: return false;
            default: throw new Error(`Unexpected response when attempting to check for blank statsbook: ${response.status} (${response.statusText})`);
        }
    }

    const setBlankStatsBook = async (blankStatsBook: File) => {
        const formData = new FormData();
        formData.append('file', blankStatsBook);

        const response = await fetch(
            `${API_URL}/api/blankStatsBook`,
            {
                method: 'POST',
                body: formData,
            });

        if(response.status == 400) {
            throw new InvalidStatsBookError;
        }
    }

    return { blankStatsBookConfigured, setBlankStatsBook };
}

export class InvalidStatsBookError extends Error { }
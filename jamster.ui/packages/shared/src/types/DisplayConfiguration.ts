export type DisplayConfiguration = {
    showSidebars: boolean;
    useTextBackgrounds: boolean;
    language: string;
}

export const DEFAULT_DISPLAY_CONFIGURATION: DisplayConfiguration = {
    showSidebars: true,
    useTextBackgrounds: true,
    language: 'en',
};
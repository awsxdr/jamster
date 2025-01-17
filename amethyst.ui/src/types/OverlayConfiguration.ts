export type OverlayConfiguration = {
    scale: number;
    useBackground: boolean;
    backgroundColor: string;
    language: string;
}

export const DEFAULT_OVERLAY_CONFIGURATION: OverlayConfiguration = {
    scale: 1.0,
    useBackground: false,
    backgroundColor: '#00ff00',
    language: 'en',
}
import { Control, DEFAULT_INPUT_CONTROLS, InputControls } from "@/types";
import { createContext, PropsWithChildren, useCallback, useContext, useState } from "react";
import { useCurrentUserConfiguration } from "./UserSettings";
import { useHotkeys } from "react-hotkeys-hook";

type ShortcutsProviderState = {
    shortcuts: InputControls;
    shortcutsEnabled: boolean;
    setShortcuts: (shortcuts: InputControls) => void;
    setShortcutsEnabled: (enabled: boolean) => void;
}

const DEFAULT_SHORTCUTS_PROVIDER_STATE: ShortcutsProviderState = {
    shortcuts: DEFAULT_INPUT_CONTROLS,
    shortcutsEnabled: false,
    setShortcuts: () => {},
    setShortcutsEnabled: () => {},
}

const ShortcutsContext = createContext(DEFAULT_SHORTCUTS_PROVIDER_STATE);

export const useShortcut = <TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey] | string>(
    groupKey: TGroupKey,
    controlKey: TControlKey,
    onShortcutTriggered: () => void,
) => {
    const context = useContext(ShortcutsContext);

    if (context === undefined) {
        throw new Error('useShortcut must be used inside a ShortcutsContextProvider');
    }

    const shortcut = context.shortcuts[groupKey]![controlKey as keyof InputControls[TGroupKey]] as Control | undefined;

    const handleShortcutTriggered = useCallback(() => {
        if(context.shortcutsEnabled) {
            onShortcutTriggered();
        }
    }, [context.shortcutsEnabled, onShortcutTriggered]);

    useHotkeys(shortcut?.binding ?? "not-in-use", handleShortcutTriggered, { preventDefault: true });

    return shortcut;
}

export const useShortcutsContext = () => useContext(ShortcutsContext);

export const ShortcutsContextProvider = ({ children }: PropsWithChildren) => {
    const { configuration: shortcuts, setConfiguration: setShortcuts } = useCurrentUserConfiguration<InputControls>("InputControls", DEFAULT_INPUT_CONTROLS);
    
    const handleSetShortcuts = (shortcuts: InputControls) => {
        setShortcuts(shortcuts);
    }

    const [shortcutsEnabled, setShortcutsEnabled] = useState(true);

    return (
        <ShortcutsContext.Provider value={{ shortcuts, shortcutsEnabled, setShortcuts: handleSetShortcuts, setShortcutsEnabled }}>
            { children }
        </ShortcutsContext.Provider>
    )
}
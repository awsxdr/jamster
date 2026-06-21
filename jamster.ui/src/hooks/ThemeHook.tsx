import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";

type Theme = "dark" | "light" | "system";

type ThemeProviderProps = {
    defaultTheme?: Theme;
}

type ThemeProviderState = {
    theme: Theme;
    setTheme: (theme: Theme) => void;
}

const initialState: ThemeProviderState = {
    theme: "system",
    setTheme: () => { throw new Error("setTheme called outside of ThemeProviderContext"); }
}

const ThemeProviderContext = createContext<ThemeProviderState>(initialState);

export const ThemeProvider = ({
    children,
    defaultTheme = "system"
}: PropsWithChildren<ThemeProviderProps>) => {
    const [theme, setTheme] = useState<Theme>(
        () => (localStorage.getItem("jamster-scoreboard-theme") as Theme) || defaultTheme
    );

    useEffect(() => {
        const root = window.document.documentElement;

        root.classList.remove("light", "dark");

        if(theme === "system") {
            const systemTheme = window.matchMedia("(prefers-color-scheme: dark)").matches
                ? "dark"
                : "light";

            root.classList.add(systemTheme);
        } else {
            root.classList.add(theme);
        }
    }, [theme]);

    const changeTheme = useCallback((theme: Theme) => {
        localStorage.setItem("jamster-scoreboard-theme", theme);
        setTheme(theme);
    }, [setTheme]);

    const context = useMemo(
        () => ({ theme, setTheme: changeTheme }),
        [theme, changeTheme]
    );

    return (
        <ThemeProviderContext.Provider value={context}>
            {children}
        </ThemeProviderContext.Provider>
    );
};

export const useTheme = () => {
    const context = useContext(ThemeProviderContext);

    if (context === undefined) {
        throw new Error('useTheme must be used inside a ThemeProvider');
    }

    return context;
}
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { userApi } from ".";
import { getCookie, setCookie, removeCookie } from 'typescript-cookie';

type UserLoginProviderState = {
    userName?: string;
    login: (userName: string) => void;
    logout: () => void;
}

const DEFAULT_PROVIDER_STATE: UserLoginProviderState = {
    login: () => { throw new Error('login called before context created'); },
    logout: () => { throw new Error('logout called before context created'); },
}

const UserLoginContext = createContext(DEFAULT_PROVIDER_STATE);

export const useUserLogin = () => {
    const context = useContext(UserLoginContext);

    if (context === undefined) {
        throw new Error('useUserLogin must be used inside a UserLoginContextProvider');
    }

    return context;
}

const USER_NAME_COOKIE_NAME = "jamster.currentUser";

export const UserLoginContextProvider = ({ children }: PropsWithChildren) => {
    const [userName, setUserName] = useState<string>();

    const login = useCallback((userName: string) => {
        userApi.createUser(userName).then(() => {
            setUserName(userName); 
            setCookie(USER_NAME_COOKIE_NAME, userName, { expires: 1 });
        });
    }, [setUserName]);

    const logout = useCallback(() => {
        setUserName(undefined);
        removeCookie(USER_NAME_COOKIE_NAME);
    }, [setUserName]);

    useEffect(() => {
        const cookieUser = getCookie(USER_NAME_COOKIE_NAME);

        if(cookieUser && !userName) {
            login(cookieUser);
        }
    }, [userName, login]);

    const context = useMemo(
        () => ({ userName, login, logout }),
        [userName, login, logout]
    );

    return (
        <UserLoginContext.Provider value={context}>
            { children }
        </UserLoginContext.Provider>
    )
}
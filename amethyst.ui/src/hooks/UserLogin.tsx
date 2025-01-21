import { createContext, PropsWithChildren, useContext, useEffect, useState } from "react";
import { useUserApi } from ".";
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

const USER_NAME_COOKIE_NAME = "amethyst.currentUser";

export const UserLoginContextProvider = ({ children }: PropsWithChildren) => {
    const [userName, setUserName] = useState<string>();
    const { createUser } = useUserApi();

    useEffect(() => {
        const cookieUser = getCookie(USER_NAME_COOKIE_NAME);

        if(cookieUser && !userName) {
            login(cookieUser);
        }
    }, []);

    const login = (userName: string) => {
        createUser(userName).then(() => {
            setUserName(userName); 
            setCookie(USER_NAME_COOKIE_NAME, userName, { expires: 1 });
        });
    }

    const logout = () => {
        setUserName(undefined);
        removeCookie(USER_NAME_COOKIE_NAME);
    }

    return (
        <UserLoginContext.Provider value={{ userName, login, logout }}>
            { children }
        </UserLoginContext.Provider>
    )
}
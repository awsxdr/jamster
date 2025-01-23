import { User } from "@/types";
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react";
import { useHubConnection } from "./SignalRHubConnection";
import { useUserApi } from "./UserApi";

type UserListContextProps = {
    users: User[];
}

const UserListContext = createContext<UserListContextProps>({
    users: [],
});

export const useUserList = () => {
    const { users } = useContext(UserListContext);

    return users;
}

export const UserListContextProvider = ({ children }: PropsWithChildren) => {
    const [users, setUsers] = useState<User[]>([]);

    const { connection } = useHubConnection('users');

    const userApi = useUserApi();

    const getInitialState = useCallback(async () => {
        return await userApi.getUsers();
    }, []);

    useEffect(() => {
        getInitialState().then(setUsers);
    }, []);

    useEffect(() => {
        (async () => {
            await connection?.invoke("WatchUserList");
        })();

        connection?.onreconnected(() => {
            connection?.invoke("WatchUserList");
        });

        connection?.on("UserListChanged", (users: User[]) => {
            setUsers(users);
        });

        return () => {
            connection?.off("UserListChanged");
        };
    }, [connection]);

    return (
        <UserListContext.Provider value={{ users }}>
            { children }
        </UserListContext.Provider>
    );
}
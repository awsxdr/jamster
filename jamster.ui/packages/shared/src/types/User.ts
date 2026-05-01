import { StringMap } from "./StringMap";

export type User = {
    userName: string;
}

export type UserConfigurations = {
    userName: string;
    configurations: StringMap<object>;
}
import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useCurrentGame } from ".";

type ValuePropertyMapper<TConfiguration> = {
    [P in keyof TConfiguration]: (value: string) => ThisParameterType<P>;
}

type StringPropertyMapper<TConfiguration> = {
    [P in keyof TConfiguration]: string;
}

export const useQueryStringConfiguration = <TValue,>(valueMapper: ValuePropertyMapper<TValue>, defaultValues: StringPropertyMapper<TValue>, omit: (keyof TValue)[] = []) => {
    const [ searchParams, setSearchParams ] = useSearchParams();

    const initialValue = Object.keys(valueMapper).reduce((result, key) => {
        const value = defaultValues[key as keyof typeof defaultValues];
        return { ...result, [key]: value };
    }, {} as TValue);

    const [value, setValue] = useState<TValue>(initialValue);

    useEffect(() => {
        const newValue = Object.keys(valueMapper)
            .reduce((result, key) => {
                const paramsValue = searchParams.get(key) ?? defaultValues[key as keyof typeof defaultValues];

                if(!omit?.includes(key as keyof TValue) && !searchParams.has(key)) {
                    searchParams.set(key, paramsValue);
                }

                return { ...result, [key]: valueMapper[key as keyof typeof valueMapper](paramsValue) };
            }, {} as TValue);

        setSearchParams(searchParams, { replace: true });
        setValue(newValue);
    }, []);

    useEffect(() => {
        const newValue = Object.keys(valueMapper)
            .reduce((result, key) => {
                const paramsValue = searchParams.get(key) ?? defaultValues[key as keyof typeof defaultValues];
                return { ...result, [key]: valueMapper[key as keyof typeof valueMapper](paramsValue) };
            }, {} as TValue);
        setValue(newValue);
    }, [searchParams]);

    return value;
}

export const useGameIdFromQueryString = () => {
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();
    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');

    const gameId = useMemo(() => selectedGameId === "current" ? currentGame?.id : selectedGameId, [selectedGameId, currentGame]);

    useEffect(() => {
        if (!selectedGameId && currentGame) {
            searchParams.set("gameId", "current");
            setSearchParams(searchParams, { replace: true });
            setSelectedGameId(currentGame.id);
        }
    }, [currentGame, selectedGameId]);

    useEffect(() => {
        const searchParamsGameId = searchParams.get("gameId");
        if(currentGame && searchParamsGameId === "current") {
            setSelectedGameId(currentGame.id);
        } else if(searchParamsGameId !== "current" && searchParamsGameId !== null) {
            setSelectedGameId(searchParamsGameId);
        }
    }, [currentGame, searchParams]);

    return gameId;
}

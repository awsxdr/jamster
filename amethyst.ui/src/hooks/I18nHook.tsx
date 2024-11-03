import { createContext, PropsWithChildren, useCallback, useContext, useMemo, useState } from "react";

type I18n = {
    translate: (key: string) => string;
    language: string;
    setLanguage: (language: string) => void;
}

export const useI18n: () => I18n = () => {
    const context = useContext(I18nContext);

    return {
        translate: context.translate,
        language: context.language,
        setLanguage: context.setLanguage
    };
}

type Languages = {
    [key: string]: Translations;
}

type Translations = {
    [key: string]: string;
}

type I18nContextProviderProps = {
    defaultLanguage: string;
    languages: Languages
}

type I18nContextProps = {
    language: string;
    translations: Translations;
    translate: (key: string) => string;
    setLanguage: (key: string) => void;
};

const I18nContext = createContext<I18nContextProps>({
    language: '',
    translations: {},
    translate: () => { throw new Error('translate used outside of I18nContextProvider')},
    setLanguage: () => { throw new Error('setLanguage used outside of I18nContextProvider')},
});

export const makeDevLanguage = (language: Translations) => {
    const characterReplacements: { [key: string]: string } = {
        a: 'ä',
        c: 'ċ',
        d: 'ɗ',
        e: 'é',
        g: 'ġ',
        h: 'һ',
        i: 'í',
        j: 'ј',
        k: 'κ',
        l: 'ḷ',
        n: 'ո',
        o: 'ỏ',
        p: 'р',
        q: 'զ',
        s: 'ʂ',
        u: 'ú',
        v: 'ν',
        x: 'х',
        y: 'ý',
        z: 'ż',
    };

    const devLanguage = { ...language };

    Object.keys(devLanguage).forEach(key => {
        const value = devLanguage[key];

        Object.keys(characterReplacements).forEach(target => {
            value.replace(target, characterReplacements[target]);
        });

        return value;
    });

    return devLanguage;
}

export const I18nContextProvider = ({ defaultLanguage, languages, children }: PropsWithChildren<I18nContextProviderProps>) => {

    const [language, setLanguage] = useState(localStorage.getItem('amethyst-language') ?? defaultLanguage);
    
    const translations = useMemo(() => languages[language] ?? {}, [languages, language]);

    const translate = useCallback((key: string) =>
        translations[key] ?? key, 
    [translations]);

    const changeLanguage = useCallback((key: string) => {
        setLanguage(key);
        localStorage.setItem('amethyst-language', key);
    }, [setLanguage]);

    return (
        <I18nContext.Provider value={{ language, translations, translate, setLanguage: changeLanguage }}>
            {children}
        </I18nContext.Provider>
    )
}
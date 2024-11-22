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

const makeDevTranslation = (key: string) => {
    const characterReplacements: { [key: string]: string } = {
        a: 'ä',
        A: '𝒜',
        b: '𝓫',
        B: 'Ꞵ',
        c: 'ċ',
        C: '𝑪',
        d: 'ɗ',
        D: '𝔻',
        e: 'é',
        E: '⋿',
        f: '𝕗',
        F: '𝓕',
        g: 'ġ',
        G: '𝔊',
        h: 'һ',
        H: '𝓗',
        i: 'í',
        I: 'Ǐ',
        j: 'ј',
        J: 'Ｊ',
        k: 'κ',
        K: 'Ｋ',
        l: 'ḷ',
        L: 'ℒ',
        M: '𝕸',
        n: 'ո',
        N: '𝒩',
        o: 'ỏ',
        O: 'Ｏ',
        p: 'р',
        P: '𝓟',
        q: 'զ',
        Q: 'ⵕ',
        r: '𝖗',
        R: 'ℝ',
        s: 'ʂ',
        S: '𐊖',
        t: '𝓽',
        T: '𝔗',
        u: 'ú',
        U: '⋃',
        v: 'ν',
        V: '𝓥',
        x: 'х',
        X: '𝔛',
        y: 'ý',
        Y: 'Ｙ',
        z: 'ż',
        Z: '𝒵',
    };

    return Object.keys(characterReplacements).reduce(
        (value, replace) => value.replace(new RegExp(replace, 'g'), characterReplacements[replace]),
        key
    );
}

export const I18nContextProvider = ({ defaultLanguage, languages, children }: PropsWithChildren<I18nContextProviderProps>) => {

    const [language, setLanguage] = useState(localStorage.getItem('amethyst-language') ?? defaultLanguage);
    
    const translations = useMemo(() => languages[language] ?? {}, [languages, language]);

    const translate = useCallback((key: string) => {
        if(language === 'dev') {
            return makeDevTranslation(key);
        }

        if(!translations[key]) {
            console.warn("Translation missing for key", key);
        }
        
        return translations[key] ?? key;
    }, 
    [translations, language]);

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
import { Languages, Translations } from "@/i18n";
import { createContext, PropsWithChildren, useCallback, useContext, useMemo, useState } from "react";

export type LanguageName = {
    code: string;
    displayName: string;
}

type I18n = {
    translate: (key: string) => string;
    language: string;
    languages: LanguageName[];
    setLanguage: (language: string) => void;
}

export const useI18n = (options?: { prefix?: string }): I18n => {
    const context = useContext(I18nContext);

    return {
        translate: (key: string) => context.translate((options?.prefix ?? "") + key),
        language: context.language,
        languages: context.languages,
        setLanguage: context.setLanguage
    };
}

type I18nContextProviderProps = {
    usageKey: string;
    defaultLanguage: string;
    languages: Languages
}

type I18nContextProps = {
    language: string;
    translations: Translations;
    languages: LanguageName[],
    translate: (key: string) => string;
    setLanguage: (key: string) => void;
};

const I18nContext = createContext<I18nContextProps>({
    language: '',
    translations: { name: 'error' },
    languages: [],
    translate: () => { throw new Error('translate used outside of I18nContextProvider')},
    setLanguage: () => { throw new Error('setLanguage used outside of I18nContextProvider')},
});

const makeDevTranslation = (key: string) => {
    const characterReplacements: { [key: string]: string } = {
        a: 'ä', A: '𝒜', b: '𝓫', B: 'Ꞵ', c: 'ċ', C: '𝑪', d: 'ɗ', D: '𝔻', e: 'é', E: '⋿',
        f: '𝕗', F: '𝓕', g: 'ġ', G: '𝔊', h: 'һ', H: '𝓗', i: 'í', I: 'Ǐ', j: 'ј', J: 'Ｊ',
        k: 'κ', K: 'Ｋ', l: 'ḷ', L: 'ℒ', M: '𝕸', n: 'ո', N: '𝒩', o: 'ỏ', O: 'Ｏ',
        p: 'р', P: '𝓟', q: 'զ', Q: 'ⵕ', r: '𝖗', R: 'ℝ', s: 'ʂ', S: '𐊖', t: '𝓽', T: '𝔗',
        u: 'ú', U: '⋃', v: 'ν', V: '𝓥', x: 'х', X: '𝔛', y: 'ý', Y: 'Ｙ', z: 'ż', Z: '𝒵',
    };

    return Object.keys(characterReplacements).reduce(
        (value, replace) => value.replace(new RegExp(replace, 'g'), characterReplacements[replace]),
        key
    );
}

export const I18nContextProvider = ({ usageKey, defaultLanguage, languages, children }: PropsWithChildren<I18nContextProviderProps>) => {

    const storageKey = `amethyst-language-${usageKey}`;

    const [language, setLanguage] = useState(localStorage.getItem(storageKey) ?? defaultLanguage);
    
    const translations = useMemo(() => languages[language] ?? {}, [languages, language]);
    const languageNames = Object.keys(languages).map(key => ({ code: key, displayName: languages[key].name }));

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

    if(import.meta.env.MODE === 'development') {
        languageNames.push({ code: 'dev', displayName: 'I18n test'});
    }

    const changeLanguage = useCallback((key: string) => {
        setLanguage(key);
        localStorage.setItem(storageKey, key);
    }, [setLanguage]);

    return (
        <I18nContext.Provider value={{ language, languages: languageNames, translations, translate, setLanguage: changeLanguage }}>
            {children}
        </I18nContext.Provider>
    )
}
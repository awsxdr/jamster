import { makeDevLanguage } from "./hooks/I18nHook";

const languages = {
    "en": {
        "Start jam": "Start jam",
    },
    "es": {
        "Start jam": "Iniciar jam"
    }
}

export default { ...languages, dev: makeDevLanguage(languages.en) };
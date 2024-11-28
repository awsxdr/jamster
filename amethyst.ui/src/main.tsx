import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import languages from './i18n.ts';
import { I18nContextProvider } from './hooks/I18nHook.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <I18nContextProvider defaultLanguage='en' languages={languages}>
      <App />
    </I18nContextProvider>
  </StrictMode>,
)

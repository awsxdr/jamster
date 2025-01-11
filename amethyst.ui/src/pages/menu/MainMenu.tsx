import { SidebarProvider } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';
import { ThemeProvider } from '@/hooks/ThemeHook';
import { getCookie } from 'typescript-cookie';
import { I18nContextProvider } from '@/hooks';
import languages from '@/i18n.ts';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    const sidebarState = getCookie('sidebar:state') === 'true';

    return (
        <>
            <I18nContextProvider usageKey="control" defaultLanguage='en' languages={languages}>
                <ThemeProvider defaultTheme='light'>
                    <SidebarProvider defaultOpen={sidebarState}>
                        <MenuSidebar />
                        <main className="w-full">
                            { content }
                        </main>
                    </SidebarProvider>
                </ThemeProvider>
            </I18nContextProvider>
        </>
    );
}
import { SidebarProvider } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';
import { ThemeProvider } from '@/hooks/ThemeHook';
import { I18nContextProvider } from '@/hooks';
import languages from '@/i18n.ts';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    return (
        <>
            <I18nContextProvider usageKey="control" defaultLanguage='en' languages={languages}>
                <ThemeProvider defaultTheme="system">
                    <SidebarProvider defaultOpen>
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
import { SidebarProvider } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';
import styles from './MainMenu.module.scss';
import { ThemeProvider } from '@/hooks/ThemeHook';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    return (
        <>
            <ThemeProvider defaultTheme='light'>
                <SidebarProvider>
                    <MenuSidebar />
                    <main className={styles.content}>
                        { content }
                    </main>
                </SidebarProvider>
            </ThemeProvider>
        </>
    );
}
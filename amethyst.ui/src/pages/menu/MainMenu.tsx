import { SidebarProvider } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';
import styles from './MainMenu.module.scss';
import { ThemeProvider } from '@/hooks/ThemeHook';
import { getCookie } from 'typescript-cookie';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    const sidebarState = getCookie('sidebar:state') === 'true';
    return (
        <>
            <ThemeProvider defaultTheme='light'>
                <SidebarProvider defaultOpen={sidebarState}>
                    <MenuSidebar />
                    <main className={styles.content}>
                        { content }
                    </main>
                </SidebarProvider>
            </ThemeProvider>
        </>
    );
}
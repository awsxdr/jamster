import { SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';
import styles from './MainMenu.module.scss';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    return (
        <>
            <SidebarProvider>
                <MenuSidebar />
                <main className={styles.content}>
                    <SidebarTrigger />
                    { content }
                </main>
            </SidebarProvider>
        </>
    );
}
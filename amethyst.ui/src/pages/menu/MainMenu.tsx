import { SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar';
import { MenuSidebar } from './components/MenuSidebar';
import { ReactNode } from 'react';

type MainMenuProps = {
    content: ReactNode;
};

export const MainMenu = ({ content }: MainMenuProps) => {
    return (
        <>
            <SidebarProvider>
                <MenuSidebar />
                <main>
                    <SidebarTrigger />
                    { content }
                </main>
            </SidebarProvider>
        </>
    );
}
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger"

export const TitlePage = () => {
    return (
        <>
            <div className="flex w-full">
                <div className="grow">
                    <MobileSidebarTrigger className="m-5" />
                </div>
            </div>
            <div>
                Welcome to Amethyst!
            </div>
        </>
    )
}
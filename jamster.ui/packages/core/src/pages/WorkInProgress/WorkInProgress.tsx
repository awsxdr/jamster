import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger"

export const WorkInProgress = () => {
    return (
        <div>
            <div className="flex w-full items-center mt-1 mb-2 pr-2">
                <MobileSidebarTrigger className="mx-5" />
            </div>
            <div className="px-5">
                This software is still in early development and as such not all features are ready yet. Check back here in a later version.
            </div>
        </div>
    )
}
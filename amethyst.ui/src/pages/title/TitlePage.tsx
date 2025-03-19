import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger"
import { useI18n } from "@/hooks"

export const TitlePage = () => {

    const { translate } = useI18n();

    return (
        <>
            <title>{translate("Main.Title")}</title>
            <div className="flex w-full items-center mt-1 mb-2 pr-2">
                <MobileSidebarTrigger className="mx-5" />
            </div>
            <div className="px-5">
                Welcome to DerbyStats!
            </div>
        </>
    )
}
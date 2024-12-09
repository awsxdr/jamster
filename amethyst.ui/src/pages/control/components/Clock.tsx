import { Card, CardContent } from "@/components/ui/card";
import { ReactNode } from "react";

type ClockProps = {
    name: string;
    clock: (className: string) => ReactNode
}

export const Clock = ({ name, clock }: ClockProps) => {
    return (
        <Card className="grow lg:w-1/5">
            <CardContent className="p-4 items-center justify-between lg:flex lg:flex-wrap">
                <span className="text-xl block">{name}</span>
                { clock("block text-center h-full text-4xl") }
            </CardContent>
        </Card>
    )
}
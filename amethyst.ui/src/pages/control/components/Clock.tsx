import { Card, CardContent } from "@/components/ui/card";
import { ReactNode } from "react";

type ClockProps = {
    name: string;
    clock: (className: string) => ReactNode
}

export const Clock = ({ name, clock }: ClockProps) => {
    return (
        <Card className="w-1/5">
            <CardContent className="flex pt-4 items-center justify-between flex-wrap">
                <span className="text-xl">{name}</span>
                { clock("inline text-center h-full text-4xl") }
            </CardContent>
        </Card>
    )
}
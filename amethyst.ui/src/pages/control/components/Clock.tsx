import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ReactNode } from "react";

type ClockProps = {
    name: string;
    clock: (className: string) => ReactNode
}

export const Clock = ({ name, clock }: ClockProps) => {
    return (
        <Card className="w-1/5">
            <CardHeader>
                <CardTitle>{name}</CardTitle>
            </CardHeader>
            <CardContent className="h-24">
                { clock("block text-center h-full w-full") }
            </CardContent>
        </Card>
    )
}
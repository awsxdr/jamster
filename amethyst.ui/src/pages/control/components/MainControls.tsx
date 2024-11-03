import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Clock, Play } from "lucide-react";

export const MainControls = () => {
    return (
        <Card className="grow m-2 pt-6">
            <CardContent className="flex justify-evenly">
                <Button><Play /> Start jam [`]</Button>
                <Button><Clock /> New timeout [t]</Button>
            </CardContent>
        </Card>
    );
}
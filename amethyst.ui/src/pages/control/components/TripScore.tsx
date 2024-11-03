import { Button } from "@/components/ui/button"

export const TripScore = () => {
    return (
        <div className="flex flex-wrap justify-center items-center m-2 space-x-2">
            <span>Trip score</span>
            <span className="flex flex-wrap justify-center items-center m-2 space-x-2 gap-y-2">
                <Button variant="secondary">0</Button>
                <Button variant="secondary">1</Button>
                <Button variant="secondary">2</Button>
                <Button variant="secondary">3</Button>
                <Button variant="secondary">4</Button>
            </span>
        </div>
    )
}
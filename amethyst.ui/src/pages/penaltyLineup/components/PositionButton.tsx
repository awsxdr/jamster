import { Button } from "@/components/ui";
import { useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition } from "@/types";

type PositionButtonProps = {
    position: LineupPosition;
    targetPosition: LineupPosition;
    className?: string;
    rowClassName?: string;
    onClick?: (position: LineupPosition) => void;
}

export const PositionButton = ({ position, targetPosition, className, rowClassName, onClick }: PositionButtonProps) => {
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyButton." })

    return (
        <Button 
            className={cn(className, position !== targetPosition && rowClassName)}
            variant={position === targetPosition ? "default" : "outline" }
            onClick={() => onClick?.(targetPosition)}
        >
            <span className="lg:hidden">{translate(`${targetPosition}.Short`)}</span>
            <span className="hidden lg:inline">{translate(`${targetPosition}.Long`)}</span>
        </Button>
    );
}
import { Button, ButtonVariant } from "@/components/ui";
import { useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition } from "@/types";
import { switchex } from "@/utilities/switchex";

type PositionButtonProps = {
    position: LineupPosition;
    targetPosition: LineupPosition;
    className?: string;
    rowClassName?: string;
    compact?: boolean;
    offTrack?: boolean;
    onClick?: (position: LineupPosition) => void;
}

export const PositionButton = ({ position, targetPosition, className, rowClassName, compact, offTrack: injured, onClick }: PositionButtonProps) => {
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyButton." })

    const variant =
        switchex(position)
            .case(targetPosition).when(() => injured ?? false).then<ButtonVariant>("destructive")
            .case(targetPosition).then("default")
            .default("outline");

    return (
        <Button 
            className={cn(className, position !== targetPosition && rowClassName)}
            variant={variant}
            onClick={() => onClick?.(targetPosition)}
        >
            <span className={cn(!compact && "lg:hidden")}>{translate(`${targetPosition}.Short`)}</span>
            <span className={cn("hidden", !compact && "lg:inline")}>{translate(`${targetPosition}.Long`)}</span>
        </Button>
    );
}
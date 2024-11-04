import { TeamSide } from "@/types";
import { TeamName } from "./TeamName";

import styles from './TeamDetails.module.scss';
import { TeamTimeouts } from "./TeamTimeouts";
import { TeamScore } from "./TeamScore";
import { TripScore } from "./TripScore";
import { cn } from "@/lib/utils";

type TeamDetailsProps = {
    side: TeamSide
}

export const TeamDetails = ({ side }: TeamDetailsProps) => {
    return (
        <div className={styles.container}>
            <div className={styles.nameContainer}>
                <TeamName side={side} />
            </div>
            <div className={cn(styles.scoreAndTimeoutsContainer, side === TeamSide.Home ? styles.home : styles.away)}>
                <TeamTimeouts side={side} />
                <TeamScore side={side} />
                <TripScore side={side} />
            </div>
        </div>
    );
}
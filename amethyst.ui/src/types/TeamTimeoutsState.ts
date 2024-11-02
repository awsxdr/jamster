import { ReviewStatus, TimeoutInUse } from ".";

export type TeamTimeoutsState = {
    numberRemaining: number,
    reviewStatus: ReviewStatus,
    currentTimeout: TimeoutInUse,
};


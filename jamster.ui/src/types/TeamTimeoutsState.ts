import { ReviewStatus, TimeoutInUse } from ".";

export type TeamTimeoutsState = {
    numberTaken: number,
    reviewStatus: ReviewStatus,
    currentTimeout: TimeoutInUse,
};


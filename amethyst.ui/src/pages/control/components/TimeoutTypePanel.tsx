import { Button, Card, CardContent } from "@/components/ui";
import { useI18n } from "@/hooks/I18nHook";
import { TeamSide, TimeoutType } from "@/types";
import { useState } from "react";

export const TimeoutTypePanel = () => {

    const { translate } = useI18n();

    const [timeoutType, setTimeoutType] = useState<TimeoutType>(TimeoutType.Untyped);
    const [timeoutTeam, setTimeoutTeam] = useState<TeamSide>();

    const handleHomeTeamTimeout = () => {
        if(timeoutType === TimeoutType.Team && timeoutTeam === TeamSide.Home) {
            setTimeoutType(TimeoutType.Untyped);
        } else {
            setTimeoutType(TimeoutType.Team);
            setTimeoutTeam(TeamSide.Home);
        }
    }

    const handleHomeTeamReview = () => {
        if(timeoutType === TimeoutType.Review && timeoutTeam === TeamSide.Home) {
            setTimeoutType(TimeoutType.Untyped);
        } else {
            setTimeoutType(TimeoutType.Review);
            setTimeoutTeam(TeamSide.Home);
        }
    }

    const handleAwayTeamTimeout = () => {
        if(timeoutType === TimeoutType.Team && timeoutTeam === TeamSide.Away) {
            setTimeoutType(TimeoutType.Untyped);
        } else {
            setTimeoutType(TimeoutType.Team);
            setTimeoutTeam(TeamSide.Away);
        }
    }

    const handleAwayTeamReview = () => {
        if(timeoutType === TimeoutType.Review && timeoutTeam === TeamSide.Away) {
            setTimeoutType(TimeoutType.Untyped);
        } else {
            setTimeoutType(TimeoutType.Review);
            setTimeoutTeam(TeamSide.Away);
        }
    }

    const handleOfficialTimeout = () => {
        if(timeoutType === TimeoutType.Official) {
            setTimeoutType(TimeoutType.Untyped);
        } else {
            setTimeoutType(TimeoutType.Official);
        }
    }

    return (
        <Card className="grow mt-5 pt-6">
            <CardContent className="flex justify-evenly">
                <Button variant={timeoutType === TimeoutType.Team && timeoutTeam === TeamSide.Home ? 'default' : 'secondary'} onClick={handleHomeTeamTimeout}>{translate("TimeoutType.HomeTeam")}</Button>
                <Button variant={timeoutType === TimeoutType.Review && timeoutTeam === TeamSide.Home ? 'default' : 'secondary'} onClick={handleHomeTeamReview}>{translate("TimeoutType.HomeTeamReview")}</Button>
                <Button variant={timeoutType === TimeoutType.Official ? 'default' : 'secondary'} onClick={handleOfficialTimeout}>{translate("TimeoutType.Official") }</Button>
                <Button variant={timeoutType === TimeoutType.Team && timeoutTeam === TeamSide.Away ? 'default' : 'secondary'} onClick={handleAwayTeamTimeout}>{translate("TimeoutType.AwayTeam")}</Button>
                <Button variant={timeoutType === TimeoutType.Review && timeoutTeam === TeamSide.Away ? 'default' : 'secondary'} onClick={handleAwayTeamReview}>{translate("TimeoutType.AwayTeamReview")}</Button>
            </CardContent>
        </Card>
    );
}
import { Button, Card, Collapsible, CollapsibleContent, CollapsibleTrigger, Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger, Input, ScrollArea, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, Switch } from "@/components/ui";
import { useEvents, useI18n, useRulesState } from "@/hooks";
import { cn } from "@/lib/utils";
import { IntermissionRules, JamRules, LineupRules, PenaltyRules, PeriodEndBehavior, PeriodRules, Ruleset, TimeoutPeriodClockStopBehavior, TimeoutResetBehavior, TimeoutRules } from "@/types";
import { RulesetSet } from "@/types/events";
import { DialogProps, DialogTriggerProps } from "@radix-ui/react-dialog";
import { ChevronRight } from "lucide-react";
import { KeyboardEvent, PropsWithChildren, useEffect, useRef, useState } from "react";

export const RulesDialogContainer = ({ children, ...props }: PropsWithChildren<DialogProps>) => (
    <Dialog {...props} modal>
        {children}
    </Dialog>
)

type RuleItemProps = {
    title: string;
    titleClassName?: string;
    className?: string;
}

const RuleItem = ({ title, titleClassName, className, children }: PropsWithChildren<RuleItemProps>) => (
    <>
        <div className={cn("col-start-1", titleClassName)}>{title}</div>
        <div className={cn("col-start-2", className)}>{ children }</div>
    </>
)

type RuleGroupProps = {
    title: string;
}

const RuleGroup = ({ title, children }: PropsWithChildren<RuleGroupProps>) => (
    <Collapsible>
        <CollapsibleTrigger asChild>
            <Button className="flex rounded-none justify-start w-full transition group/collapsible" variant="ghost">
                <div className="grow text-left">
                    {title}
                </div>
                <ChevronRight className="transition duration-300 ease-in-out group-data-[state=open]/collapsible:rotate-90" />
            </Button>
        </CollapsibleTrigger>
        <CollapsibleContent>
            <div className="m-2 ml-6 grid grid-flow-col grid-cols-[minmax(auto,60%)_1fr] gap-2 items-center">
                { children }
            </div>
        </CollapsibleContent>
    </Collapsible>
)

export const RulesDialogTrigger = ({ children, ...props }: PropsWithChildren<DialogTriggerProps>) => (
    <DialogTrigger {...props}>
        {children}
    </DialogTrigger>
)

type RuleInputProps = {
    initialValue: string;
    onChange: (value: string) => void;
}

const RuleInput = ({ initialValue, onChange }: RuleInputProps) => {
    const [value, setValue] = useState(initialValue);

    useEffect(() => setValue(initialValue), [initialValue]);

    const inputRef = useRef<HTMLInputElement>(null);

    const handleBlur = () => {
        if(value === initialValue) {
            return;
        }

        onChange(value);
    }

    const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
        if(event.key === "Escape") {
            event.preventDefault();
            setValue(initialValue);
            inputRef.current?.blur();
        }
    }
    
    return (
        <Input
            ref={inputRef}
            value={value} 
            onChange={e => setValue(e.target.value)} 
            onKeyDown={handleKeyDown}
            onBlur={handleBlur}
        />
    );
}

type RulesDialogProps = {
    gameId: string;
}

const TIME_REGEX = /^(?:(?<h>\d+):){0,1}?(?:(?<m>\d{1,2}):)?(?<s>\d{1,2})?$/;

export const RulesDialog = ({ gameId }: RulesDialogProps) => {

    const { translate } = useI18n({ prefix: "ScoreboardControl.RulesDialog." });
    

    const { rules } = useRulesState() ?? { };
    const { sendEvent } = useEvents();

    if(!rules) {
        return <></>
    }

    const formatTime = (totalSeconds: number) => {
        const totalMinutes = Math.floor(totalSeconds / 60);
        const hours = Math.floor(totalMinutes / 60);
        const minutes = totalMinutes % 60;
        const seconds = totalSeconds % 60;

        const padComponent = (value: number) => value.toString().padStart(2, '0');

        return hours > 0 ? `${hours}:${padComponent(minutes)}:${padComponent(seconds)}`
            : minutes > 0 ? `${minutes}:${padComponent(seconds)}`
            : seconds.toString();
    }

    const handleChange = (value: (rules: Ruleset) => Ruleset) =>
        sendEvent(gameId, new RulesetSet({ ...rules, ...value(rules) }));

    const handlePeriodChange = (value: Partial<PeriodRules>) =>
        handleChange(r => ({ ...r, periodRules: { ...r.periodRules, ...value }}));

    const handleJamChange = (value: Partial<JamRules>) =>
        handleChange(r => ({ ...r, jamRules: { ...r.jamRules, ...value }}));

    const handleLineupChange = (value: Partial<LineupRules>) =>
        handleChange(r => ({ ...r, lineupRules: { ...r.lineupRules, ...value }}));

    const handleTimeoutChange = (value: Partial<TimeoutRules>) =>
        handleChange(r => ({ ...r, timeoutRules: { ...r.timeoutRules, ...value }}));

    const handleIntermissionChange = (value: Partial<IntermissionRules>) =>
        handleChange(r => ({ ...r, intermissionRules: { ...r.intermissionRules, ...value }}));

    const handlePenaltyChange = (value: Partial<PenaltyRules>) =>
        handleChange(r => ({ ...r, penaltyRules: { ...r.penaltyRules, ...value }}));

    const parsePositiveNumber = (value: string) => {
        const numberValue = parseInt(value);
        
        return Number.isNaN(numberValue) ? 0 : Math.max(0, numberValue);
    }

    const parseTime = (value: string) => {
        const match = value.match(TIME_REGEX);
        
        if(!match) {
            return undefined;
        }

        return (
            parseInt(match.groups?.['h'] ?? '0') * 60 * 60
            + parseInt(match.groups?.['m'] ?? '0') * 60
            + parseInt(match.groups?.['s'] ?? '0')
        );
    }

    return (
        <DialogContent className="max-h-[100%] overflow-auto">
            <DialogHeader>
                <DialogTitle>{translate("Title")}</DialogTitle>
                <DialogDescription>{translate("Description")}</DialogDescription>
            </DialogHeader>
            <ScrollArea>
                <Card className="overflow-hidden">
                    <RuleGroup title={translate("Group.Period")}>
                        <RuleItem title={translate("PeriodCount")}>
                            <RuleInput 
                                initialValue={rules.periodRules.periodCount.toString()} 
                                onChange={v => handlePeriodChange({ periodCount: parsePositiveNumber(v) })}
                            />
                        </RuleItem>
                        <RuleItem title={translate("PeriodDuration")}>
                            <RuleInput 
                                initialValue={formatTime(rules.periodRules.durationInSeconds)} 
                                onChange={v => handlePeriodChange({ durationInSeconds: parseTime(v) ?? rules.periodRules.durationInSeconds})}
                            />
                        </RuleItem>
                        <RuleItem title={translate("PeriodEnd")}>
                            <Select 
                                value={rules.periodRules.periodEndBehavior} 
                                onValueChange={v => handlePeriodChange({ periodEndBehavior: v as PeriodEndBehavior })}
                            >
                                <SelectTrigger>
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value={PeriodEndBehavior.AnytimeOutsideJam}>{translate("PeriodEnd.AnytimeOutsideJam")}</SelectItem>
                                    <SelectItem value={PeriodEndBehavior.OnJamEnd}>{translate("PeriodEnd.OnJamEnd")}</SelectItem>
                                    <SelectItem value={PeriodEndBehavior.Immediately}>{translate("PeriodEnd.Immediately")}</SelectItem>
                                    <SelectItem value={PeriodEndBehavior.Manual}>{translate("PeriodEnd.Manual")}</SelectItem>
                                </SelectContent>
                            </Select>
                        </RuleItem>
                    </RuleGroup>
                    <RuleGroup title={translate("Group.Jam")}>
                        <RuleItem title={translate("JamDuration")}>
                            <RuleInput 
                                initialValue={formatTime(rules.jamRules.durationInSeconds)} 
                                onChange={v => handleJamChange({ durationInSeconds: parseTime(v) ?? rules.jamRules.durationInSeconds }) } 
                            />
                        </RuleItem>
                        <RuleItem title={translate("JamReset")} className="text-right">
                            <Switch 
                                checked={rules.jamRules.resetJamNumbersBetweenPeriods} 
                                onCheckedChange={v => handleJamChange({ resetJamNumbersBetweenPeriods: v })}
                            />
                        </RuleItem>
                    </RuleGroup>
                    <RuleGroup title={translate("Group.Lineup")}>
                        <RuleItem title={translate("LineupDuration")}>
                            <RuleInput 
                                initialValue={formatTime(rules.lineupRules.durationInSeconds)} 
                                onChange={v => handleLineupChange({ durationInSeconds: parseTime(v) ?? rules.lineupRules.durationInSeconds })}
                            />
                        </RuleItem>
                        <RuleItem title={translate("LineupOvertimeDuration")}>
                            <RuleInput 
                                initialValue={formatTime(rules.lineupRules.overtimeDurationInSeconds)} 
                                onChange={v => handleLineupChange({ overtimeDurationInSeconds: parseTime(v) ?? rules.lineupRules.overtimeDurationInSeconds })}
                            />
                        </RuleItem>
                    </RuleGroup>
                    <RuleGroup title={translate("Group.Timeout")}>
                        <RuleItem title={translate("TimeoutTeamDuration")}>
                            <RuleInput
                                initialValue={formatTime(rules.timeoutRules.teamTimeoutDurationInSeconds)} 
                                onChange={v => handleTimeoutChange({ teamTimeoutDurationInSeconds: parseTime(v) ?? rules.timeoutRules.teamTimeoutDurationInSeconds })}
                            />
                        </RuleItem>
                        <RuleItem title={translate("TimeoutAllowance")}>
                            <RuleInput
                                initialValue={rules.timeoutRules.teamTimeoutAllowance.toString()} 
                                onChange={v => handleTimeoutChange({ teamTimeoutAllowance: parsePositiveNumber(v) })}
                            />
                        </RuleItem>
                        <RuleItem title={translate("TimeoutReset")}>
                            <Select 
                                value={rules.timeoutRules.resetBehavior}
                                onValueChange={v => handleTimeoutChange({ resetBehavior: TimeoutResetBehavior[v as TimeoutResetBehavior] })}
                            >
                                <SelectTrigger>
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value={TimeoutResetBehavior.Never}>{translate("TimeoutReset.Never")}</SelectItem>
                                    <SelectItem value={TimeoutResetBehavior.Period}>{translate("TimeoutReset.Period")}</SelectItem>
                                </SelectContent>
                            </Select>
                        </RuleItem>
                        <RuleItem title={translate("TimeoutPeriodClockBehavior")} titleClassName="self-start">
                            <div className="grid grid-flow-col grid-cols-[auto_1fr] gap-2 self-start">
                                <div className="col-start-1">{translate("TimeoutPeriodClockBehaviour.Team")}</div>
                                <Switch 
                                    className="col-start-2" 
                                    checked={(rules.timeoutRules.periodClockBehavior & TimeoutPeriodClockStopBehavior.TeamTimeout) !== 0}
                                    onCheckedChange={checked => handleTimeoutChange({ 
                                        periodClockBehavior: checked 
                                            ? rules.timeoutRules.periodClockBehavior | TimeoutPeriodClockStopBehavior.TeamTimeout
                                            : rules.timeoutRules.periodClockBehavior & ~TimeoutPeriodClockStopBehavior.TeamTimeout
                                    })}
                                />
                                <div className="col-start-1">{translate("TimeoutPeriodClockBehaviour.Review")}</div>
                                <Switch 
                                    className="col-start-2" 
                                    checked={(rules.timeoutRules.periodClockBehavior & TimeoutPeriodClockStopBehavior.OfficialReview) !== 0} 
                                    onCheckedChange={checked => handleTimeoutChange({ 
                                        periodClockBehavior: checked 
                                            ? rules.timeoutRules.periodClockBehavior | TimeoutPeriodClockStopBehavior.OfficialReview
                                            : rules.timeoutRules.periodClockBehavior & ~TimeoutPeriodClockStopBehavior.OfficialReview
                                    })}
                                />
                                <div className="col-start-1">{translate("TimeoutPeriodClockBehaviour.Official")}</div>
                                <Switch 
                                    className="col-start-2" 
                                    checked={(rules.timeoutRules.periodClockBehavior & TimeoutPeriodClockStopBehavior.OfficialTimeout) !== 0} 
                                    onCheckedChange={checked => handleTimeoutChange({ 
                                        periodClockBehavior: checked 
                                            ? rules.timeoutRules.periodClockBehavior | TimeoutPeriodClockStopBehavior.OfficialTimeout
                                            : rules.timeoutRules.periodClockBehavior & ~TimeoutPeriodClockStopBehavior.OfficialTimeout
                                    })}
                                />
                            </div>
                        </RuleItem>
                    </RuleGroup>
                    <RuleGroup title={translate("Group.Intermission")}>
                        <RuleItem title={translate("IntermissionDuration")}>
                            <RuleInput 
                                initialValue={formatTime(rules.intermissionRules.durationInSeconds)} 
                                onChange={v => handleIntermissionChange({ durationInSeconds: parseTime(v) })}
                            />
                        </RuleItem>
                    </RuleGroup>
                    <RuleGroup title={translate("Group.Penalty")}>
                        <RuleItem title={translate("PenaltyFouloutCount")}>
                            <RuleInput 
                                initialValue={rules.penaltyRules.foulOutPenaltyCount.toString()} 
                                onChange={v => handlePenaltyChange({ foulOutPenaltyCount: parsePositiveNumber(v) })}
                            />
                        </RuleItem>
                    </RuleGroup>
                </Card>
            </ScrollArea>
        </DialogContent>
    );
}
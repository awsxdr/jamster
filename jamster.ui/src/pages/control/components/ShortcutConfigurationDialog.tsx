import { Button, Card, Collapsible, CollapsibleContent, CollapsibleTrigger, Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger, ScrollArea } from "@/components/ui";
import { useI18n } from "@/hooks";
import { useShortcutsContext } from "@/hooks/InputControls";
import { cn } from "@/lib/utils";
import { Control, InputControls, InputControlsItem, InputType } from "@/types";
import { DialogProps, DialogTriggerProps } from "@radix-ui/react-dialog";
import { ChevronRight, Trash } from "lucide-react";
import { PropsWithChildren, useEffect, useMemo, useRef, useState } from "react";
import { useRecordHotkeys } from "react-hotkeys-hook";

export const ShortcutConfigurationDialogContainer = ({ children, ...props }: PropsWithChildren<DialogProps>) => (
    <Dialog {...props} modal>
        {children}
    </Dialog>   
);

export const ShortcutConfigurationDialogTrigger = ({ children, ...props }: PropsWithChildren<DialogTriggerProps>) => (
    <DialogTrigger {...props}>
        {children}
    </DialogTrigger>
);

type ShortcutRecorderProps = {
    input: Control;
    className?: string;
    onRecordingStarted: () => void;
    onRecordingEnded: () => void;
    onShortcutSet: (keys: string) => void;
}

const ShortcutRecorder = ({ input, className, onRecordingStarted, onRecordingEnded, onShortcutSet }: ShortcutRecorderProps) => {

    const [keys, { start, stop, resetKeys, isRecording }] = useRecordHotkeys();
    const [keyDownCount, setKeyDownCount] = useState(0);
    const inputRef = useRef<HTMLDivElement>(null);

    const handleFocus = () => {
        start();
        onRecordingStarted();
    }

    const handleBlur = () => {
        stop();
    }

    const formattedKeys = useMemo(() => Array.from(keys).join("+"), [keys]);

    const handleKeyDown = () => {
        setKeyDownCount(v => v + 1);
    }

    const handleKeyUp = () => {
        setKeyDownCount(v => v - 1);
    }

    useEffect(() => {
        if (keyDownCount === 0) {
            if(formattedKeys) {
                onShortcutSet(formattedKeys);
                onRecordingEnded();
                inputRef.current?.blur();
            }
            resetKeys();
        }
    }, [keyDownCount]);

    return (
        <div 
            ref={inputRef}
            className={cn("min-w-[40%] bg-accent border border-transparent focus:border-destructive cursor-pointer", className)}
            tabIndex={0} 
            onFocus={handleFocus}
            onBlur={handleBlur}
            onKeyDown={handleKeyDown}
            onKeyUp={handleKeyUp}
        >
            { isRecording ? formattedKeys : input?.binding }
        </div>
    );
}

type ShortcutItemProps<TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey]> = {
    groupName: TGroupKey;
    controlName: TControlKey;
    input: Control;
    onRecordingStarted: () => void;
    onRecordingEnded: () => void;
    onShortcutSet: (groupName: TGroupKey, controlName: TControlKey, keys?: string) => void;
}

const ShortcutItem = <TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey] & string>({ 
    groupName, 
    controlName, 
    input, 
    onRecordingStarted, 
    onRecordingEnded, 
    onShortcutSet 
}: ShortcutItemProps<TGroupKey, TControlKey>) => {
    const { translate } = useI18n();

    const handleShortcutSet = (keys: string) => {
        onShortcutSet(groupName, controlName, keys);
    }

    const handleShortcutRemoved = () => {
        onShortcutSet(groupName, controlName, undefined);
    }

    return (
        <>
            <div className="col-start-1">{translate(`Shortcut.${groupName}.${controlName}`)}</div>
            <ShortcutRecorder
                input={input}
                className="col-start-2"
                onRecordingStarted={onRecordingStarted} 
                onRecordingEnded={onRecordingEnded} 
                onShortcutSet={handleShortcutSet}
            />
            <Button variant="ghost" className="col-start-3 text-red-500" size="icon" onClick={handleShortcutRemoved}>
                <Trash />
            </Button>
        </>
    )
}

type ShortcutGroupProps<TGroup extends InputControlsItem, TGroupKey extends keyof InputControls> = {
    groupName: TGroupKey,
    group: TGroup;
    onRecordingStarted: () => void;
    onRecordingEnded: () => void;
    onShortcutSet: <TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey]>(groupName: TGroupKey, controlName: TControlKey, keys?: string) => void;
}

const ShortcutGroup = <TGroup extends InputControlsItem, TGroupKey extends keyof InputControls>(
    { groupName, group, onRecordingStarted, onRecordingEnded, onShortcutSet }: ShortcutGroupProps<TGroup, TGroupKey>
) => {
    const { translate } = useI18n();

    return (
        <Collapsible>
            <CollapsibleTrigger asChild>
                <Button className="flex rounded-none justify-start w-full transition group/collapsible" variant="ghost">
                    <div className="grow text-left">
                        {translate(`Shortcut.${groupName}`)}
                    </div>
                    <ChevronRight className="transition duration-300 ease-in-out group-data-[state=open]/collapsible:rotate-90" />
                </Button>
            </CollapsibleTrigger>
            <CollapsibleContent className="ml-6 grid grid-flow-col grid-cols-[50%_auto_36px]">
                {Object.keys(group).map(controlName => (
                    <ShortcutItem 
                        key={controlName}
                        groupName={groupName} 
                        controlName={controlName as keyof InputControls[TGroupKey] & string} 
                        input={group[controlName as keyof InputControlsItem]} 
                        onRecordingStarted={onRecordingStarted}
                        onRecordingEnded={onRecordingEnded}
                        onShortcutSet={onShortcutSet}
                    />
                ))}
            </CollapsibleContent>
        </Collapsible>
    )
}

export const ShortcutConfigurationDialog = () => {

    const { shortcuts, setShortcuts, setShortcutsEnabled } = useShortcutsContext();
    const { translate } = useI18n({ prefix: "ScoreboardControl.ShortcutConfigurationDialog."});

    const handleRecordingStarted = () => setShortcutsEnabled(false);
    const handleRecordingEnded = () => setShortcutsEnabled(true);

    const handleShortcutSet = <TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey]>(
        groupName: TGroupKey, 
        controlName: TControlKey,
        keys?: string
    ) => {
        setShortcuts({
            ...shortcuts,
            [groupName]: {
                ...shortcuts[groupName],
                [controlName]: {
                    inputType: InputType.Keyboard,
                    binding: keys
                }
            }
        });
    }

    return (
        <DialogContent className="max-h-[100%] overflow-auto">
            <DialogHeader>
                <DialogTitle>{translate("Title")}</DialogTitle>
                <DialogDescription>{translate("Description")}</DialogDescription>
            </DialogHeader>
            <ScrollArea>
                <Card className="overflow-hidden">
                    { Object.keys(shortcuts).map((groupName) =>
                        <ShortcutGroup 
                            key={groupName}
                            groupName={groupName as keyof InputControls} 
                            group={shortcuts[groupName as keyof InputControls]} 
                            onRecordingStarted={handleRecordingStarted}
                            onRecordingEnded={handleRecordingEnded}
                            onShortcutSet={handleShortcutSet}
                        />
                    )}
                </Card>
            </ScrollArea>
        </DialogContent>
    );
}
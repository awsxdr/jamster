import { MouseEvent, ReactNode, useEffect, useMemo, useState } from "react";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, Card, CardContent, Checkbox, Form, FormControl, FormField, FormItem, FormMessage, Input, Separator } from "@/components/ui"
import { useI18n, useTeamApi } from "@/hooks";
import { cn } from "@/lib/utils";
import { SkaterRole, Team } from "@/types";
import { Check, Pencil, Trash, X } from "lucide-react";
import { useRosterInputSchema } from "./RosterInput";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

type RowLayoutProps = {
    selectContent?: ReactNode;
    numberContent?: ReactNode;
    nameContent?: ReactNode;
    toolsContent?: ReactNode;
    contentWrapper?: (children: ReactNode, rowClassName: string) => ReactNode;
    className?: string;
    onClick?: () => void;
}

const RowLayout = ({ className, selectContent, numberContent, nameContent, toolsContent, contentWrapper, onClick }: RowLayoutProps) => {

    const defaultContentWrapper = (children: ReactNode) => <>{children}</>;

    const rowClassName = cn("flex w-full flex-nowrap items-center", className);

    return (
        <div className={rowClassName} onClick={onClick}>
            {(contentWrapper ?? defaultContentWrapper)(
                <>
                    <div className="flex w-8 text-nowrap justify-center">
                        {selectContent}
                    </div>
                    <div className="flex grow text-nowrap flex-nowrap gap-2">
                        <div className="w-1/3">{numberContent}</div>
                        <div className="w-2/3">{nameContent}</div>
                    </div>
                    <div className="flex w-20 text-nowrap justify-end">
                        {toolsContent}
                    </div>
                </>
            , rowClassName)}
        </div>
    );
}

type RosterTableRowProps = {
    number: string;
    name: string;
    preventEdit?: boolean;
    selected?: boolean;
    existingNumbers: string[];
    disableSelect?: boolean;
    onEditStart?: () => void;
    onEditEnd?: () => void;
    onSelectedChanged?: (selected: boolean) => void;
    onSkaterChanged?: (number: string, name: string) => void;
}

const RosterTableRow = ({ number, name, preventEdit, selected, existingNumbers, disableSelect, onEditStart, onEditEnd, onSelectedChanged, onSkaterChanged }: RosterTableRowProps) => {

    const [isEditing, setIsEditing] = useState(false);

    const formSchema = useRosterInputSchema(existingNumbers);

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            number: number,
            name: name,
        }
    });

    useEffect(() => {
        form.setFocus('number', { shouldSelect: true });
    }, [isEditing]);

    const handleEditClick = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsEditing(true);
        onEditStart?.();
    }

    const handleAcceptEdit = ({number, name}: { number: string, name: string }) => {
        setIsEditing(false);
        onEditEnd?.();
        onSkaterChanged?.(number, name);
        form.reset();
    }

    const handleRejectEdit = () => {
        setIsEditing(false);
        onEditEnd?.();
        form.reset();
    }

    return (
        <RowLayout
            className={cn("py-2", selected && "bg-accent")}
            contentWrapper={(children, rowClassName) => 
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(handleAcceptEdit)} className={cn(rowClassName, "p-0")}>
                        {children}
                    </form>
                </Form>}
            selectContent={
                <Checkbox checked={selected} onCheckedChange={onSelectedChanged} disabled={disableSelect} />
            }
            numberContent={
                isEditing ? (
                    <FormField control={form.control} name="number" render={({field}) => (
                        <FormItem>
                            <FormControl>
                                <Input {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )} />
                ) : (
                    <span className="inline-block text-nowrap">{number}</span>
                )
            }
            nameContent={
                isEditing ? (
                    <FormField control={form.control} name="name" render={({field}) => (
                        <FormItem>
                            <FormControl>
                                <Input {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )} />
                ) : (
                    <span className="inline-block text-nowrap">{name}</span>
                )
            }
            toolsContent={
                isEditing ? (
                    <div className="flex gap-0.5">
                        <Button size="icon" variant="creative" type="submit">
                            <Check />
                        </Button>
                        <Button size="icon" variant="destructive" onClick={handleRejectEdit}>
                            <X />
                        </Button>
                    </div>
                ) : (
                    <Button size="icon" variant="ghost" disabled={preventEdit} onClick={handleEditClick}>
                        <Pencil />
                    </Button>
                )
            }
        />
    );
}

type RosterItem = {
    number: string;
    name: string;
    selected: boolean;
}

type RosterTableProps = {
    team: Team;
}

export const RosterTable = ({ team }: RosterTableProps) => {

    const [isEditing, setIsEditing] = useState(false);

    const { translate } = useI18n();

    const { setRoster } = useTeamApi();

    const handleEditStart = () => {
        setIsEditing(true);
        setTableRoster(v => v.map(s => ({ ...s, selected: false })));
    }

    const handleEditEnd = () => {
        setIsEditing(false);
    }

    const [tableRoster, setTableRoster] = useState<RosterItem[]>([]);

    useEffect(() => {
        setTableRoster(
            [...team.roster]
                .sort((a, b) => a.number.localeCompare(b.number))
                .map(skater => ({ ...skater, selected: false}))
        );
    }, [team]);

    const allSelected = useMemo(() => tableRoster.length > 0 && tableRoster.every(s => s.selected), [tableRoster]);

    const [handleSelectAllChange, setHandleSelectAllChange] = useState(false);
    const [shouldSelectAll, setShouldSelectAll] = useState(false);

    const handleSelectAllChanged = (checked: boolean) => {
        setShouldSelectAll(checked);
    }

    const handleSelectAllClicked = () => {
        setHandleSelectAllChange(true);
    }

    useEffect(() => {
        setShouldSelectAll(allSelected);
    }, [allSelected]);

    useEffect(() => {
        if(!handleSelectAllChange) {
            setShouldSelectAll(allSelected);
            return;
        }

        setHandleSelectAllChange(false);

        setTableRoster(v => v.map(s => ({ ...s, selected: shouldSelectAll })));

    }, [shouldSelectAll]);

    const selectedSkaters = tableRoster.filter(s => s.selected);

    const rosterItemToSkater = ({ number, name }: { number: string, name: string }) => 
        ({ number, name, pronouns: '', role: SkaterRole.Skater });

    const handleDeleteSelected = () => {
        setRoster(team.id, tableRoster.filter(s => !s.selected).map(rosterItemToSkater));
    }

    const handleEditSkater = (index: number, number: string, name: string) => {
        setRoster(
            team.id, 
            tableRoster
                .map((s, i) => i === index ? { number, name } : s)
                .map (rosterItemToSkater));
    }

    return (
        <Card className="pt-4">
            <CardContent className="flex flex-col">
                <div className="flex w-full justify-end">
                    <AlertDialog>
                        <AlertDialogTrigger asChild>
                            <Button variant="destructive" disabled={selectedSkaters.length < 1}>
                                <Trash />
                                { translate("RosterTable.DeleteSkater") }
                            </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                            <AlertDialogHeader>
                                <AlertDialogTitle>{ translate("RosterTable.DeleteSkaterDialog.Title") }</AlertDialogTitle>
                                <AlertDialogDescription>{ translate("RosterTable.DeleteSkaterDialog.Description") }</AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                                <AlertDialogCancel>{ translate("RosterTable.DeleteSkaterDialog.Cancel") }</AlertDialogCancel>
                                <AlertDialogAction className={buttonVariants({ variant: "destructive" })} onClick={handleDeleteSelected}>
                                    { translate("RosterTable.DeleteSkaterDialog.Confirm") }
                                </AlertDialogAction>
                            </AlertDialogFooter>
                        </AlertDialogContent>
                    </AlertDialog>
                </div>
                <RowLayout
                    className="pb-2"
                    selectContent={<Checkbox checked={allSelected} onCheckedChange={handleSelectAllChanged} onClick={handleSelectAllClicked} />}
                    numberContent={<span className="inline-block nowrap font-bold">{ translate("RosterTable.Number") }</span>}
                    nameContent={<span className="inline-block nowrap font-bold">{ translate("RosterTable.Name") }</span>}
                />
                <Separator />
                { tableRoster.map((skater, i) => (
                    <RosterTableRow 
                        {...skater} 
                        key={skater.number}
                        preventEdit={isEditing}
                        existingNumbers={tableRoster.map(s => s.number).filter(n => n !== skater.number)}
                        disableSelect={isEditing}
                        onSelectedChanged={selected => setTableRoster(tableRoster.map((s, i2) => i2 == i ? { ...s, selected } : s))}
                        onSkaterChanged={(number, name) => handleEditSkater(i, number, name)}
                        onEditStart={handleEditStart} 
                        onEditEnd={handleEditEnd} 
                    />
                ))}
                { tableRoster.length === 0 && (
                    <div className="w-full italic text-center">{ translate("RosterTable.NoSkaters") }</div>
                )}
            </CardContent>
        </Card>
    );
}
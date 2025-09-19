import { MouseEvent, ReactNode, useEffect, useMemo, useState } from "react";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger, Button, buttonVariants, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, Form, FormControl, FormField, FormItem, FormMessage, Input, Separator, Switch } from "@/components/ui"
import { useI18n, useIsMobile } from "@/hooks";
import { cn } from "@/lib/utils";
import { GameSkater } from "@/types";
import { Check, EllipsisVertical, Pencil, Trash, X } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

type RowLayoutProps = {
    numberContent?: ReactNode;
    nameContent?: ReactNode;
    skatingContent?: ReactNode;
    toolsContent?: ReactNode;
    contentWrapper?: (children: ReactNode, rowClassName: string) => ReactNode;
    className?: string;
    onClick?: () => void;
}

const RowLayout = ({ className, numberContent, nameContent, skatingContent, toolsContent, contentWrapper, onClick }: RowLayoutProps) => {

    const defaultContentWrapper = (children: ReactNode) => <>{children}</>;

    const rowClassName = cn("flex w-full flex-nowrap items-center", className);

    return (
        <div className={rowClassName} onClick={onClick}>
            {(contentWrapper ?? defaultContentWrapper)(
                <>
                    <div className="flex grow text-nowrap flex-nowrap gap-2 items-center">
                        <div className="w-1/5">{numberContent}</div>
                        <div className="w-3/5">{nameContent}</div>
                        <div className="w-1/5">{skatingContent}</div>
                    </div>
                    <div className="flex w-20 text-nowrap justify-end">
                        {toolsContent}
                    </div>
                </>
                , rowClassName)}
        </div>
    );
}

type RosterTableRowDropdownMenuProps = {
    disabled?: boolean;
    onDelete?: () => void;
}

const RosterTableRowDropdownMenu = ({ disabled, onDelete }: RosterTableRowDropdownMenuProps) => {

    const { translate } = useI18n();

    return (
        <AlertDialog>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button size="icon" variant="ghost" disabled={disabled}>
                        <EllipsisVertical />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                    <AlertDialogTrigger asChild>
                        <DropdownMenuItem>
                            <Trash />
                            {translate("GameRosterTable.Delete")}
                        </DropdownMenuItem>
                    </AlertDialogTrigger>
                </DropdownMenuContent>
            </DropdownMenu>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>{translate("GameRosterTable.DeleteTitle")}</AlertDialogTitle>
                    <AlertDialogDescription>{translate("GameRosterTable.DeleteDescription")}</AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel>{translate("GameRosterTable.DeleteCancel")}</AlertDialogCancel>
                    <AlertDialogAction className={buttonVariants({ variant: "destructive" })} onClick={onDelete}>
                        {translate("GameRosterTable.DeleteConfirm")}
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    )
}

export const useRosterTableInputSchema = (existingNumbers: string[]) => {
    const { translate } = useI18n();

    const notExistingNumberRegex = useMemo(() => new RegExp(`^(?!(${existingNumbers.join('|')})$).*$`), [existingNumbers]);

    return z.object({
        number: z.string().min(1, {
            message: translate('GameRosterInput.NumberMissing'),
        }).regex(/^\d{1,4}$/, {
            message: translate('GameRosterInput.NumberInvalid'),
        }).regex(notExistingNumberRegex, {
            message: translate("GameRosterInput.NumberExists"),
        }),
        name: z.string(),
        isSkating: z.boolean(),
    });
}


type RosterTableRowProps = {
    number: string;
    name: string;
    isSkating: boolean;
    preventEdit?: boolean;
    selected?: boolean;
    existingNumbers: string[];
    disableSelect?: boolean;
    onEditStart?: () => void;
    onEditEnd?: () => void;
    onSelectedChanged?: (selected: boolean) => void;
    onSkaterChanged?: (skater: GameSkater) => void;
    onSkaterDeleted?: () => void;
}

const RosterTableRow = ({ number, name, isSkating, preventEdit, selected, existingNumbers, onEditStart, onEditEnd, onSkaterChanged, onSkaterDeleted }: RosterTableRowProps) => {

    const [isEditing, setIsEditing] = useState(false);

    const formSchema = useRosterTableInputSchema(existingNumbers);

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            number: number,
            name: name,
            isSkating: isSkating,
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

    const handleAcceptEdit = (skater: GameSkater) => {
        setIsEditing(false);
        onEditEnd?.();
        onSkaterChanged?.(skater);
        form.reset();
    }

    const handleRejectEdit = () => {
        setIsEditing(false);
        onEditEnd?.();
        form.reset();
    }

    useEffect(() => {
        const { isSkating } = form.getValues();

        onSkaterChanged?.({ number, name, isSkating });
    }, [form.watch('isSkating')]);

    return (
        <RowLayout
            className={cn("py-2", selected && "bg-accent")}
            contentWrapper={(children, rowClassName) => 
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(handleAcceptEdit)} className={cn(rowClassName, "p-0")}>
                        {children}
                    </form>
                </Form>}
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
            skatingContent={
                <FormField control={form.control} name="isSkating" render={({field: {value, onChange, ...field}}) => (
                    <Switch checked={value} onCheckedChange={onChange} {...field} />
                )} />
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
                    <div className="flex gap-0.5">
                        <Button size="icon" variant="ghost" disabled={preventEdit} onClick={handleEditClick}>
                            <Pencil />
                        </Button>
                        <RosterTableRowDropdownMenu onDelete={onSkaterDeleted} />
                    </div>
                )
            }
        />
    );
}

type RosterTableProps = {
    roster: GameSkater[];
    className?: string;
    onSkaterChanged?: (rosterIndex: number, skater: GameSkater) => void;
    onSkaterDeleted?: (rosterIndex: number) => void;
}

export const GameRosterTable = ({ roster, className, onSkaterChanged, onSkaterDeleted }: RosterTableProps) => {

    const [isEditing, setIsEditing] = useState(false);
    const { translate } = useI18n();
    const isMobile = useIsMobile();

    const handleEditStart = () => {
        setIsEditing(true);
    }

    const handleEditEnd = () => {
        setIsEditing(false);
    }

    return (
        <div className={cn("pt-4", className)}>
            <div className="flex flex-col">
                <RowLayout
                    className="py-2"
                    numberContent={
                        <span className="inline-block nowrap font-bold">
                            { isMobile ? translate("RosterTable.NumberSymbol") : translate("RosterTable.Number") }
                        </span>}
                    nameContent={<span className="inline-block nowrap font-bold">{ translate("RosterTable.Name") }</span>}
                    skatingContent={<span className="inline-block nowrap font-bold">{ translate("GameRosterTable.Skating") }</span>}
                />
                <Separator />
                { roster.map((skater, i) => (
                    <RosterTableRow 
                        {...skater} 
                        key={skater.number}
                        preventEdit={isEditing}
                        existingNumbers={roster.map(s => s.number).filter(n => n !== skater.number)}
                        disableSelect={isEditing}
                        onSkaterChanged={skater => onSkaterChanged?.(i, skater)}
                        onSkaterDeleted={() => onSkaterDeleted?.(i)}
                        onEditStart={handleEditStart} 
                        onEditEnd={handleEditEnd} 
                    />
                ))}
                { roster.length === 0 && (
                    <div className="w-full italic text-center">{ translate("RosterTable.NoSkaters") }</div>
                )}
            </div>
        </div>
    );
}
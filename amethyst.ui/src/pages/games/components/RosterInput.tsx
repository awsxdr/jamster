import { Button, Form, FormControl, FormField, FormItem, FormLabel, FormMessage, Input } from "@/components/ui";
import { z } from 'zod';
import { zodResolver } from "@hookform/resolvers/zod"
import { Plus } from "lucide-react";
import { useI18n } from "@/hooks";
import { useMemo, ClipboardEvent } from "react";
import { useForm } from "react-hook-form";
import { GameSkater } from "@/types";

export const useRosterInputSchema = (existingNumbers: string[]) => {
    const { translate } = useI18n();

    const notExistingNumberRegex = useMemo(() => new RegExp(`^(?!(${existingNumbers.join('|')})$).*$`), [existingNumbers]);

    return z.object({
        number: z.string().min(1, {
            message: translate('RosterInput.NumberMissing'),
        }).regex(/^\d{1,4}$/, {
            message: translate('RosterInput.NumberInvalid'),
        }).regex(notExistingNumberRegex, {
            message: translate("RosterInput.NumberExists"),
        }),
        name: z.string(),
    });
}

type RosterRowProps = {
    existingNumbers: string[];
    onSkatersAdded?: (skaters: GameSkater[]) => void;
}

export const RosterInput = ({ existingNumbers, onSkatersAdded }: RosterRowProps) => {

    const { translate } = useI18n();

    const formSchema = useRosterInputSchema(existingNumbers);

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: '',
            number: '',
        }
    });
    
    const handleSubmit = (skater: { name: string, number: string }) => {
        form.setFocus('number');
        onSkatersAdded?.([{ ...skater, isSkating: true }]);
        form.reset();
    }

    function handlePaste(event: ClipboardEvent<HTMLInputElement>): void {
        /* Regex searches for either: 
            * 1-4 digits with an optional trailing * followed by whitespace followed by at least 1 character that isn't a newline
            * Any number of characters followed by one non-whitespace character followed by whitespace followed by 1-4 digits with an optional trailing *
        */
        const rosterRegex = /(?<skater>(?:(?<number>\d{1,4})(?<ns>\*)?[\s\r\n]+(?<name>[^\r\n]+))|(?:(?<name2>.*[^\s\r\n])[\s\r\n]+(?<number2>\d{1,4})(?<ns2>\*)?))/gm;

        const pastedText = event.clipboardData.getData("text/plain");

        if(!pastedText.includes("\n")) {
            return;
        }

        const matches = [...pastedText.matchAll(rosterRegex)];

        const pastedSkaters = matches.map(match => {
                const name = match.groups?.["name"] || match.groups?.["name2"];
                const number = match.groups?.["number"] || match.groups?.["number2"];

                return name && number
                    ? { number, name, isSkating: true } as GameSkater
                    : undefined;
            }).filter(skater =>
                skater !== undefined
            );

        if(pastedSkaters.length == 0) {
            return;
        }

        event.preventDefault();

        onSkatersAdded?.(pastedSkaters);
    }

    return (
        <div className="flex w-full">
            <Form {...form}>
                <form onSubmit={form.handleSubmit(handleSubmit)} className="flex flex-col w-full">
                    <div className="flex gap-2 w-full">
                        <div className="flex gap-2 w-full">
                            <FormField control={form.control} name="number" render={({field}) => (
                                <FormItem className="w-1/3">
                                    <FormLabel>{ translate("RosterInput.Number") }</FormLabel>
                                    <FormControl>
                                        <Input {...field} onPaste={handlePaste} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                            <FormField control={form.control} name="name" render={({field}) => (
                                <FormItem className="w-2/3">
                                    <FormLabel>{ translate("RosterInput.Name") }</FormLabel>
                                    <FormControl>
                                        <Input {...field} onPaste={handlePaste} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                        </div>
                        <FormItem>
                            <div className="h-6"></div>
                            <Button type="submit" variant="creative"><Plus /> { translate("RosterInput.AddSkater") }</Button>
                        </FormItem>
                    </div>
                </form>
            </Form>
        </div>
    )
}
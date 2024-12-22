import { Button, Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage, Input } from "@/components/ui";
import { z } from 'zod';
import { zodResolver } from "@hookform/resolvers/zod"
import { Plus } from "lucide-react";
import { useI18n } from "@/hooks";
import { useMemo } from "react";
import { useForm } from "react-hook-form";

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
    onSkaterAdded?: (number: string, name: string) => void;
}

export const RosterInput = ({ existingNumbers, onSkaterAdded }: RosterRowProps) => {

    const { translate } = useI18n();

    const formSchema = useRosterInputSchema(existingNumbers);

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: '',
            number: '',
        }
    });
    
    const handleSubmit = ({number, name}: { number: string, name: string }) => {
        form.setFocus('number');
        onSkaterAdded?.(number, name);
        form.reset();
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
                                        <Input {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )} />
                            <FormField control={form.control} name="name" render={({field}) => (
                                <FormItem className="w-2/3">
                                    <FormLabel>{ translate("RosterInput.Name") }</FormLabel>
                                    <FormControl>
                                        <Input {...field} />
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
                    <FormDescription>
                        { translate("RosterInput.PasteRosterTip") }
                    </FormDescription>
                </form>
            </Form>
        </div>
    )
}
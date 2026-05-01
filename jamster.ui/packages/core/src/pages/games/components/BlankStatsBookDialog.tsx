import { Button, Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Form, FormControl, FormField, FormItem, FormMessage, Input } from "@/components/ui";
import { useBlankStatsBookApi, useI18n } from "@/hooks";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { PropsWithChildren, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

type BlankStatsBookDialogContainerProps = {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
}

export const BlankStatsBookDialogContainer = ({ children, ...props }: PropsWithChildren<BlankStatsBookDialogContainerProps>) => (
    <Dialog {...props}>
        {children}
    </Dialog>
);

export const BlankStatsBookDialogTrigger = ({ children }: PropsWithChildren) => (
    <DialogTrigger asChild>
        {children}
    </DialogTrigger>
);

const useUploadStatsBookSchema = () => {
    const { translate } = useI18n({ prefix: "GameEdit.BlankStatsBookDialog." });

    return z.object({
        statsBookFile: z
            .instanceof(File, { message: translate("FileRequired") })
            .refine(file => 
                file.type === "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            { message: translate("InvalidFileFormat")}),
    });
}

type BlankStatsBookDialogProps = {
    onUploadSuccessful?: () => void;
    onCancel?: () => void;
}

export const BlankStatsBookDialog = ({ onUploadSuccessful, onCancel }: BlankStatsBookDialogProps) => {

    const { translate } = useI18n({ prefix: "GameEdit.BlankStatsBookDialog." });
    const [isUploading, setIsUploading] = useState(false);
    const { setBlankStatsBook } = useBlankStatsBookApi();

    const formSchema = useUploadStatsBookSchema();

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            statsBookFile: undefined,
        },
    });

    const handleSubmit = async ({ statsBookFile }: { statsBookFile: File }) => {
        setIsUploading(true);

        try {
            await setBlankStatsBook(statsBookFile);
            onUploadSuccessful?.();
        } catch {
            form.setError("statsBookFile", { message: translate("InvalidStatsBook") });
        } finally {
            setIsUploading(false);
        }
    }

    const handleCancel = () => {
        form.reset();
        setIsUploading(false);
        onCancel?.();
    }

    return (
        <DialogContent>
            <Form {...form}>
                <form onSubmit={form.handleSubmit(handleSubmit)}>
                    <DialogHeader>
                        <DialogTitle>{translate("Title")}</DialogTitle>
                        <DialogDescription>
                            {translate("Description.Start")}
                            <a 
                                href="https://resources.wftda.org/competition/statsbook/" 
                                target="_blank"
                                rel="noreferrer"
                                className="underline underline-offset-4 text-primary"
                            >
                                {translate("Description.Link")}
                            </a>
                            {translate("Description.End")}
                        </DialogDescription>
                    </DialogHeader>
                    <div className="pt-4">
                        <FormField control={form.control} name="statsBookFile" render={({ field: { value: _, onChange, ...fieldProps }}) => (
                            <FormItem>
                                <FormControl>
                                    <Input 
                                        {...fieldProps} 
                                        type="file" 
                                        accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" 
                                        onChange={event => onChange(event.target.files && event.target.files[0])}
                                    />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )} />
                    </div>
                    <DialogFooter>
                        <div className="flex flex-row-reverse gap-2">
                            <Button 
                                variant="default" 
                                type="submit"
                                className="mt-4" 
                                disabled={isUploading}
                            >
                                {isUploading && <Loader2 className="animate-spin" />}
                                {translate("Upload")}
                            </Button>
                            <Button
                                variant="outline"
                                className="mt-4"
                                disabled={isUploading}
                                onClick={handleCancel}
                            >
                                {translate("Cancel")}
                            </Button>
                        </div>
                    </DialogFooter>
                </form>
            </Form>
        </DialogContent>
    );
}
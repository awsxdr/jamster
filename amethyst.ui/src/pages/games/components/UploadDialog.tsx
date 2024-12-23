import { Button, Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger, Form, FormControl, FormField, FormItem, FormMessage, Input } from "@/components/ui";
import { useI18n } from "@/hooks";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { PropsWithChildren, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

type UploadDialogContainerProps = {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
}

export const UploadDialogContainer = ({ children, ...props }: PropsWithChildren<UploadDialogContainerProps>) => {
    return (
        <Dialog {...props}>
            {children}
        </Dialog>
    );
}

export const UploadDialogTrigger = ({children}: PropsWithChildren) => {
    return (
        <DialogTrigger asChild>
            {children}
        </DialogTrigger>
    )
}

const useUploadGameSchema = () => {
    const { translate } = useI18n();

    return z.object({
        statsBookFile: z
            .instanceof(File, { message: translate("UploadDialog.FileRequired") })
            .refine(file => 
                file.type === "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                { message: translate("UploadDialog.InvalidFileFormat")}),
    });
}

type UploadDialogProps = {
    onGameUploaded?: (file: File) => Promise<void>;
    onCancelled?: () => void;
}

export const UploadDialog = ({ onGameUploaded, onCancelled }: UploadDialogProps) => {

    const { translate } = useI18n();
    const [isUploading, setIsUploading] = useState(false);

    const formSchema = useUploadGameSchema();

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            statsBookFile: undefined,
        },
    });

    const handleSubmit = ({ statsBookFile }: { statsBookFile: File }) => {
        setIsUploading(true);
        (onGameUploaded?.(statsBookFile) ?? Promise.resolve()).then(() => {
            form.reset();
            setIsUploading(false);
        });
    }

    const handleCancel = () => {
        form.reset();
        setIsUploading(false);
        onCancelled?.();
    }

    return (
        <DialogContent>
            <Form {...form}>
                <form onSubmit={form.handleSubmit(handleSubmit)}>
                    <DialogHeader>
                        <DialogTitle>{translate("UploadDialog.Title")}</DialogTitle>
                        <DialogDescription>{translate("UploadDialog.Description")}</DialogDescription>
                    </DialogHeader>
                    <div>
                        <FormField control={form.control} name="statsBookFile" render={({ field: { value, onChange, ...fieldProps }}) => (
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
                                {translate("UploadDialog.Upload")}
                            </Button>
                            <Button
                                variant="outline"
                                className="mt-4"
                                disabled={isUploading}
                                onClick={handleCancel}
                            >
                                {translate("UploadDialog.Cancel")}
                            </Button>
                        </div>
                    </DialogFooter>
                </form>
            </Form>
        </DialogContent>
    );
}
import { cn } from "@/lib/utils";
import { Button, TooltipProvider } from "./ui";
import { ReactNode } from "react";
import { TooltipButton } from "./TooltipButton";

export type RadioItem<TValue> = { 
    value: TValue;
    name: string;
};

export type TooltipRadioItem<TValue> = RadioItem<TValue> & { 
    description: ReactNode;
};

type RadioButtonGroupProps<TValue> = {
    items: (RadioItem<TValue> | TooltipRadioItem<TValue>)[];
    value?: TValue;
    variant?: "default" | "secondary" | "ghost";
    size?: 'lg' | 'sm' | 'default';
    rowClassName?: string;
    buttonClassName?: string;
    disabled?: boolean;
    toggle?: boolean;
    onItemSelected?: (value: TValue) => void;
    onItemDeselected?: () => void;
}

export const RadioButtonGroup = <TValue,>({ items, value, variant, size, rowClassName, buttonClassName, disabled, toggle, onItemSelected, onItemDeselected }: RadioButtonGroupProps<TValue>) => {
    
    const handleButtonClick = (buttonValue: TValue) => {
        if(buttonValue === value && toggle) {
            onItemDeselected?.();
        } else {
            onItemSelected?.(buttonValue);
        }
    }

    return (
        <div className={cn("flex flex-wrap gap-2", rowClassName)}>
            <TooltipProvider>
            {
                items.map((item, i) => (item as TooltipRadioItem<TValue>)?.description ? (
                    <TooltipButton 
                        description={(item as TooltipRadioItem<TValue>).description}
                        size={size}
                        variant={variant ?? "secondary"}
                        className={cn("border-2", value === item.value ? "border-primary" : "", buttonClassName)}
                        disabled={disabled}
                        key={i}
                        onClick={() => handleButtonClick(item.value)}
                    >
                        {item.name}
                    </TooltipButton>
                ) : (
                    <Button 
                        size={size}
                        variant={variant ?? "secondary"}
                        className={cn("border-2", value === item.value ? "border-primary" : "", buttonClassName)}
                        disabled={disabled}
                        key={i}
                        onClick={() => handleButtonClick(item.value)}
                    >
                        {item.name}
                    </Button>
                ))
            }
            </TooltipProvider>
        </div>
    )
}
import { cn } from "@/lib/utils";
import { Button } from "./ui";

export type RadioItem<TValue> = { value: TValue, name: string };

type RadioButtonGroupProps<TValue> = {
    items: RadioItem<TValue>[];
    value?: TValue;
    size?: 'lg' | 'sm' | 'default';
    rowClassName?: string;
    buttonClassName?: string;
    disabled?: boolean;
    toggle?: boolean;
    onItemSelected?: (value: TValue) => void;
    onItemDeselected?: () => void;
}

export const RadioButtonGroup = <TValue,>({ items, value, size, rowClassName, buttonClassName, disabled, toggle, onItemSelected, onItemDeselected }: RadioButtonGroupProps<TValue>) => {
    
    const handleButtonClick = (buttonValue: TValue) => {
        if(buttonValue === value && toggle) {
            onItemDeselected?.();
        } else {
            onItemSelected?.(buttonValue);
        }
    }

    return (
        <div className={cn("flex flex-wrap gap-2", rowClassName)}>
            {
                items.map(item => (
                    <Button 
                        size={size}
                        variant="secondary"
                        className={cn("border-2", value === item.value ? "border-primary" : "", buttonClassName)}
                        disabled={disabled}
                        onClick={() => handleButtonClick(item.value)}
                    >
                        {item.name}
                    </Button>
                ))
            }
        </div>
    )
}
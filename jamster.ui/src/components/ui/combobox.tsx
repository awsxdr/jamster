import { useState } from "react"
import { Popover, PopoverContent, PopoverTrigger } from "./popover";
import { Button } from "./button";
import { Check, ChevronsUpDown } from "lucide-react";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "./command";
import { cn } from "@/lib/utils";

export type ComboBoxProps = {
    value: string;
    items: ComboBoxItem[];
    placeholder: string;
    className?: string;
    dropdownClassName?: string;
    disabled?: boolean;
    hideSearch?: boolean;
    onValueChanged: (value: string) => void;
};

export type ComboBoxItem = {
    text: string,
    value: string,
};

export const ComboBox = ({ value, onValueChanged, items, placeholder, className, dropdownClassName, disabled, hideSearch }: ComboBoxProps) => {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <Popover open={isOpen} onOpenChange={setIsOpen} modal>
            <PopoverTrigger asChild disabled={disabled}>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={isOpen}
                    className={cn("justify-between", className)}
                >
                    <span className="overflow-hidden text-ellipsis">
                        {
                            value
                                ? items.find(item => item.value === value)?.text
                                : placeholder
                        }
                    </span>
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className={cn("p-0", dropdownClassName)}>
                <Command>
                    { !hideSearch && <CommandInput placeholder={placeholder} /> }
                    <CommandList>
                        <CommandEmpty>Nothing found.</CommandEmpty>
                        <CommandGroup>
                            {
                                items.map(item => (
                                    <CommandItem
                                        key={item.value}
                                        value={item.value}
                                        onSelect={currentValue => {
                                            onValueChanged(currentValue === value ? "" : currentValue);
                                            setIsOpen(false);
                                        }}
                                    >
                                        <Check
                                            className={cn(
                                                "mr-2 h-4 w-4",
                                                value === item.value ? "opacity-100" : "opacity-0"
                                            )}
                                        />
                                        {item.text}
                                    </CommandItem>
                                ))
                            }
                        </CommandGroup>
                    </CommandList>
                </Command>
            </PopoverContent>
        </Popover>
    )
}
import { useState } from "react"
import { Popover, PopoverContent, PopoverTrigger } from "./popover";
import { Button } from "./button";
import { Check, ChevronsUpDown } from "lucide-react";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "./command";
import { cn } from "@/lib/utils";

export type ComboBoxProps = {
    value: string,
    onValueChanged: (value: string) => void,
    items: ComboBoxItem[],
    placeholder: string,
};

export type ComboBoxItem = {
    text: string,
    value: string,
};

export const ComboBox = ({ value, onValueChanged, items, placeholder }: ComboBoxProps) => {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <Popover open={isOpen} onOpenChange={setIsOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={isOpen}
                    className="sm:w-[200px] md:w-[250px] lg:w-[400px] justify-between"
                >
                    {
                        value
                            ? items.find(item => item.value === value)?.text
                            : placeholder
                    }
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[200px] p-0">
                <Command>
                    <CommandInput placeholder={placeholder} />
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
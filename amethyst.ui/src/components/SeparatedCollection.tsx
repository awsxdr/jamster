import { ReactNode } from "react";
import { Separator } from "./ui";

type SeparatedCollectionProps = {
    children: (ReactNode | undefined)[]
}

export const SeparatedCollection = ({ children }: SeparatedCollectionProps) => {
    return (
        <>
        {
            children.reduce((current, child) => 
                <>{ current }{ child && current && <Separator /> }{ child }</>
            , undefined)            
        }
        </>
    );
}
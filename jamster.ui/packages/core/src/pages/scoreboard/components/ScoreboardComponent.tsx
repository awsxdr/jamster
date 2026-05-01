import { CSSProperties, PropsWithChildren } from 'react';
import { cn } from '@/lib/utils';
import { ScaledText } from '@/components/ScaledText';

type ScoreboardComponentProps = {
    header?: string,
    style?: CSSProperties,
    className?: string,
    headerClassName?: string,
}

export const ScoreboardComponent = ({ header, style, className, headerClassName, children }: PropsWithChildren<ScoreboardComponentProps>) => {
    return (
        <div 
            style={style} 
            className={cn(
                "flex flex-col bg-white content-stretch rounded-md sm:rounded-lg md:rounded-xl xl:rounded-3xl",
                className
            )}
        >
            { header && (
                <ScaledText 
                    className={cn(
                        "bg-gray-300 text-center font-extrabold leading-[130%]",
                        "rounded-t-md sm:rounded-t-lg md:rounded-t-xl xl:rounded-t-3xl",
                        "w-full h-[40%] p-0",
                        headerClassName,
                    )}
                    text={header}
                />
            )}
            {children}
        </div>
    );
}
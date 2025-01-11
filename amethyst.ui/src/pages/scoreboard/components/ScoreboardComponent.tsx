import { CSSProperties, PropsWithChildren } from 'react';
import { cn } from '@/lib/utils';

type ScoreboardComponentProps = {
    header?: string,
    style?: CSSProperties,
    className?: string,
    headerClassName?: string,
}

export const ScoreboardComponent = ({ header, style, className, headerClassName, children }: PropsWithChildren<ScoreboardComponentProps>) => {
    return (
        <div style={style} className={cn("flex flex-col bg-white content-stretch rounded-md sm:rounded-lg md:rounded-xl xl:rounded-3xl", className)}>
            { header && (
                <div 
                    className={cn(
                        "bg-gray-300 text-center font-bold p-2",
                        "rounded-t-md sm:rounded-t-lg md:rounded-t-xl xl:rounded-t-3xl",
                        "text-xl sm:text-2xl md:text-3xl lg:text-5xl",
                        headerClassName,
                    )}
                >
                    {header}
                </div> 
            )}
            {children}
        </div>
    );
}
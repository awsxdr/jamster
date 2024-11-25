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
        <div style={style} className={cn(className, "flex flex-col bg-white content-stretch rounded-3xl" )}>
            { header && <div className={cn("bg-gray-300 rounded-t-3xl text-center text-5xl font-bold p-4", headerClassName)}>{header}</div> }
            {children}
        </div>
    );
}
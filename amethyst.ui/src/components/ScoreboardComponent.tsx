import { CSSProperties, PropsWithChildren } from 'react';
import styles from './ScoreboardComponent.module.css';
import { cn } from '@/lib/utils';

type ScoreboardComponentProps = {
    style?: CSSProperties,
    className?: string,
}

export const ScoreboardComponent = ({ style, className, children }: PropsWithChildren<ScoreboardComponentProps>) => {
    return (
        <div style={style} className={cn(className, styles.component)}>
            {children}
        </div>
    );
}
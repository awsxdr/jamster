import { CSSProperties, PropsWithChildren } from 'react';
import styles from './ScoreboardComponent.module.css';
import { cn } from '@/lib/utils';

type ScoreboardComponentProps = {
    header?: string,
    style?: CSSProperties,
    className?: string,
}

export const ScoreboardComponent = ({ header, style, className, children }: PropsWithChildren<ScoreboardComponentProps>) => {
    return (
        <div style={style} className={cn(className, styles.component )}>
            { header && <div className={styles.header}>{header}</div> }
            {children}
        </div>
    );
}
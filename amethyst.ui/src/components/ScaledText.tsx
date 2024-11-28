import { cn } from "@/lib/utils";
import { CSSProperties, useCallback, useEffect, useMemo, useRef, useState } from "react";

type ScaledTextProps = {
    text: string;
    className?: string;
    style?: CSSProperties;
};

export const ScaledText = ({ text, className, style }: ScaledTextProps) => {

    const [fontSize, setFontSize] = useState(0);
    const canvas = useMemo(() => document.createElement("canvas"), []);
    const [controlSize, setControlSize] = useState({width: 0, height: 0 });

    const spanRef = useRef<HTMLSpanElement>(null);

    const font = useMemo(() => {
        if (spanRef.current === null) {
            return null;
        }

        const computedStyle = window.getComputedStyle(spanRef.current, null);
        return {
            weight: computedStyle.fontWeight,
            family: computedStyle.fontFamily,
        };
    }, [spanRef, spanRef.current]);

    const measureText = useCallback((text: string, font: string) => {
        const context = canvas.getContext("2d")!;
        context.font = font;
        const metrics = context.measureText(text);
        
        return { width: metrics.width, height: metrics.fontBoundingBoxAscent + metrics.fontBoundingBoxDescent };
    }, [canvas]);

    const updateSize = useCallback(() => {
        if(!spanRef.current) {
            return;
        }

        const computedStyle = window.getComputedStyle(spanRef.current);
        const width = spanRef.current.clientWidth - parseFloat(computedStyle.paddingLeft) - parseFloat(computedStyle.paddingRight);
        const height = spanRef.current.clientHeight - parseFloat(computedStyle.paddingTop) - parseFloat(computedStyle.paddingBottom);

        setControlSize({ width, height });
    }, [spanRef, setControlSize]);

    useEffect(() => {
        if(!spanRef.current) {
            return;
        }

        const resizeObserver = new ResizeObserver(() => {
            updateSize();
        });
        resizeObserver.observe(spanRef.current);
    
        updateSize();

        return () => resizeObserver.disconnect();
    }, [updateSize]);


    useEffect(() => {
        updateSize()
    }, [text, updateSize]);

    useEffect(() => {
        if(!font) {
            return;
        }

        let size = 200;
        let change = size / 2;

        while(change >= 1) {
            const textSize = measureText(text, `${font.weight} ${size}px ${font.family}`);

            if(textSize.width > controlSize.width || textSize.height > controlSize.height ) {
                size -= change;
                change /= 2;
                continue;
            }

            if (Math.abs(textSize.width - controlSize.width) <= 1 || Math.abs(textSize.height - controlSize.height) <= 1) {
                break;
            }

            size += change;
            change /= 2;
        }

        setFontSize(Math.round(size));

    }, [measureText, text, font, setFontSize, controlSize]);

    return (
        <span 
            ref={spanRef} 
            className={cn('overflow-hidden', className)} 
            style={{ ...style, fontSize: `${fontSize}px`}}
        >
            {text}
        </span>
    );
}
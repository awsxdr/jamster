import { useCallback, useEffect, useMemo, useRef, useState } from "react";

type ScaledTextProps = {
    text: string,
};

export const ScaledText = ({ text }: ScaledTextProps) => {

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

    useEffect(() => {
        if(!spanRef.current) {
            return;
        }

        window.addEventListener('resize', () => {
            if(!spanRef.current) {
                return;
            }

            setControlSize({ width: spanRef.current.clientWidth, height: spanRef.current.clientHeight })
        });
        setControlSize({ width: spanRef.current.clientWidth, height: spanRef.current.clientHeight })
    }, [spanRef.current, setControlSize]);

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

            if (textSize.width === controlSize.width || textSize.height === controlSize.height) {
                break;
            }

            size += change;
            change /= 2;
        }

        setFontSize(size);

    }, [measureText, text, font, setFontSize, controlSize]);

    return (
        <span ref={spanRef} style={{ display: 'inline-block', width: '100%', height: '100%', fontSize: `${fontSize}px` }}>
            {text}
        </span>
    );
}
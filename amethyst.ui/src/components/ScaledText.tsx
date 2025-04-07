import { cn } from "@/lib/utils";
import { StringMap } from "@/types";
import { CSSProperties, useCallback, useEffect, useMemo, useRef, useState } from "react";

type ScaledTextProps = {
    text: string;
    className?: string;
    style?: CSSProperties;
    scale?: number;
};

type Measure = {
    width: number;
    height: number;
}

export const ScaledText = ({ text, className, style, scale }: ScaledTextProps) => {

    const [fontSize, setFontSize] = useState(0);
    const canvas = useMemo(() => document.createElement("canvas"), []);
    const [controlSize, setControlSize] = useState({width: 0, height: 0 });
    const [aspectRatio, setAspectRatio] = useState(0);
    const [textChunks, setTextChunks] = useState<string[]>([]);

    const spanRef = useRef<HTMLDivElement>(null);

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

    const measureText = useCallback((text: string, font: string): Measure => {
        const context = canvas.getContext("2d")!;
        context.font = font;
        const metrics = context.measureText(text);
        
        return { width: metrics.width, height: metrics.fontBoundingBoxAscent + metrics.fontBoundingBoxDescent };
    }, [canvas]);

    const measureTextChunks = (font: string): Measure => {
        const context = canvas.getContext("2d");

        if(!context || !textChunks) {
            return { width: 0, height: 0 };
        }

        context.font = font;
        const metrics = textChunks.map(t => context.measureText(t));

        return {
            width: Math.max(...metrics.map(m => m.width)),
            height: metrics.reduce((t, m) => t + m.fontBoundingBoxAscent + m.fontBoundingBoxDescent, 0),
        };
    }

    const updateSize = useCallback(() => {
        if(!spanRef.current) {
            return;
        }

        const computedStyle = window.getComputedStyle(spanRef.current);
        const width = (spanRef.current.clientWidth - parseFloat(computedStyle.paddingLeft) - parseFloat(computedStyle.paddingRight)) * 0.95;
        const height = (spanRef.current.clientHeight - parseFloat(computedStyle.paddingTop) - parseFloat(computedStyle.paddingBottom)) * 0.95;

        setControlSize({ width, height });
        setAspectRatio(width / height);
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
    }, [textChunks, updateSize]);

    useEffect(() => {
        if(!font) {
            return;
        }

        const words = text.split(' ').filter(w => !!w);

        const getIterations = (items: string[]) => {
            if (items.length === 0) {
                return [[]];
            }

            const result: string[][] = [];

            for(let i = 0; i < items.length; ++i) {
                result.push(...getIterations(items.slice(i + 1)).map(r => [items.slice(0, i + 1).join(' '), ...r]));
            }

            return result;
        }

        const iterations = getIterations(words);

        const uniqueIterations = [...new Set(iterations.flatMap(i => i))];
        const measures: StringMap<Measure> = uniqueIterations.reduce((c, t) => ({ ...c, [t]: measureText(t, `${font.weight} 40px ${font.family}`) }), {});
        
        const iterationAspectRatios = iterations
            .map(i => i.map(t => measures[t]!))
            .map(m => ({
                width: Math.max(...m.map(x => x.width)),
                height: m.reduce((t, x) => t + x.height, 0),
            }))
            .map(m => m.width / m.height)
            .sort();

        const iterationAspectRatioDifferences = iterationAspectRatios.map(a => Math.abs(a - aspectRatio));
        const minDifference = Math.min(...iterationAspectRatioDifferences);
        const bestFitIndex = iterationAspectRatioDifferences.indexOf(minDifference);

        setTextChunks(iterations[bestFitIndex] ?? iterations.reverse()[0] ?? []);

    }, [font, text, aspectRatio])

    useEffect(() => {
        if(!font || !textChunks || textChunks.length === 0) {
            return;
        }

        let size = 200;
        let change = size / 2;
        const controlScale = scale ?? 1;
        const scaledControl = { width: controlSize.width * controlScale, height: controlSize.height * controlScale };

        while(change >= 1) {
            const textSize = measureTextChunks(`${font.weight} ${size}px ${font.family}`);

            if(textSize.width > scaledControl.width || textSize.height > scaledControl.height ) {
                size -= change;
                change /= 2;
                continue;
            }

            if (Math.abs(textSize.width - scaledControl.width) <= 1 || Math.abs(textSize.height - scaledControl.height) <= 1) {
                break;
            }

            size += change;
            change /= 2;
        }

        setFontSize(Math.round(size));

    }, [measureText, textChunks, font, setFontSize, controlSize]);

    return (
        <div 
            ref={spanRef} 
            className={cn('overflow-hidden', className)} 
            style={{ ...style, fontSize: `${fontSize}px`}}
        >
            {textChunks.map((t, i) => <p key={i}>{t}</p>)}
        </div>
    );
}
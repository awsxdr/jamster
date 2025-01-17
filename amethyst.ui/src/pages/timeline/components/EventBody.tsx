import { ReactNode } from "react";

type EventBodyProps = {
    body: object;
}

type ValueEditProps = {
    value: number;
}

const ValueEdit = ({ value }: ValueEditProps) => {
    return (
        <span className="transition-colors hover:bg-accent py-1 px-0.5">
            {value.toString()}
        </span>
    )
}

export const EventBody = ({ body }: EventBodyProps) => {

    const joinList = (list: ReactNode[]) => {
        const items = list.flatMap(o => [o, <>, </>]);
        return items.slice(0, -1);
    }

    const renderValue = (value: object): ReactNode => {

        if(value === null) {
            return typeof value;
        }

        switch(typeof value) {
            case "number":
                case "boolean":
                    return <ValueEdit value={value} />;

            case "string":
                return <span>"<ValueEdit value={value} />"</span>;

            case "object":
                return Array.isArray(value)
                    ? renderArray(value)
                    : renderObject(value);
            
            case "undefined":
                return <span>undefined</span>;

            default:
                return (value as object).toString();
        }
    };

    const renderArray = (value: object[]) => 
        <span>[{joinList(value.map(renderObject))}]</span>

    const renderObject = (value: object) =>
        <span>{"{"}{joinList(Object.keys(value).map(k => <span>"{k}": {renderValue(value[k as keyof typeof value])}</span>))}{"}"}</span>

    return (
        <div className="border font-mono text-sm p-1 rounded">
            {renderObject(body)}
        </div>
    )
}
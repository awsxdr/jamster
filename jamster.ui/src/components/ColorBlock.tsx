type ColorBlockProps = {
    color: string;
}

export const ColorBlock = ({ color }: ColorBlockProps) => (
    <span 
        className="display-inline-block w-5 h-5 self-center" 
        style={{ backgroundColor: color}}
    >
    </span>
)
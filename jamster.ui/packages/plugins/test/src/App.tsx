import { useGamesList } from '@awsxdr/jamster.ui.shared';

export const App = () => {
    const games = useGamesList();

    return (
        <div>
            <div>{games.length} games found</div>
            { games.map((g, i) => (
                <div key={i}>
                    {i}. {g.name} ({g.id})
                </div>
            ))}
        </div>
    )
}
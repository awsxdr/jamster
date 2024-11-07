import { useTeamList } from "@/hooks/TeamsHook";

export const TeamManagement = () => {

    const teams = useTeamList();

    return (
        <>{ teams.map((t, i) => (<div key={i}>{t.names["default"]}</div>)) }</>
    );
}
import { useTeamList } from "@/hooks/TeamsHook";

export const TeamManagement = () => {

    const teams = useTeamList();

    return (
        <>{ teams.forEach(t => t.names) }</>
    );
}
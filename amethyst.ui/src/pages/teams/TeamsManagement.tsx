import { useTeamList } from "@/hooks/TeamsHook";
import { TeamTable } from "./components/TeamTable";

export const TeamsManagement = () => {

    const teams = useTeamList();

    return (
        <>
            <TeamTable teams={teams} />
        </>
    );
}
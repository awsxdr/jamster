using Func;

using jamster.engine.Domain;

namespace jamster.engine.tests.GameGeneration;

public abstract record GameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    BoxedSkater[] PenaltyBox,
    int PeriodClock,
    int HomeTeamPoints,
    int AwayTeamPoints
);

public record PreGameGameState(
    SimulatorGame Game
) : GameState(Game, 0, [], [], 30 * 60, 0, 0);

public record LineupInProgressGameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    BoxedSkater[] PenaltyBox,
    int PeriodClock,
    bool PeriodClockRunning,
    int PeriodNumber,
    int JamNumber,
    int HomeTimeoutsTaken,
    int AwayTimeoutsTaken,
    int StartTick,
    int HomeTeamPoints,
    int AwayTeamPoints
) : GameState(Game, Tick, Events, PenaltyBox, PeriodClock, HomeTeamPoints, AwayTeamPoints);

public record TimeoutInProgressGameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    BoxedSkater[] PenaltyBox,
    int PeriodClock,
    int PeriodNumber,
    int JamNumber,
    int HomeTimeoutsTaken,
    int AwayTimeoutsTaken,
    int StartTick,
    int HomeTeamPoints,
    int AwayTeamPoints
) : GameState(Game, Tick, Events, PenaltyBox, PeriodClock, HomeTeamPoints, AwayTeamPoints);

public record IntermissionInProgressGameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    BoxedSkater[] PenaltyBox,
    int HomeTimeoutsTaken,
    int AwayTimeoutsTaken,
    int StartTick,
    int HomeTeamPoints,
    int AwayTeamPoints
) : GameState(Game, Tick, Events, PenaltyBox, 30 * 60, HomeTeamPoints, AwayTeamPoints);

public record JamInProgressGameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    BoxedSkater[] PenaltyBox,
    int StartTick,
    int PeriodClock,
    int PeriodNumber,
    int JamNumber,
    int HomeTimeoutsTaken,
    int AwayTimeoutsTaken,
    OnTrackTeam HomeTeam,
    OnTrackTeam AwayTeam,
    JammerState HomeJammer,
    JammerState AwayJammer,
    Option<TeamType> LeadJammerTeam,
    int HomeTeamPoints,
    int AwayTeamPoints
) : GameState(Game, Tick, Events, PenaltyBox, PeriodClock, HomeTeamPoints, AwayTeamPoints);

public record PostGameGameState(
    SimulatorGame Game,
    int Tick,
    SimulatorEvent[] Events,
    int HomeTeamPoints,
    int AwayTeamPoints
) : GameState(Game, Tick, Events, [], 0, HomeTeamPoints, AwayTeamPoints);

public record OnTrackTeam(
    SimulatorSkater Jammer,
    SimulatorSkater Pivot,
    SimulatorSkater[] Blockers
)
{
    public SimulatorSkater[] Skaters => [
        Jammer,
        Pivot,
        ..Blockers
    ];
}

public record JammerState(
    float Progress,
    bool EligibleForLead,
    int Trip
);

public enum TeamType
{
    Home,
    Away,
}

public record BoxedSkater(
    SimulatorSkater Skater,
    TeamType Team,
    SkaterPosition Position,
    float DistanceToBox,
    int TicksRemaining
) : SimulatorSkater(Skater);
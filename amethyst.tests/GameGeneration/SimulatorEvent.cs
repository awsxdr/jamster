using amethyst.Domain;

namespace amethyst.tests.GameGeneration;

public abstract record SimulatorEvent(
    int Tick,
    ExpectedState ExpectedState
);

public record LineupClockStartedEvent(
    int Tick,
    ExpectedState ExpectedState
) : SimulatorEvent(Tick, ExpectedState);

public record JamStartedEvent(
    int Tick,
    ExpectedState ExpectedState,
    OnTrackTeam HomeTeam,
    OnTrackTeam AwayTeam
) : SimulatorEvent(Tick, ExpectedState);

public record JamAdvancedEvent(
    int Tick,
    ExpectedState ExpectedState
) : SimulatorEvent(Tick, ExpectedState);

public record SkaterLinedUpEvent(
    int Tick,
    ExpectedState ExpectedState,
    string Number,
    SkaterPosition Position,
    TeamType Team
) : SimulatorEvent(Tick, ExpectedState);

public record SkaterSentToBoxEvent(
    int Tick,
    ExpectedState ExpectedState,
    string PenaltyCode,
    TeamType Team,
    string SkaterNumber
) : SimulatorEvent(Tick, ExpectedState);

public record SkaterReleasedFromBoxEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team,
    string SkaterNumber
) : SimulatorEvent(Tick, ExpectedState);

public record SkaterSatInBoxEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team,
    string SkaterNumber
) : SimulatorEvent(Tick, ExpectedState);

public record LeadEarnedEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team
) : SimulatorEvent(Tick, ExpectedState);

public record LeadLostEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team
) : SimulatorEvent(Tick, ExpectedState);

public record JamCalledEvent(
    int Tick,
    ExpectedState ExpectedState
) : SimulatorEvent(Tick, ExpectedState);

public record InitialTripCompletedEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team
) : SimulatorEvent(Tick, ExpectedState);

public record PointsAwardedEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team,
    int Points
) : SimulatorEvent(Tick, ExpectedState);

public record TimeoutStartedEvent(
    int Tick,
    ExpectedState ExpectedState,
    TeamType Team
) : SimulatorEvent(Tick, ExpectedState);
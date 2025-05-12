using amethyst.Domain;
using amethyst.Reducers;

namespace amethyst.tests.GameGeneration;

public record ExpectedState(
    Clock PeriodClock,
    Clock JamClock,
    Clock TimeoutClock,
    Clock LineupClock,
    Clock IntervalClock,
    Stage Stage,
    int PeriodNumber,
    int JamNumber,
    int HomeScore,
    int AwayScore
);

public record Clock(
    int Value,
    bool IsRunning
);
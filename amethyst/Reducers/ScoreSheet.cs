using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class ScoreSheet(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<ScoreSheetState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<SkaterOnTrack>
    , IHandlesEvent<InitialTripCompleted>
    , IHandlesEvent<TripCompleted>
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<LeadMarked>
    , IHandlesEvent<LostMarked>
    , IHandlesEvent<CallMarked>
    , IDependsOnState<JamLineupState>
    , IDependsOnState<GameStageState>
    , IDependsOnState<TeamJamStatsState>
{
    protected override ScoreSheetState DefaultState => new([]);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        var lineup = GetKeyedState<JamLineupState>(teamSide.ToString());
        var gameStage = GetState<GameStageState>();

        SetState(new(state.Jams.Append(new ScoreSheetJam(
            gameStage.PeriodNumber,
            gameStage.JamNumber,
            gameStage.JamNumber.ToString(), 
            lineup.JammerNumber ?? "?",
            false,
            false,
            false,
            false,
            true,
            [],
            0,
            state.Jams.Any() ? state.Jams[^1].GameTotal : 0
        )).ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(SkaterOnTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var gameStage = GetState<GameStageState>();
        if (gameStage.Stage != Stage.Jam)
            return [];

        var state = GetState();

        if (@event.Body.Position == SkaterPosition.Jammer)
        {
            SetState(new (
                state.Jams.Take(state.Jams.Length - 1)
                    .Append(state.Jams[^1] with
                    {
                        JammerNumber = @event.Body.SkaterNumber ?? string.Empty,
                    })
                    .ToArray()));
        }

        return [];
    });

    public IEnumerable<Event> Handle(InitialTripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var jamStatsState = GetKeyedState<TeamJamStatsState>(teamSide.ToString());

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    NoInitial = !jamStatsState.HasCompletedInitial,
                    Trips = jamStatsState.HasCompletedInitial ? [new JamLineTrip(null)] : [],
                })
                .ToArray()
            ));

        return [];
    });

    public IEnumerable<Event> Handle(TripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    Trips = state.Jams[^1].Trips.Append(new JamLineTrip(null)).ToArray(),
                })
                .ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        
        logger.LogDebug("Jams length: {jamsLength} | Last jam trips length: {tripsLength}", state.Jams.Length, state.Jams[^1].Trips.Length);

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    JamTotal = state.Jams[^1].JamTotal + @event.Body.Value,
                    GameTotal = state.Jams[^1].GameTotal + @event.Body.Value,
                    Trips =
                        state.Jams[^1].Trips.Any()
                        ? state.Jams[^1].Trips.Take(state.Jams[^1].Trips.Length - 1).Append(state.Jams[^1].Trips[^1] with
                        {
                            Score = (state.Jams[^1].Trips[^1].Score ?? 0) + @event.Body.Value,
                        }).ToArray()
                        : [new JamLineTrip(@event.Body.Value)]
                }).ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(LeadMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    Lead = @event.Body.Lead,
                })
                .ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(LostMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    Lost = @event.Body.Lost,
                })
                .ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(CallMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(state.Jams[^1] with
                {
                    Called = @event.Body.Call,
                })
                .ToArray()
        ));

        return [];
    });
}

public sealed record ScoreSheetState(ScoreSheetJam[] Jams)
{
    public bool Equals(ScoreSheetState? other) =>
        other is not null
        && other.Jams.SequenceEqual(Jams);

    public override int GetHashCode() => Jams.GetHashCode();
}

public sealed record ScoreSheetJam(
    int Period,
    int Jam,
    string LineLabel,
    string JammerNumber,
    bool Lost,
    bool Lead,
    bool Called,
    bool Injury,
    bool NoInitial,
    JamLineTrip[] Trips,
    int JamTotal,
    int GameTotal
)
{
    public bool Equals(ScoreSheetJam? other) =>
        other is not null
        && other.Period.Equals(Period)
        && other.Jam.Equals(Jam)
        && other.LineLabel.Equals(LineLabel)
        && other.JammerNumber.Equals(JammerNumber)
        && other.Lost.Equals(Lost)
        && other.Lead.Equals(Lead)
        && other.Called.Equals(Called)
        && other.Injury.Equals(Injury)
        && other.NoInitial.Equals(NoInitial)
        && other.Trips.SequenceEqual(Trips)
        && other.JamTotal.Equals(JamTotal)
        && other.GameTotal.Equals(GameTotal);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Period);
        hashCode.Add(Jam);
        hashCode.Add(LineLabel);
        hashCode.Add(JammerNumber);
        hashCode.Add(Lost);
        hashCode.Add(Lead);
        hashCode.Add(Called);
        hashCode.Add(Injury);
        hashCode.Add(NoInitial);
        hashCode.Add(Trips);
        hashCode.Add(JamTotal);
        hashCode.Add(GameTotal);
        return hashCode.ToHashCode();
    }
}
public record JamLineTrip(int? Score);

public sealed class HomeScoreSheet(ReducerGameContext gameContext, ILogger<HomeScoreSheet> logger)
    : ScoreSheet(TeamSide.Home, gameContext, logger);
public sealed class AwayScoreSheet(ReducerGameContext gameContext, ILogger<AwayScoreSheet> logger)
    : ScoreSheet(TeamSide.Away, gameContext, logger);
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Microsoft.Extensions.Logging;

namespace amethyst.Reducers;

public class TimeoutList(ReducerGameContext context, ILogger<TimeoutList> logger) 
    : Reducer<TimeoutListState>(context)
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TeamReviewRetained>
    , IHandlesEvent<TeamReviewLost>
{
    protected override TimeoutListState DefaultState => new([]);

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();

        var newState = SetLastTimeoutDuration(state, @event.Tick);

        SetState(new (
            newState.Timeouts
                .Append(new TimeoutListItem(@event.Id, TimeoutType.Untyped, null, null, false))
                .ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        var state = GetState();

        if (!state.Timeouts.Any())
        {
            logger.LogWarning("Timeout type set but no known timeouts to set type of");
            return [];
        }

        var timeouts = state.Timeouts;

        SetState(new (
            timeouts.Take(timeouts.Length - 1)
                .Append(timeouts.Last() with { Type = @event.Body.Type, Side = @event.Body.Side })
                .ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        var state = GetState();

        if (!state.Timeouts.Any())
        {
            logger.LogWarning("Timeout ended but no known timeouts to end");
            return [];
        }

        var newState = SetLastTimeoutDuration(state, @event.Tick);

        SetState(newState);

        return [];
    }

    public IEnumerable<Event> Handle(TeamReviewRetained @event)
    {
        var state = GetState();
        var reviewIndex = state.Timeouts
            .Select((t, i) => (Index: i, Timeout: t))
            .Where(x => x.Timeout.EventId.Equals(@event.Body.TimeoutEventId))
            .Select(x => (int?) x.Index)
            .FirstOrDefault();

        if (reviewIndex == null) return [];

        var newTimeouts = state.Timeouts.ToArray();
        newTimeouts[(int)reviewIndex] = state.Timeouts[(int)reviewIndex] with { Retained = true };

        SetState(new(newTimeouts));

        return [];
    }

    public IEnumerable<Event> Handle(TeamReviewLost @event)
    {
        var state = GetState();
        var reviewIndex = state.Timeouts
            .Select((t, i) => (Index: i, Timeout: t))
            .Where(x => x.Timeout.EventId.Equals(@event.Body.TimeoutEventId))
            .Select(x => (int?)x.Index)
            .FirstOrDefault();

        if (reviewIndex == null) return [];

        var newTimeouts = state.Timeouts.ToArray();
        newTimeouts[(int)reviewIndex] = state.Timeouts[(int)reviewIndex] with { Retained = false };

        SetState(new(newTimeouts));

        return [];
    }

    private static TimeoutListState SetLastTimeoutDuration(TimeoutListState state, Tick eventTick)
    {
        if (!state.Timeouts.Any())
        {
            return state;
        }

        var timeouts = state.Timeouts;

        var lastTimeout = timeouts.Last();

        return new(
            timeouts.Take(timeouts.Length - 1)
                .Append(lastTimeout with { DurationInSeconds = lastTimeout.DurationInSeconds ?? (eventTick - lastTimeout.EventId.Tick).Seconds })
                .ToArray());
    }
}

public sealed record TimeoutListState(TimeoutListItem[] Timeouts)
{
    public bool Equals(TimeoutListState? other) =>
        other is not null
        && other.Timeouts.SequenceEqual(Timeouts);

    public override int GetHashCode() => Timeouts.GetHashCode();
}

public record TimeoutListItem(Guid7 EventId, TimeoutType Type, TeamSide? Side, int? DurationInSeconds, bool Retained);
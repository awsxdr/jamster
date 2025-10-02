using System.Reflection;
using System.Runtime.CompilerServices;

using FluentAssertions;
using Func;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.tests.EventHandling;

public class EventsBuilder(Tick tick, Event[] events)
{
    internal Tick Tick { get; } = tick;

    public virtual EventsBuilder<TEvent> Event<TEvent>(int durationInSeconds) where TEvent : Event =>
        new (Tick, events, durationInSeconds);

    public EventsBuilder Event(Event @event, int durationInSeconds) =>
        new (
            GetNextTick(durationInSeconds),
            [.. events, @event]);

    public virtual EventsBuilder Validate(params object[] states) =>
        Event(new ValidateStateFakeEvent(Tick, states), 0);

    public virtual EventsBuilder Validate(Func<Tick, object[]> states) =>
        Validate(states(Tick));

    public virtual EventsBuilder Wait(int durationInSeconds) => Event<WaitFakeEvent>(durationInSeconds);

    public virtual Event[] Build() => [..events];

    private Tick GetNextTick(int durationInSeconds)
    {
        var durationInTicks = Tick.FromSeconds(durationInSeconds);
        return Math.Max(0, Tick + durationInTicks);
    }
}

public class EventsBuilder<TEventBeingBuilt>(Tick tick, Event[] events, int currentEventDurationInSeconds) : EventsBuilder(tick, events)
    where TEventBeingBuilt : Event
{
    internal int EventDurationInSeconds { get; } = currentEventDurationInSeconds;
    private Guid7? _nextEventId = null;

    public override EventsBuilder<TEvent> Event<TEvent>(int durationInSeconds) =>
        BuildCurrentEvent().Event<TEvent>(durationInSeconds);

    public override EventsBuilder Validate(params object[] states) =>
        BuildCurrentEvent().Validate(states);

    public override EventsBuilder Validate(Func<Tick, object[]> states) =>
        BuildCurrentEvent().Validate(states);

    public override EventsBuilder Wait(int durationInSeconds) =>
        BuildCurrentEvent().Wait(durationInSeconds);

    public override Event[] Build() =>
        BuildCurrentEvent().Build();

    public virtual EventsBuilder<TEventBeingBuilt> GetTick(out Tick tick)
    {
        tick = Tick;
        return this;
    }

    public virtual EventsBuilder<TEventBeingBuilt> GetId(out Guid7 eventId)
    {
        _nextEventId = Tick;
        eventId = _nextEventId;
        return this;
    }

    private EventsBuilder BuildCurrentEvent()
    {
        var @event = (TEventBeingBuilt)Activator.CreateInstance(typeof(TEventBeingBuilt), _nextEventId ?? Tick)!;

        return Event(@event, EventDurationInSeconds);
    }
}

public static class EventsBuilderExtensions
{
    public static EventsBuilder WithBody<TEvent, TBody>(this EventsBuilder<TEvent> builder, TBody body)
        where TEvent : Event<TBody>
    {
        var @event = (TEvent) Activator.CreateInstance(typeof(TEvent), [(Guid7) builder.Tick, body])!;

        return builder.Event(@event, builder.EventDurationInSeconds);
    }
}

public interface IFakeEvent;

public class ValidateStateFakeEvent(Tick tick, params object[] states) : Event(tick), IFakeEvent
{
    public void ValidateStates(IGameStateStore stateStore)
    {
        Console.WriteLine($"Validating state at tick {tick}");
        foreach (var state in states)
        {
            if (state is ITuple tuple)
            {
                if (tuple is not [string key, _] || tuple[1] is null)
                    throw new ArgumentException();

                var tupleState = tuple[1]!;

                var storedState = stateStore.GetStateByName($"{tupleState.GetType().Name}_{key}");

                storedState.Should().BeAssignableTo<Success>();
                storedState.ValueOr(() => null).Result.Should().Be(tupleState);
            }
            else
            {
                var storedState = stateStore.GetStateByName(state.GetType().Name);

                storedState.Should().BeAssignableTo<Success>();
                storedState.ValueOr(() => null).Result.Should().Be(state);
            }
        }
    }
}

public class WaitFakeEvent(Guid7 id) : Event(id), IFakeEvent;

public class DebugFakeEvent(Guid7 id, string label) : Event(id), IFakeEvent
{
    public string Label => label;
}
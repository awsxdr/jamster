using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using FluentAssertions;
using Func;

namespace amethyst.tests.EventHandling;

public class EventsBuilder(Tick tick, Event[] events)
{
    public EventsBuilder Event<TEvent>(int durationInSeconds) where TEvent : Event
    {
        var @event = (Event)Activator.CreateInstance(typeof(TEvent), [(Guid7) tick])!;

        return Event(@event, durationInSeconds);
    }

    public EventsBuilder Event(Event @event, int durationInSeconds) =>
        new (
            GetNextTick(durationInSeconds),
            [.. events, @event]);

    public EventsBuilder Validate(params object[] states) =>
        Event(new ValidateStateFakeEvent(tick, states), 0);

    public EventsBuilder Validate(Func<Tick, object[]> states) =>
        Validate(states(tick));

    public EventsBuilder Wait(int durationInSeconds) => Event<WaitFakeEvent>(durationInSeconds);

    public Event[] Build() => [..events];

    private Tick GetNextTick(double durationInSeconds)
    {
        //var variability = Random.Shared.Next(-100, 100);
        var variability = 0;
        var durationInTicks = (int)(durationInSeconds * 1000 + variability);
        return Math.Max(0, tick + durationInTicks);
    }
}

public class ValidateStateFakeEvent(Tick tick, params object[] states) : Event(tick)
{
    public void ValidateStates(IGameStateStore stateStore)
    {
        foreach (var state in states)
        {
            var storedState = stateStore.GetStateByName(state.GetType().Name);

            storedState.Should().BeAssignableTo<Success>();
            storedState.ValueOr(() => null).Result.Should().Be(state);
        }
    }
}

public class WaitFakeEvent(Guid7 id) : Event(id);
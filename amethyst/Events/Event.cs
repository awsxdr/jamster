using amethyst.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Func;

namespace amethyst.Events;

public abstract class Event
{
    public string Type { get; set; }
    public long Tick { get; set; }

    protected Event(long tick)
    {
        Type = GetType().Name;
        Tick = tick;
    }
}

public abstract class Event<TBody>(long tick, TBody body) : Event(tick)
{
    public TBody Body { get; set; } = body;
}

public interface IUntypedEvent
{
    string Type { get; }

    Result<Event> AsEvent(Type eventType);
}

public sealed class UntypedEvent : Event, IUntypedEvent
{
    public UntypedEvent(string type) : base(0)
    {
        Type = type;
    }

    public Result<Event> AsEvent(Type eventType)
    {
        if (eventType.Name != Type)
            throw new EventTypeDoesNotMatchException();

        if (!eventType.IsDerivedFrom(typeof(Event)))
            throw new EventTypeIsNotDerivedFromEventClassException();

        if (eventType.IsDerivedFrom(typeof(Event<>)))
            throw new EventTypeIncludesUnexpectedBodyException();

        return Result.Succeed((Event)Activator.CreateInstance(eventType, 0L)!);
    }
}

public sealed class UntypedEventWithBody : Event<JsonObject>, IUntypedEvent
{
    public UntypedEventWithBody(string type, JsonObject body) : base(0, body)
    {
        Type = type;
    }

    public Result<Event> AsEvent(Type eventType)
    {
        if (!eventType.IsDerivedFrom(typeof(Event)))
            throw new EventTypeIsNotDerivedFromEventClassException();

        if (!eventType.IsDerivedFrom(typeof(Event<>), out var eventBodyType))
            throw new EventTypeDoesNotIncludeBodyException();

        var bodyType = eventBodyType!.GetGenericArguments().Single();
        var body = Body.Deserialize(bodyType);

        return body is null
            ? Result<Event>.Fail<BodyFormatIncorrectError>()
            : Result.Succeed((Event)Activator.CreateInstance(eventType, 0L, body)!);
    }
}

public sealed class EventTypeIsNotDerivedFromEventClassException : ArgumentException;
public sealed class EventTypeDoesNotMatchException : ArgumentException;
public sealed class EventTypeIncludesUnexpectedBodyException : ArgumentException;
public sealed class EventTypeDoesNotIncludeBodyException : ArgumentException;
public sealed class BodyFormatIncorrectError : ResultError;
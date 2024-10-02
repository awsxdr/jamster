using amethyst.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Services;
using Func;

namespace amethyst.Events;

public abstract class Event
{
    public string Type { get; set; }
    public Guid7 Id { get; set; }
    public long Tick => Id.Tick;
    public virtual bool HasBody => false;

    protected Event(Guid7 id)
    {
        Type = GetType().Name;
        Id = id;
    }

    public virtual object? GetBodyObject() => null;

    public bool Equals(Event other) =>
        Type == other.Type && Id == other.Id;

    public override bool Equals(object? obj) =>
        obj is Event @event && Equals(@event);

    public override int GetHashCode() =>
        HashCode.Combine(Type, Id);
}

public abstract class Event<TBody>(Guid7 id, TBody body) : Event(id)
{
    public override bool HasBody => true;
    public TBody Body { get; set; } = body;

    public override object? GetBodyObject() => Body;
}

public interface IUntypedEvent
{
    string Type { get; }

    Result<Event> AsEvent(Type eventType);
}

public sealed class UntypedEvent : Event, IUntypedEvent
{
    public UntypedEvent(string type) : this(type, Guid.Empty)
    {
    }

    public UntypedEvent(string type, Guid7 id) : base(id)
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

        return Result.Succeed((Event)Activator.CreateInstance(eventType, Id)!);
    }
}

public sealed class UntypedEventWithBody : Event<JsonObject>, IUntypedEvent
{
    public UntypedEventWithBody(string type, JsonObject body) : this(type, Guid.Empty, body)
    {
    }

    public UntypedEventWithBody(string type, Guid7 id, JsonObject body) : base(id, body)
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
using System.Collections.Immutable;

using jamster.engine.Events;
using jamster.engine.Extensions;

namespace jamster.engine.Services;

public interface IEventConverter
{
    Result<Event> DecodeEvent(IUntypedEvent @event);
}

[Singleton]
public class EventConverter : IEventConverter
{
    private readonly ImmutableDictionary<string, Type> _eventTypes;

    public EventConverter()
    {
        _eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type.IsDerivedFrom(typeof(Event)))
            .Where(type => type is { IsAbstract: false, IsGenericType: false })
            .ToImmutableDictionary(type => type.Name, type => type);
    }

    public Result<Event> DecodeEvent(IUntypedEvent @event) =>
        _eventTypes.TryGetValue(@event.Type, out var eventType)
            ? @event.AsEvent(eventType)
            : Result<Event>.Fail<EventTypeNotKnownError>();
}

public class EventTypeNotKnownError : ResultError;
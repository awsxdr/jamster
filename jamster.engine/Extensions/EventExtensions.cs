using jamster.engine.Domain;
using jamster.engine.Events;

namespace jamster.engine.Extensions;

public static class EventExtensions
{
    public static IEnumerable<Event> HandleIfTeam<TEvent>(this TEvent @event, TeamSide teamSide, Func<IEnumerable<Event>> handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return [];

        return handler();
    }
}
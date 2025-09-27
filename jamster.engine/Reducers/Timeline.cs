using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public class Timeline(ReducerGameContext context, ILogger<Timeline> logger) 
    : Reducer<TimelineState>(context)
    , IHandlesAllEvents
    , IDependsOnState<GameStageState>
{
    protected override TimelineState DefaultState => new(Stage.BeforeGame, 0, Guid.Empty, "", []);

    public IEnumerable<Event> Handle(Event @event, Guid7? sourceEventId)
    {
        if (sourceEventId != null && sourceEventId == GameClock.TickEventId)
            return [];

        var stage = GetState<GameStageState>().Stage;
        var currentState = GetState();

        var eventChangesStage = stage != currentState.CurrentStage;
        if (!eventChangesStage)
            return [];

        var triggeringEventId = sourceEventId ?? @event.Id;

        logger.LogDebug("Timeline stage moving from {oldStage} to {newStage}. Event ID: {eventId}", currentState.CurrentStage, stage, triggeringEventId);

        SetState(new(
            stage,
            @event.Tick,
            triggeringEventId, 
            @event.Type,
            [
                ..currentState.PreviousStages,
                new(
                    currentState.CurrentStage,
                    currentState.CurrentStageStartTick,
                    @event.Tick - currentState.CurrentStageStartTick,
                    currentState.CurrentStageEventId,
                    currentState.CurrentEventType
                )
            ]
        ));

        return [];
    }
}

public record TimelineState(
    Stage CurrentStage,
    Tick CurrentStageStartTick,
    Guid CurrentStageEventId,
    string CurrentEventType,
    StageListItem[] PreviousStages
);

public record StageListItem(Stage Stage, Tick StartTick, Tick Duration, Guid EventId, string EventType);
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class Timeline(ReducerGameContext context, ILogger<Timeline> logger) 
    : Reducer<TimelineState>(context)
    , IHandlesAllEvents
    , IDependsOnState<GameStageState>
{
    protected override TimelineState DefaultState => new(Stage.BeforeGame, 0, Guid.Empty, []);

    public IEnumerable<Event> Handle(Event @event, Guid7? sourceEventId)
    {
        var eventIsPersisted = sourceEventId == null;
        if (!eventIsPersisted)
            return [];

        var stage = GetState<GameStageState>().Stage;
        var currentState = GetState();

        var eventChangesStage = stage != currentState.CurrentStage;
        if (!eventChangesStage)
            return [];

        logger.LogDebug("Timeline stage moving from {oldStage} to {newStage}. Event ID: {eventId}", currentState.CurrentStage, stage, @event.Id);

        SetState(new(
            stage,
            @event.Tick,
            @event.Id, 
            [
                ..currentState.PreviousStages,
                new(
                    currentState.CurrentStage,
                    currentState.CurrentStageStartTick,
                    @event.Tick - currentState.CurrentStageStartTick,
                    currentState.CurrentStageEventId
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
    StageListItem[] PreviousStages
);

public record StageListItem(Stage Stage, Tick StartTick, Tick Duration, Guid EventId);
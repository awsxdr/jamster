#if DEBUG
using jamster.engine.DataStores;
using jamster.engine.Domain;
using jamster.engine.Events;

namespace jamster.engine.Services;

public interface ITestGameLoader
{
    Task<Result> ConfigureTestGame(string testGameName);
}

[Singleton]
public class TestGameLoader(
    IGameDiscoveryService gameDiscoveryService,
    IGameDataStoreFactory gameDataStoreFactory,
    ISystemTime systemTime
    ) : ITestGameLoader
{
    private static readonly GameTeam HomeTeam = new(
        new() { ["team"] = "Test Home Team" },
        new TeamColor(Color.Black, Color.White),
        [
            new("0", "Test Skater 1", true),
            new("12", "Test Skater 2", true),
            new("267", "Test Skater 3", true),
            new("3876", "Test Skater 4", true),
            new("4", "Test Skater 5", true),
            new("52", "Test Skater 6", true),
            new("697", "Test Skater 7", true),
            new("7293", "Test Skater 8", true),
            new("8", "Test Skater 9", true),
            new("90", "Test Skater 10", true),
        ]);

    private static readonly GameTeam AwayTeam = new(
        new() { ["team"] = "Test Away Team" },
        new TeamColor(Color.White, Color.Black),
        [
            new("0583", "Test Skater 1", true),
            new("183", "Test Skater 2", true),
            new("28", "Test Skater 3", true),
            new("3", "Test Skater 4", true),
            new("4957", "Test Skater 5", true),
            new("572", "Test Skater 6", true),
            new("60", "Test Skater 7", true),
            new("7", "Test Skater 8", true),
            new("8273", "Test Skater 9", true),
            new("984", "Test Skater 10", true),
        ]);

    private readonly Dictionary<string, Event[]> _gameData = new()
    {
        ["TiedGame"] = new EventListBuilder(systemTime.GetTick() - Tick.FromSeconds(81 * 60), [])
            .Add<TeamSet>().WithBody(new TeamSetBody(TeamSide.Home, HomeTeam))
            .Add<TeamSet>().WithBody(new TeamSetBody(TeamSide.Away, AwayTeam))
            .Add<IntermissionClockSet>().WithBody(new IntermissionClockSetBody(120)).Wait(120)
            .Repeat(8)
            .Add<JamStarted>().Wait(10)
            .Add<LeadMarked>().WithBody(new LeadMarkedBody(TeamSide.Home, true)).Wait(5)
            .Add<InitialTripCompleted>().WithBody(new InitialTripCompletedBody(TeamSide.Away, true)).Wait(70)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 4)).Wait(5)
            .Add<CallMarked>().WithBody(new CallMarkedBody(TeamSide.Home, true)).Wait(2)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 2)).Wait(28)
            .Add<JamStarted>().Wait(10)
            .Add<LeadMarked>().WithBody(new LeadMarkedBody(TeamSide.Away, true)).Wait(5)
            .Add<InitialTripCompleted>().WithBody(new InitialTripCompletedBody(TeamSide.Home, true)).Wait(70)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4)).Wait(5)
            .Add<CallMarked>().WithBody(new CallMarkedBody(TeamSide.Away, true)).Wait(2)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 2)).Wait(28)
            .EndRepeat()
            .Wait(30)
            .Add<PeriodFinalized>()
            .Wait(15 * 60)
            .Repeat(8)
            .Add<JamStarted>().Wait(10)
            .Add<LeadMarked>().WithBody(new LeadMarkedBody(TeamSide.Home, true)).Wait(5)
            .Add<InitialTripCompleted>().WithBody(new InitialTripCompletedBody(TeamSide.Away, true)).Wait(70)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 4)).Wait(5)
            .Add<CallMarked>().WithBody(new CallMarkedBody(TeamSide.Home, true)).Wait(2)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 2)).Wait(28)
            .Add<JamStarted>().Wait(10)
            .Add<LeadMarked>().WithBody(new LeadMarkedBody(TeamSide.Away, true)).Wait(5)
            .Add<InitialTripCompleted>().WithBody(new InitialTripCompletedBody(TeamSide.Home, true)).Wait(70)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Away, 4)).Wait(5)
            .Add<CallMarked>().WithBody(new CallMarkedBody(TeamSide.Away, true)).Wait(2)
            .Add<ScoreModifiedRelative>().WithBody(new ScoreModifiedRelativeBody(TeamSide.Home, 2)).Wait(28)
            .EndRepeat()
    };

    public async Task<Result> ConfigureTestGame(string testGameName)
    {
        if (!_gameData.TryGetValue(testGameName, out var events))
            return Result.Fail<TestGameNotFoundError>();

        var gameInfo = new GameInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Test Game");

        if (gameDiscoveryService.GameExists(gameInfo.Id))
            await gameDiscoveryService.ArchiveGame(gameInfo.Id);

        var databaseName = IGameDiscoveryService.GetGameFileName(gameInfo);
        var store = await gameDataStoreFactory.GetDataStore(databaseName);
        store.SetInfo(gameInfo);

        foreach (var @event in events)
        {
            store.AddEvent(@event);
        }

        return Result.Succeed();
    }

    public class TestGameNotFoundError : ResultError;
}

public class EventListBuilder(Tick tick, Event[] events)
{
    public Tick Tick => tick;

    public EventListBuilder Add<TEvent>(TEvent @event) where TEvent : Event
        => new(tick, [..events, @event]);

    public virtual EventListBuilder<TEvent> Add<TEvent>() where TEvent : Event
        => new(tick, events);

    public virtual EventListBuilder Wait(double waitTimeInSeconds)
        => new(tick + (int)TimeSpan.FromSeconds(waitTimeInSeconds).TotalMilliseconds, events);

    public RepeatingEventListBuilder Repeat(int repeatCount)
        => new(tick, 0, repeatCount, events, []);

    public virtual Event[] Build() => [..events];

    public static implicit operator Event[](EventListBuilder eventListBuilder)
        => eventListBuilder.Build();
}

public class RepeatingEventListBuilder(Tick originalTick, Tick repeatTicks, int repeatCount, Event[] priorEvents, Func<Tick, Event>[] repeatedEvents)
{
    public Tick RepeatTicks => repeatTicks;

    public RepeatingEventListBuilder Add<TEvent>(Func<Tick, TEvent> eventFactory) where TEvent : Event
        => new(originalTick, repeatTicks, repeatCount, priorEvents, [.. repeatedEvents, eventFactory]);

    public virtual RepeatingEventListBuilder<TEvent> Add<TEvent>() where TEvent : Event
        => new(originalTick, repeatTicks, repeatCount, priorEvents, repeatedEvents);

    public virtual RepeatingEventListBuilder Wait(double waitTimeInSeconds)
        => new(originalTick, repeatTicks + (int)TimeSpan.FromSeconds(waitTimeInSeconds).TotalMilliseconds, repeatCount, priorEvents, repeatedEvents);

    public EventListBuilder EndRepeat()
        => new(
            originalTick + repeatTicks * repeatCount, 
            [
                ..priorEvents,
                ..Enumerable.Range(0, repeatCount)
                    .SelectMany(i => repeatedEvents.Select(e => e(i * repeatTicks + originalTick)))
            ]);
}

public class EventListBuilder<TEventBeingBuilt>(Tick tick, Event[] events) : EventListBuilder(tick, events)
    where TEventBeingBuilt : Event
{
    public override EventListBuilder<TEvent> Add<TEvent>()
        => BuildCurrentEvent().Add<TEvent>();

    public override EventListBuilder Wait(double waitTimeInSeconds)
        => BuildCurrentEvent().Wait(waitTimeInSeconds);

    public override Event[] Build()
        => BuildCurrentEvent().Build();

    public EventListBuilder BuildCurrentEvent()
    {
        var @event = (TEventBeingBuilt)Activator.CreateInstance(typeof(TEventBeingBuilt), (Guid7) Tick)!;

        return Add(@event);
    }
}

public class RepeatingEventListBuilder<TEventBeingBuilt>(Tick originalTick, Tick repeatTicks, int repeatCount, Event[] priorEvents, Func<Tick, Event>[] repeatedEvents) 
    : RepeatingEventListBuilder(originalTick, repeatTicks, repeatCount, priorEvents, repeatedEvents)
    where TEventBeingBuilt : Event
{
    public override RepeatingEventListBuilder<TEvent> Add<TEvent>()
        => BuildCurrentEvent().Add<TEvent>();

    public override RepeatingEventListBuilder Wait(double waitTimeInSeconds)
        => BuildCurrentEvent().Wait(waitTimeInSeconds);

    public RepeatingEventListBuilder BuildCurrentEvent()
    {
        var eventFactory = (Tick tickOffset) => (TEventBeingBuilt)Activator.CreateInstance(typeof(TEventBeingBuilt), (Guid7)(repeatTicks + tickOffset))!;

        return Add(eventFactory);
    }
}

public static class EventListBuilderExtensions
{
    public static EventListBuilder WithBody<TEvent, TBody>(this EventListBuilder<TEvent> builder, TBody body)
        where TEvent : Event<TBody>
    {
        var @event = (TEvent)Activator.CreateInstance(typeof(TEvent), (Guid7)builder.Tick, body)!;

        return builder.Add(@event);
    }

    public static RepeatingEventListBuilder WithBody<TEvent, TBody>(this RepeatingEventListBuilder<TEvent> builder, TBody body)
        where TEvent : Event<TBody>
    {
        var eventFactory = (Tick tickOffset) => (TEvent)Activator.CreateInstance(typeof(TEvent), (Guid7)(builder.RepeatTicks + tickOffset), body)!;

        return builder.Add(eventFactory);
    }
}
#endif
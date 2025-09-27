using FluentAssertions;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class CurrentTimeoutTypeUnitTests : ReducerUnitTest<CurrentTimeoutType, CurrentTimeoutTypeState>
{
    [Test]
    public async Task TimeoutTypeSet_UpdatesStateAsExpected(
        [Values] TimeoutType initialType, 
        [Values] TeamSide? initialSide, 
        [Values] TimeoutType timeoutType, 
        [Values] TeamSide? teamSide)
    {
        State = new(initialType, initialSide);

        await Subject.Handle(new TimeoutTypeSet(0, new(timeoutType, teamSide)));

        State.Should().Be(new CurrentTimeoutTypeState(timeoutType, teamSide));
    }

    [Test]
    public async Task TimeoutStarted_ClearsTimeoutType()
    {
        State = new(TimeoutType.Review, TeamSide.Away);

        await Subject.Handle(new TimeoutStarted(0));

        State.Should().Be(new CurrentTimeoutTypeState(TimeoutType.Untyped, null));
    }
}
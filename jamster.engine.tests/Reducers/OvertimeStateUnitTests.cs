using FluentAssertions;

using jamster.engine.Events;
using jamster.engine.Reducers;

namespace jamster.engine.tests.Reducers;

public class OvertimeStateUnitTests : ReducerUnitTest<Overtime, OvertimeState>
{
    [Test]
    public async Task OvertimeStarted_SetsIsInOvertimeToTrue()
    {
        State = new(false);

        await Subject.Handle(new OvertimeStarted(0));

        State.IsInOvertime.Should().BeTrue();
    }

    [Test]
    public async Task OvertimeEnded_SetsIsInOvertimeToFalse()
    {
        State = new(true);

        await Subject.Handle(new OvertimeEnded(0));

        State.IsInOvertime.Should().BeFalse();
    }
}

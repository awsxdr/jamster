using amethyst.Events;
using amethyst.Services;

namespace amethyst.tests;

public class TestEvent(Guid7 id, TestEventBody body) : Event<TestEventBody>(id, body);

public class TestEventBody
{
    public string Value { get; set; } = string.Empty;
}

public class TestAlignedEvent(Guid7 id) : Event(id), IPeriodClockAligned;

public class TestUndoEvent(Guid7 id) : Event(id), IShownInUndo;
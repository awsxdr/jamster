using System.Text.Json.Nodes;

using FluentAssertions;
using Func;

using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.tests;

[TestFixture]
public class UntypedEventUnitTests
{
    [Test]
    public void AsEvent_ParsesJsonBodyCorrectly()
    {
        var subject = new UntypedEventWithBody(
            "TestEvent",
            JsonNode.Parse("{\"enumValue\": \"Value2\", \"stringValue\": \"This is a test\", \"intValue\": 1234 }")!.AsObject());

        var result = subject.AsEvent(typeof(TestEvent));

        result.Should().BeAssignableTo<Success<Event>>()
            .Which.Value.Should().BeAssignableTo<TestEvent>()
            .Which.Body.Should().Be(new TestEventBody(TestEventEnum.Value2, "This is a test", 1234));
    }

    private sealed class TestEvent(Guid7 id, TestEventBody body) : Event<TestEventBody>(id, body);
    private sealed record TestEventBody(TestEventEnum EnumValue, string StringValue, int IntValue);

    private enum TestEventEnum
    {
        Value1,
        Value2,
    }
}


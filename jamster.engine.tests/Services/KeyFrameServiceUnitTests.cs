using FluentAssertions;
using Func;

using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.tests.Services;

public class KeyFrameServiceUnitTests : UnitTest<KeyFrameService>
{
    [Test]
    public void KeyframesAreImmutable()
    {
        var scoreState = new TeamScoreState(10, 5);
        var states = new Dictionary<string, object>
        {
            ["TestState"] = scoreState
        };

        GetMock<ISystemTime>().Setup(mock => mock.GetTick()).Returns(1000);
        GetMock<IGameStateStore>()
            .Setup(mock => mock.GetAllStates())
            .Returns(() => states.AsReadOnly());

        Subject.CaptureKeyFrame();

        var keyFrame = ((Some<KeyFrame>)Subject.GetKeyFrameBefore(2000)).Value;

        ((TeamScoreState)keyFrame["TestState"]).Score.Should().Be(10);

        states["TestState"] = new TeamScoreState(5, 2);

        ((TeamScoreState)keyFrame["TestState"]).Score.Should().Be(10);
    }
}
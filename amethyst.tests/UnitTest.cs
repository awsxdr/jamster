using Microsoft.Extensions.Logging;

namespace amethyst.tests;

using Moq;
using Moq.AutoMock;

[TestFixture]
public abstract class UnitTest<TSubject> : UnitTest<TSubject, TSubject>
    where TSubject : class;

public abstract class UnitTest<TSubject, TSubjectCast> 
    where TSubject : class, TSubjectCast
{
    protected MockBehavior MockingBehavior { get; set; } = MockBehavior.Loose;

    private Lazy<AutoMocker> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected AutoMocker Mocker => _mocker.Value;

    protected TSubjectCast Subject { get; private set; }

    protected Mock<TMock> GetMock<TMock>() where TMock : class => Mocker.GetMock<TMock>();
    protected TConcrete Create<TConcrete>() where TConcrete : class => Mocker.CreateInstance<TConcrete>();

    [OneTimeSetUp]
    protected virtual void OneTimeSetup()
    {
    }

    [SetUp]
    protected virtual void Setup()
    {
        _mocker = new(() => new AutoMocker(MockingBehavior));

        Mocker.Use(
            LoggerFactory
                .Create(builder =>
                {
                    builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
                })
                .CreateLogger<TSubject>());

        Subject = Mocker.CreateInstance<TSubject>();
    }

    [TearDown]
    protected virtual void Teardown()
    {
    }

    [OneTimeTearDown]
    protected virtual void OneTimeTeardown()
    {
    }
}
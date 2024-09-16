namespace amethyst.tests;

using Moq;
using Moq.AutoMock;

[TestFixture]
public abstract class UnitTest<TSubject> where TSubject : class
{
    protected MockBehavior MockingBehavior { get; set; } = MockBehavior.Loose;

    private Lazy<AutoMocker> _mocker;
    protected AutoMocker Mocker => _mocker.Value;

    protected UnitTest()
    {
        _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    }

    protected TSubject Subject => Mocker.CreateInstance<TSubject>();

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
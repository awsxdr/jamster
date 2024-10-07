using Autofac;
using Microsoft.Extensions.Logging;

namespace amethyst.tests;

using Moq;
using Autofac.Extras.Moq;

[TestFixture]
public abstract class UnitTest<TSubject> : UnitTest<TSubject, TSubject>
    where TSubject : class;

public abstract class UnitTest<TSubject, TSubjectCast> 
    where TSubject : class, TSubjectCast
{
    private Lazy<AutoMock> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected AutoMock Mocker => _mocker.Value;

    protected TSubjectCast Subject { get; private set; }

    protected virtual TSubject SubjectFactory() => Mocker.Create<TSubject>();

    protected Mock<TMock> GetMock<TMock>() where TMock : class => Mocker.Mock<TMock>();
    protected TConcrete Create<TConcrete>() where TConcrete : class => Mocker.Create<TConcrete>();

    [OneTimeSetUp]
    protected virtual void OneTimeSetup()
    {
    }

    [SetUp]
    protected virtual void Setup()
    {
        _mocker = new(AutoMock.GetLoose(builder =>
        {
            builder.RegisterInstance(
                LoggerFactory
                    .Create(options =>
                    {
                        options.AddConsole().SetMinimumLevel(LogLevel.Debug);
                    })
                    .CreateLogger<TSubject>());
        }));

        Subject = SubjectFactory();
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
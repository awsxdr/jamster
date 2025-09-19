using Autofac;
using Microsoft.Extensions.Logging;

namespace jamster.engine.tests;

using Moq;
using Autofac.Extras.Moq;

[TestFixture]
public abstract class UnitTest<TSubject> : UnitTest<TSubject, TSubject>
    where TSubject : class;

public abstract class UnitTest<TSubject, TSubjectCast> 
    where TSubject : class, TSubjectCast
{
    private Lazy<AutoMock> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected virtual AutoMock Mocker => _mocker.Value;

    protected TSubjectCast Subject { get; private set; }

    protected virtual TSubject SubjectFactory() => Mocker.Create<TSubject>();

    protected Mock<TMock> GetMock<TMock>() where TMock : class => Mocker.Mock<TMock>();
    protected TConcrete Create<TConcrete>() where TConcrete : class => Mocker.Create<TConcrete>();

    protected async Task Wait(Task task, TimeSpan? delay = null) =>
        await await Task.WhenAny(
            task,
            Task.Run(async () =>
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(4));
                throw new TimeoutException();
            }));

    protected async Task<TResult> Wait<TResult>(Task<TResult> task, TimeSpan? delay = null) =>
        await await Task.WhenAny(
            task,
            Task.Run<TResult>(async () =>
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(4));
                throw new TimeoutException();
            }));

    protected Task WaitAll(params Task[] tasks) =>
        Wait(Task.WhenAll(tasks));

    protected Task<TResult[]> WaitAll<TResult>(params Task<TResult>[] tasks) =>
        Wait(Task.WhenAll(tasks));

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
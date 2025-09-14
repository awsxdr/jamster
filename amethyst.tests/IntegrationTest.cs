using Autofac;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Logging;

namespace amethyst.tests;

public abstract class IntegrationTest<TSubject> : UnitTest<TSubject>
    where TSubject : class
{
    private Lazy<AutoMock> _mocker = new(() => throw new Exception("Mocker cannot be used until Setup() has run"));
    protected override AutoMock Mocker => _mocker.Value;

    protected TService Resolve<TService>() where TService : notnull => Mocker.Container.Resolve<TService>();

    protected abstract void ConfigureDependencies(ContainerBuilder builder);

    protected override void Setup()
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

            ConfigureDependencies(builder);
        }));

        base.Setup();
    }
}
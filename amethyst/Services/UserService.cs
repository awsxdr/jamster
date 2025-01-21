using amethyst.DataStores;
using amethyst.Extensions;
using Func;

namespace amethyst.Services;

public interface IUserService
{
    event AsyncEventHandler<UserListChangedEventArgs>? UserListChanged;
    event AsyncEventHandler<UserConfigurationChangedEventArgs>? UserConfigurationChanged;

    IEnumerable<string> GetUserNames();
    Result<Domain.User> GetUser(string userName);
    void CreateIfNotExists(string userName);
    Result<TConfiguration> GetConfiguration<TConfiguration>(string userName);
    Result<object> GetConfiguration(string userName, Type configurationType);
    Task<Result> SetConfiguration(string userName, object configuration);
}

[Singleton]
public class UserService(IUserDataStore dataStore) : IUserService
{
    public event AsyncEventHandler<UserListChangedEventArgs>? UserListChanged;
    public event AsyncEventHandler<UserConfigurationChangedEventArgs>? UserConfigurationChanged;

    public IEnumerable<string> GetUserNames() => dataStore.GetUserNames();
    public Result<Domain.User> GetUser(string userName) => dataStore.GetUser(userName);

    public void CreateIfNotExists(string userName)
    {
        var changed = dataStore.CreateIfNotExists(userName);

        if (changed)
        {
            var users = GetUserNames().ToArray();
            UserListChanged?.InvokeHandlersAsync(this, new(users));
        }
    }

    public Result<TConfiguration> GetConfiguration<TConfiguration>(string userName) =>
        dataStore.GetConfiguration<TConfiguration>(userName);

    public Result<object> GetConfiguration(string userName, Type configurationType) =>
        dataStore.GetConfiguration(userName, configurationType);

    public Task<Result> SetConfiguration(string userName, object configuration) =>
        dataStore.SetConfiguration(userName, configuration)
            .OnSuccess(() =>
                UserConfigurationChanged?.InvokeHandlersAsync(this, new(userName, configuration.GetType(), configuration))
                ?? Task.CompletedTask);
}

public sealed class UserListChangedEventArgs(string[] users) : EventArgs
{
    public string[] Users { get; } = users;
}

public sealed class UserConfigurationChangedEventArgs(string userName, Type configurationType, object value) : EventArgs
{
    public string UserName { get; } = userName;
    public Type ConfigurationType { get; } = configurationType;
    public object Value { get; } = value;
}

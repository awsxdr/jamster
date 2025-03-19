using amethyst.DataStores;

namespace amethyst.Services;

public interface IUserService
{
    event AsyncEventHandler<UserListChangedEventArgs>? UserListChanged;
    event AsyncEventHandler<UserConfigurationChangedEventArgs>? UserConfigurationChanged;

    IEnumerable<string> GetUserNames();
    Result<Domain.User> GetUser(string userName);
    Task CreateIfNotExists(string userName);
    Task<Result> ImportUsers(Stream jsonStream);
    Result<string> GetUsersJson(string[] userNames);
    Task<Result> DeleteUser(string userName);
    Result<TConfiguration> GetConfiguration<TConfiguration>(string userName);
    Result<object> GetConfiguration(string userName, Type configurationType);
    Task<Result> SetConfiguration(string userName, object configuration);
}

[Singleton]
public class UserService(IUserDataStore dataStore, IUserDataSerializer userDataSerializer) : IUserService
{
    public event AsyncEventHandler<UserListChangedEventArgs>? UserListChanged;
    public event AsyncEventHandler<UserConfigurationChangedEventArgs>? UserConfigurationChanged;

    public IEnumerable<string> GetUserNames() => dataStore.GetUserNames();
    public Result<Domain.User> GetUser(string userName) => dataStore.GetUser(userName);

    public async Task CreateIfNotExists(string userName)
    {
        var changed = dataStore.CreateIfNotExists(userName);

        if (changed)
        {
            var users = GetUserNames().ToArray();
            await (UserListChanged?.InvokeHandlersAsync(this, new(users)) ?? Task.CompletedTask);
        }
    }

    public async Task<Result> ImportUsers(Stream jsonStream)
    {
        var result = userDataSerializer.Deserialize(jsonStream);

        if(result is not Success<IEnumerable<UserWithConfigurations>> s)
            return result;

        foreach (var user in s.Value)
        {
            await CreateIfNotExists(user.UserName);
            foreach (var configuration in user.Configurations.Values)
            {
                await SetConfiguration(user.UserName, configuration);
            }
        }

        return Result.Succeed();
    }

    public Result<string> GetUsersJson(string[] userNames) =>
        userNames.Select(dataStore.GetUser)
            .Aggregate(
                Result.Succeed<IEnumerable<UserWithConfigurations>>([]),
                (a, c) => a.And(c).ThenMap(x => x.Item1.Append(new UserWithConfigurations(x.Item2.Name, x.Item2.Configurations))))
            .ThenMap(userDataSerializer.Serialize);

    public Task<Result> DeleteUser(string userName) =>
        dataStore.DeleteUser(userName)
            .OnSuccess(() =>
            {
                var users = GetUserNames().ToArray();
                return UserListChanged?.InvokeHandlersAsync(this, new(users));
            });

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

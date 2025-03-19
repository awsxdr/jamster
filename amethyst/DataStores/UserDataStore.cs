using System.Text.Json;
using amethyst.Configurations;
using amethyst.Domain;

namespace amethyst.DataStores;

public interface IUserDataStore : IDisposable
{
    IEnumerable<string> GetUserNames();
    Result<Domain.User> GetUser(string userName);
    bool CreateIfNotExists(string userName);
    Result DeleteUser(string userName);
    Result SetConfiguration(string userName, object configuration);
    Result<TConfiguration> GetConfiguration<TConfiguration>(string userName);
    Result<object> GetConfiguration(string userName, Type configurationType);
}

public class UserDataStore : DataStore, IUserDataStore
{
    private readonly IDataTable<User, string> _usersTable;
    private readonly IDataTable<UserConfiguration, string> _configurationTable;
    private readonly IReadOnlyDictionary<string, IConfigurationFactory> _configurationFactories;

    public UserDataStore(ConnectionFactory connectionFactory, IDataTableFactory dataTableFactory, IEnumerable<IConfigurationFactory> configurationFactories)
        : base("users", 1, connectionFactory, dataTableFactory)
    {
        _usersTable = GetTable<User, string>(u => u.Name);
        _configurationTable = GetTable<UserConfiguration, string>(
            c => $"{c.UserName}_{c.ConfigurationType}",
            [
                new Column<string, UserConfiguration>("userName", u => u.UserName),
                new Column<string, UserConfiguration>("configurationType", u => u.ConfigurationType),
            ]);

        _configurationFactories = configurationFactories.ToDictionary(f => f.ConfigurationType.Name, f => f);
    }

    public IEnumerable<string> GetUserNames() =>
        _usersTable.GetAll().Select(u => u.Name).ToArray();

    public Result<Domain.User> GetUser(string userName) =>
        _usersTable.Get(userName.ToLowerInvariant())
                .ThenMap(user => new Domain.User(
                    user.Name,
                    _configurationTable.GetByColumn(_configurationTable.Columns["userName"], user.Name)
                        .Select(x => (x.ConfigurationType, Value: _configurationFactories[x.ConfigurationType].ParseConfiguration(x.ConfigurationJson)))
                        .Where(x => x.Value is Success<object>)
                        .ToDictionary(
                            x => x.ConfigurationType,
                            x => ((Success<object>)x.Value).Value)
                ))
            switch
            {
                Failure<NotFoundError> => Result<Domain.User>.Fail<UserNotFoundError>(),
                var r => r
            };

    public bool CreateIfNotExists(string userName) =>
        _usersTable.Insert(new User(userName.ToLowerInvariant()));

    public Result DeleteUser(string userName) =>
        _usersTable.Archive(userName.ToLowerInvariant())
            .Then(() =>
            {
                // Result is ignored as it will fail if no configurations have been set for the user
                _ = _configurationTable.ArchiveByColumn(_configurationTable.Columns["userName"], userName.ToLowerInvariant());
                return Result.Succeed();
            })
            switch
            {
                Failure<NotFoundError> => Result<Domain.User>.Fail<UserNotFoundError>(),
                var r => r
            };

    public Result SetConfiguration(string userName, object configuration)
    {
        if (!_usersTable.Exists(userName.ToLowerInvariant()))
            return Result.Fail<UserNotFoundError>();

        return _configurationTable.Upsert(
            new UserConfiguration(
                userName.ToLowerInvariant(), 
                configuration.GetType().Name,
                JsonSerializer.Serialize(configuration, Program.JsonSerializerOptions)));
    }

    public Result<TConfiguration> GetConfiguration<TConfiguration>(string userName) =>
        GetConfiguration(userName, typeof(TConfiguration))
            .ThenMap(c => (TConfiguration)c);

    public Result<object> GetConfiguration(string userName, Type configurationType)
    {
        if (!_usersTable.Exists(userName.ToLowerInvariant()))
            return Result<object>.Fail<UserNotFoundError>();

        if (!_configurationFactories.TryGetValue(configurationType.Name, out var configurationFactory))
            return Result<object>.Fail<ConfigurationTypeNotKnownError>();

        return _configurationTable.Get(_configurationTable.KeySelector(new UserConfiguration(userName.ToLowerInvariant(), configurationType.Name, "")))
            .Then(c => configurationFactory.ParseConfiguration(c.ConfigurationJson))
            .Else<object>(_ => Result.Succeed(configurationFactory.GetDefaultValue()));
    }

    protected override void ApplyUpgrade(int version)
    {
    }
}

public record User(string Name)
{
    public User() : this(string.Empty)
    {
    }
}

public record UserConfiguration(string UserName, string ConfigurationType, string ConfigurationJson)
{
    public UserConfiguration() : this("", "", "")
    {
    }
}

public sealed class UserNotFoundError : NotFoundError;
public sealed class ConfigurationTypeNotKnownError : ResultError;
public sealed class UnableToSetConfigurationException : Exception;

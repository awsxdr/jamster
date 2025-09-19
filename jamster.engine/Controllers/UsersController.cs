using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;
using jamster.Configurations;
using jamster.DataStores;
using jamster.Domain;
using jamster.Services;
using Microsoft.AspNetCore.Mvc;

namespace jamster.Controllers;

[ApiController, Route("/api/users")]
public class UsersController(IUserService userService, IEnumerable<IConfigurationFactory> configurationFactories, ILogger<UsersController> logger) : Controller
{
    private readonly Dictionary<string, IConfigurationFactory> _configurationFactories =
        configurationFactories.ToDictionary(x => x.ConfigurationType.Name.ToLowerInvariant(), x => x);

    [HttpGet]
    public ActionResult<UserModel[]> GetUsers()
    {
        logger.LogDebug("Getting list of user names");

        return Ok(userService.GetUserNames().Select(n => new UserModel(n)));
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] UserModel user)
    {
        logger.LogDebug("Creating user {userName} if not already present", user.UserName);

        await userService.CreateIfNotExists(user.UserName);
        
        return Created($"api/users/${Uri.EscapeDataString(user.UserName)}", user);
    }

    [HttpPost]
    public async Task<ActionResult> UploadUser(IFormFile userFile)
    {
        logger.LogDebug("Importing user JSON file");

        if (userFile.ContentType != MediaTypeNames.Application.Json)
            return new UnsupportedMediaTypeResult();

        await using var readStream = userFile.OpenReadStream();

        return await userService.ImportUsers(readStream) switch
        {
            Success => Ok(),
            Failure<InvalidStreamContentError> => BadRequest(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpGet("file")]
    public ActionResult GetUserExportFile([FromQuery] string[] userNames)
    {
        logger.LogDebug("Exporting user file");

        return userService.GetUsersJson(userNames) switch
        {
            Success<string> userJson => File(Encoding.UTF8.GetBytes(userJson.Value), MediaTypeNames.Application.Json),
            var r => throw new UnexpectedResultException(r)
        };
    }

    [HttpGet("{userName}")]
    public ActionResult<UserConfigurationsModel> GetUser(string userName)
    {
        logger.LogDebug("Getting user details for {userName}", userName);

        return userService.GetUser(userName)
            switch
            {
                Success<Domain.User> s => Ok((UserConfigurationsModel)s.Value),
                Failure<UserNotFoundError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpDelete("{userName}")]
    public async Task<ActionResult> DeleteUser(string userName)
    {
        logger.LogDebug("Deleting user {userName}", userName);

        return await userService.DeleteUser(userName)
            switch
            {
                Success => NoContent(),
                Failure<UserNotFoundError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpGet("{userName}/configuration/{configurationType}")]
    public ActionResult GetConfiguration(string userName, string configurationType)
    {
        logger.LogDebug("Getting configuration {configurationName} for user {userName}", configurationType, userName);

        if (!_configurationFactories.TryGetValue(configurationType.ToLowerInvariant(), out var factory))
            return BadRequest();

        return userService.GetConfiguration(userName, factory.ConfigurationType)
            switch
            {
                Success<object> s => Ok(s.Value),
                Failure<UserNotFoundError> => NotFound(),
                Failure<ConfigurationTypeNotKnownError> => BadRequest(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpPut("{userName}/configuration/{configurationType}")]
    public async Task<ActionResult> SetConfiguration(string userName, string configurationType, [FromBody] JsonObject configuration)
    {
        logger.LogDebug("Setting configuration {configurationType} for user {userName}", configurationType, userName);

        return await DeserializeConfiguration(configurationType, configuration)
                .Then(userService.SetConfiguration, userName)
            switch
            {
                Success => Ok(),
                Failure<UserNotFoundError> => NotFound(),
                Failure<ConfigurationTypeNotKnownError> => BadRequest(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    private Result<object> DeserializeConfiguration(string configurationType, JsonObject configuration) =>
        _configurationFactories.TryGetValue(configurationType.ToLowerInvariant(), out var factory) 
            ? factory.ParseConfiguration(configuration) 
            : Result<object>.Fail<ConfigurationTypeNotKnownError>();

    private sealed class ConfigurationTypeNotKnownError : ResultError;
}

public sealed record UserModel(string UserName);

public sealed record UserConfigurationsModel(string UserName, Dictionary<string, object> Configurations)
{
    public static explicit operator UserConfigurationsModel(Domain.User user) =>
        new(user.Name, user.Configurations);
}

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using amethyst.Domain;
using amethyst.Services;
using Func;
using Microsoft.AspNetCore.Mvc;

namespace amethyst.Controllers;

[ApiController, Route("/api/configurations/{configurationKey:alpha}")]
public class ConfigurationsController : Controller
{
    [HttpGet("")]
    public async Task<ActionResult> GetConfiguration(
        string configurationKey, 
        [FromQuery] Guid? gameId, 
        [FromServices] IConfigurationService configurationService, 
        [FromServices] IDefaultConfigurationFactory defaultConfigurationFactory)
    {
        return await defaultConfigurationFactory.GetKnownConfigurationTypeForKey(configurationKey)
                .Then(type => gameId is not null
                    ? configurationService.GetConfigurationForGame((Guid)gameId, type)
                    : configurationService.GetConfiguration(type).ToTask())
            switch
            {
                Success<object> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    [HttpPost("")]
    public async Task<ActionResult> SetConfiguration(
        string configurationKey,
        [FromQuery] Guid? gameId,
        [FromBody] JsonObject configuration,
        [FromServices] IConfigurationService configurationService,
        [FromServices] IDefaultConfigurationFactory defaultConfigurationFactory)
    {
        return await defaultConfigurationFactory.GetKnownConfigurationTypeForKey(configurationKey)
                .Then(DeserializeConfiguration, configuration)
                .Then(deserializedConfiguration => gameId is not null
                    ? configurationService.SetConfigurationForGame((Guid)gameId, deserializedConfiguration, deserializedConfiguration.GetType())
                    : configurationService.SetConfiguration(deserializedConfiguration, deserializedConfiguration.GetType()))
            switch
            {
                Success => Ok(),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                Failure<InvalidConfigurationJsonError> => BadRequest(),
                var r => throw new UnexpectedResultException(r)
            };
    }

    private static Result<object> DeserializeConfiguration(JsonObject json, Type configurationType)
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        var result = json.Deserialize(configurationType, serializerOptions);

        return result is not null 
            ? Result.Succeed(result) 
            : Result<object>.Fail<InvalidConfigurationJsonError>();
    }

    private sealed class InvalidConfigurationJsonError : ResultError;
}
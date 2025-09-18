using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Domain;
using amethyst.Services;
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
    =>
        await defaultConfigurationFactory.GetKnownConfigurationTypeForKey(configurationKey)
                .Then(type => gameId is not null
                    ? configurationService.GetConfigurationForGame((Guid)gameId, type)
                    : configurationService.GetConfiguration(type).ToTask())
            switch
            {
                Success<object> s => Ok(s.Value),
                Failure<GameFileNotFoundForIdError> => NotFound(),
                var r => throw new UnexpectedResultException(r)
            };

    [HttpPut("")]
    public async Task<ActionResult> SetConfiguration(
        string configurationKey,
        [FromQuery] Guid? gameId,
        [FromBody] JsonObject configuration,
        [FromServices] IConfigurationService configurationService,
        [FromServices] IDefaultConfigurationFactory defaultConfigurationFactory) 
    =>
        await defaultConfigurationFactory.GetKnownConfigurationTypeForKey(configurationKey)
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

    private static Result<object> DeserializeConfiguration(JsonObject json, Type configurationType)
    {
        var result = json.Deserialize(configurationType, Program.JsonSerializerOptions);

        return result is not null 
            ? Result.Succeed(result) 
            : Result<object>.Fail<InvalidConfigurationJsonError>();
    }

    private sealed class InvalidConfigurationJsonError : ResultError;
}
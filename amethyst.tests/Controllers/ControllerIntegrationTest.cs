namespace amethyst.tests.Controllers;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

[TestFixture]
public abstract class ControllerIntegrationTest
{
    private readonly WebApplicationFactory<Program> _applicationFactory = new();
    protected HttpClient Client { get; private set; }

    protected JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    [OneTimeSetUp]
    public virtual void OneTimeSetup()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());

        Client = _applicationFactory.CreateClient();
        Client.GetAsync("/").Wait();
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Client.Dispose();
        _applicationFactory.Dispose();
    }

    [SetUp]
    public virtual void Setup()
    {
    }

    [TearDown]
    public virtual void TearDown()
    {
        CleanDatabase();
    }

    protected Task<HttpResponseMessage> Get(string path) =>
        Time($"GET {path}", () => Client.GetAsync(path));

    protected async Task<TContent?> Get<TContent>(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Get(path);
        response.StatusCode.Should().Be(expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected Task<HttpResponseMessage> Post(string path, HttpContent content) =>
        Time($"POST {path}", () => Client.PostAsync(path, content));

    protected async Task<TContent?> Post<TContent>(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Post(path, JsonContent.Create(content, content.GetType()));
        response.StatusCode.Should().Be(expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected Task<HttpResponseMessage> Put(string path, HttpContent content) =>
        Time($"PUT {path}", () => Client.PutAsync(path, content));

    protected async Task<HttpResponseMessage> Put(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Put(path, JsonContent.Create(content, content.GetType()));
        response.StatusCode.Should().Be(expectedStatusCode);
        return response;
    }

    protected async Task<TContent?> Put<TContent>(string path, object content, HttpStatusCode expectedStatusCode)
    {
        var response = await Put(path, JsonContent.Create(content, content.GetType()), expectedStatusCode);
        return await response.Content.ReadFromJsonAsync<TContent>(SerializerOptions);
    }

    protected Task<HttpResponseMessage> Delete(string path) =>
        Time($"DELETE {path}", () => Client.DeleteAsync(path));

    protected async Task Delete(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Delete(path);
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    protected static async Task<TResult> Time<TResult>(string message, Func<Task<TResult>> method)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await method();
        stopwatch.Stop();
        Console.WriteLine($"{message} ({stopwatch.ElapsedMilliseconds}ms)");

        return result;
    }

    protected static TResult Time<TResult>(string message, Func<TResult> method)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = method();
        stopwatch.Stop();
        Console.WriteLine($"{message} ({stopwatch.ElapsedMilliseconds}ms)");

        return result;
    }

    protected abstract void CleanDatabase();
}
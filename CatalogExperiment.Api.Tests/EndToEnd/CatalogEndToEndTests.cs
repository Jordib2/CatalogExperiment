using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogExperiment.Api.Tests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace CatalogExperiment.Api.Tests.EndToEnd;

public sealed class WebAppFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7").Build();
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        // Program.cs reads configuration at startup, so use environment variables.
        Environment.SetEnvironmentVariable("ConnectionStrings__CatalogDb", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Caching__Enabled", "true");
        Environment.SetEnvironmentVariable("Caching__RedisConnectionString", _redis.GetConnectionString());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}

public sealed class CatalogEndToEndTests(WebAppFixture app) : IClassFixture<WebAppFixture>
{
    [Fact]
    public async Task HomePage_ShowsFirstPageOfTheCatalogue()
    {
        var html = await app.Client.GetStringAsync("/");

        Assert.Contains("Film 00001", html);
        Assert.Contains("Film 00020", html);
        Assert.DoesNotContain("Film 00021", html);
    }

    [Fact]
    public async Task Api_ReturnsTwentyFilmsByDefault()
    {
        var json = await app.Client.GetFromJsonAsync<JsonElement>("/api/catalogue");

        Assert.Equal(20, json.GetProperty("items").GetArrayLength());
        Assert.Equal(10_000, json.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task HealthEndpoint_ReportsHealthy()
    {
        var response = await app.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
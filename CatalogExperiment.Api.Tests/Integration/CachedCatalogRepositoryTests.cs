using System;
using System.Collections.Generic;
using System.Text;
using CatalogExperiment.Api.Tests.Fixtures;
using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using CatalogExperiment.Persistence.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace CatalogExperiment.Api.Tests.Integration;

public sealed class CachedCatalogRepositoryTests(RedisFixture redis) : IClassFixture<RedisFixture>
{
    private IDistributedCache CreateCache() =>
        new RedisCache(Options.Create(new RedisCacheOptions { Configuration = redis.ConnectionString }));

    private static CatalogQuery UniqueQuery() => new(1, 20, $"genre-{Guid.NewGuid()}");

    [Fact]
    public async Task SecondCall_IsServedFromRedis_InnerCalledOnce()
    {
        var inner = new CountingRepository();
        var repository = new CachedCatalogRepository(inner, CreateCache(), TimeSpan.FromSeconds(30));
        var query = UniqueQuery();

        var first = await repository.GetPageAsync(query);
        var second = await repository.GetPageAsync(query);

        Assert.Equal(1, inner.Calls);
        Assert.Equal(first.TotalCount, second.TotalCount);
        Assert.Equal(first.Items[0].Title, second.Items[0].Title);
    }

    [Fact]
    public async Task DifferentQueries_UseDifferentCacheKeys()
    {
        var inner = new CountingRepository();
        var repository = new CachedCatalogRepository(inner, CreateCache(), TimeSpan.FromSeconds(30));

        await repository.GetPageAsync(UniqueQuery());
        await repository.GetPageAsync(UniqueQuery());

        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Entry_ExpiresAfterTtl_InnerCalledAgain()
    {
        var inner = new CountingRepository();
        var repository = new CachedCatalogRepository(inner, CreateCache(), TimeSpan.FromSeconds(1));
        var query = UniqueQuery();

        await repository.GetPageAsync(query);
        await Task.Delay(TimeSpan.FromMilliseconds(1500));
        await repository.GetPageAsync(query);

        Assert.Equal(2, inner.Calls);
    }

    private sealed class CountingRepository : ICatalogRepository
    {
        public int Calls { get; private set; }

        public Task<PagedResult<Film>> GetPageAsync(
            CatalogQuery query,
            CancellationToken cancellationToken = default)
        {
            Calls++;

            var films = new List<Film>
            {
                new() { Id = 1, Title = "The Matrix", Description = "d", PosterUrl = "u", ReleaseYear = 1999, Genre = "Sci-Fi" }
            };

            return Task.FromResult(new PagedResult<Film>(films, 1, query.Page, query.PageSize));
        }
    }
}
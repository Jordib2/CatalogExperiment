using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Distributed;

namespace CatalogExperiment.Persistence.Caching;

public sealed class CachedCatalogRepository(
    ICatalogRepository inner,
    IDistributedCache cache,
    TimeSpan timeToLive) : ICatalogRepository
{
    public async Task<PagedResult<Film>> GetPageAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var key = $"catalog:page:{query.Page}:{query.PageSize}:{query.Genre?.ToLowerInvariant() ?? "all"}";

        var cachedJson = await cache.GetStringAsync(key, cancellationToken);

        if (cachedJson is not null)
        {
            var cached = JsonSerializer.Deserialize<PagedResult<Film>>(cachedJson);

            if (cached is not null)
            {
                return cached;
            }
        }

        var result = await inner.GetPageAsync(query, cancellationToken);

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeToLive },
            cancellationToken);

        return result;
    }
}
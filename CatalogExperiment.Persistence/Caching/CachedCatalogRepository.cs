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

        try
        {
            var cachedJson = await cache.GetStringAsync(key, cancellationToken);

            if (cachedJson is not null)
            {
                var cached = JsonSerializer.Deserialize<PagedResult<Film>>(cachedJson);

                if (cached is not null)
                {
                    return cached;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Redis unavailable: fall back to the database (NFR4).
        }

        var result = await inner.GetPageAsync(query, cancellationToken);

        try
        {
            await cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeToLive },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Redis unavailable: serve the result without caching it.
        }

        return result;
    }
}
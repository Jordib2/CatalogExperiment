using System;
using System.Collections.Generic;
using System.Text;

using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;

namespace CatalogExperiment.Services;

public sealed class CatalogService(ICatalogRepository repository) : ICatalogService
{
    public const int MaxPageSize = 100;

    public Task<PagedResult<Film>> GetCatalogueAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = query with
        {
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, MaxPageSize),
            Genre = string.IsNullOrWhiteSpace(query.Genre) ? null : query.Genre.Trim()
        };

        return repository.GetPageAsync(normalized, cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Text;

using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using CatalogExperiment.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogExperiment.Persistence.Repositories;

public sealed class SqlCatalogRepository(CatalogDbContext dbContext) : ICatalogRepository
{
    public async Task<PagedResult<Film>> GetPageAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var films = dbContext.Films.AsNoTracking();

        if (query.Genre is not null)
        {
            films = films.Where(f => f.Genre == query.Genre);
        }

        var totalCount = await films.CountAsync(cancellationToken);

        var items = await films
            .OrderBy(f => f.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Film>(items, totalCount, query.Page, query.PageSize);
    }
}
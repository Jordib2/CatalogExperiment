using System;
using System.Collections.Generic;
using System.Text;

using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;

namespace CatalogExperiment.Core.Interfaces;

public interface ICatalogRepository
{
    Task<PagedResult<Film>> GetPageAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default);
}
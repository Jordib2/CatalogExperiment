using System;
using System.Collections.Generic;
using System.Text;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;

namespace CatalogExperiment.Core.Interfaces;

public interface ICatalogService
{
    Task<PagedResult<Film>> GetCatalogueAsync(
        CatalogQuery query,
        CancellationToken cancellationToken = default);
}
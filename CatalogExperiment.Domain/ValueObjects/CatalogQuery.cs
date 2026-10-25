using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogExperiment.Domain.ValueObjects;

public sealed record CatalogQuery(int Page = 1, int PageSize = 20, string? Genre = null);
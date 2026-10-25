using CatalogExperiment.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CatalogExperiment.Domain.ValueObjects;

namespace CatalogExperiment.Api.Controllers;

[ApiController]
[Route("api/catalogue")]
public sealed class CatalogController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCatalogue(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var result = await catalogService.GetCatalogueAsync(
            new CatalogQuery(page, pageSize, genre),
            cancellationToken);

        return Ok(result);
    }
}

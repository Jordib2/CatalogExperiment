using System.Diagnostics;
using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CatalogExperiment.Web.Pages;

public class IndexModel(ICatalogService catalogService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Genre { get; set; }

    public PagedResult<Film> Result { get; private set; } = new([], 0, 1, 20);

    public double ElapsedMilliseconds { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        Result = await catalogService.GetCatalogueAsync(
            new CatalogQuery(PageNumber, 20, Genre),
            cancellationToken);

        stopwatch.Stop();
        ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
    }
}
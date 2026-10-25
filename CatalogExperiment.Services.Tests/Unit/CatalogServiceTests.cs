using System;
using System.Collections.Generic;
using System.Text;


using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using Xunit;

namespace CatalogExperiment.Services.Tests.Unit;

public sealed class CatalogServiceTests
{
    [Fact]
    public async Task GetCatalogueAsync_ReturnsPageFromRepository()
    {
        var repository = new FakeCatalogRepository();
        var service = new CatalogService(repository);

        var result = await service.GetCatalogueAsync(new CatalogQuery(2, 10, "Drama"));

        Assert.Equal(1, repository.CallCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetCatalogueAsync_ClampsPageSizeAndPage()
    {
        var repository = new FakeCatalogRepository();
        var service = new CatalogService(repository);

        await service.GetCatalogueAsync(new CatalogQuery(-5, 100000));

        Assert.Equal(1, repository.LastQuery!.Page);
        Assert.Equal(CatalogService.MaxPageSize, repository.LastQuery.PageSize);
    }

    [Fact]
    public async Task GetCatalogueAsync_TreatsBlankGenreAsNoFilter()
    {
        var repository = new FakeCatalogRepository();
        var service = new CatalogService(repository);

        await service.GetCatalogueAsync(new CatalogQuery(1, 20, "   "));

        Assert.Null(repository.LastQuery!.Genre);
    }

    [Fact]
    public async Task GetCatalogueAsync_PassesCancellationTokenToRepository()
    {
        var repository = new FakeCatalogRepository();
        var service = new CatalogService(repository);
        using var cts = new CancellationTokenSource();

        await service.GetCatalogueAsync(new CatalogQuery(), cts.Token);

        Assert.Equal(cts.Token, repository.ReceivedToken);
    }

    private sealed class FakeCatalogRepository : ICatalogRepository
    {
        public int CallCount { get; private set; }
        public CatalogQuery? LastQuery { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }

        public Task<PagedResult<Film>> GetPageAsync(
            CatalogQuery query,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastQuery = query;
            ReceivedToken = cancellationToken;

            return Task.FromResult(
                new PagedResult<Film>([], 0, query.Page, query.PageSize));
        }
    }
}
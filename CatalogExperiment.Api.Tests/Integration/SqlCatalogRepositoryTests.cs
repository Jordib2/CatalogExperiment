using System;
using System.Collections.Generic;
using System.Text;
using CatalogExperiment.Api.Tests.Fixtures;
using CatalogExperiment.Domain.Entities;
using CatalogExperiment.Domain.ValueObjects;
using CatalogExperiment.Persistence.Data;
using CatalogExperiment.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogExperiment.Api.Tests.Integration;

public sealed class SqlCatalogRepositoryTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private CatalogDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(postgres.ConnectionString).Options);

    [Fact]
    public async Task GetPageAsync_FiltersByGenreAndPages_AgainstRealPostgres()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        context.Films.AddRange(Enumerable.Range(1, 30).Select(i => new Film
        {
            Title = $"Film {i}",
            Description = "Description",
            PosterUrl = "https://example.com/poster.jpg",
            ReleaseYear = 2000,
            Genre = i % 2 == 0 ? "Drama" : "Comedy"
        }));
        await context.SaveChangesAsync();

        var repository = new SqlCatalogRepository(context);

        var result = await repository.GetPageAsync(new CatalogQuery(2, 5, "Drama"));

        Assert.Equal(15, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.All(result.Items, f => Assert.Equal("Drama", f.Genre));
        Assert.Equal(3, result.TotalPages);
    }
}
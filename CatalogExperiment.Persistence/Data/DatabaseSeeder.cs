using System;
using System.Collections.Generic;
using System.Text;

using CatalogExperiment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogExperiment.Persistence.Data;

public static class DatabaseSeeder
{
    private static readonly string[] Genres =
        ["Sci-Fi", "Action", "Drama", "Comedy", "Horror", "Thriller", "Romance", "Documentary"];

    public static async Task InitializeAsync(CatalogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Films.AnyAsync())
        {
            return;
        }

        var random = new Random(42);

        var films = Enumerable.Range(1, 10_000)
    .Select(i => new Film
    {
        Title = $"Film {i:D5}",
        Description = $"Dit is de beschrijving van film {i}. Een verhaal vol spanning, humor en onverwachte wendingen.",
        PosterUrl = $"https://picsum.photos/seed/film{i}/300/450",
        ReleaseYear = random.Next(1950, 2026),
        Genre = Genres[random.Next(Genres.Length)]
    })
    .ToList();

        dbContext.Films.AddRange(films);
        await dbContext.SaveChangesAsync();
    }
}
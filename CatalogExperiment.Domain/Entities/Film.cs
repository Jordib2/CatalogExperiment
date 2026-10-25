using System;
using System.Collections.Generic;
using System.Text;


namespace CatalogExperiment.Domain.Entities;

public sealed class Film
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PosterUrl { get; init; } = string.Empty;
    public int ReleaseYear { get; init; }
    public string Genre { get; init; } = string.Empty;
}
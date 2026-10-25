using CatalogExperiment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CatalogExperiment.Persistence.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Film> Films => Set<Film>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Film>(entity =>
        {
            entity.ToTable("films");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Title).HasMaxLength(200).IsRequired();
            entity.Property(f => f.Description).HasMaxLength(1000).IsRequired();
            entity.Property(f => f.PosterUrl).HasMaxLength(300).IsRequired();
            entity.Property(f => f.Genre).HasMaxLength(100).IsRequired();
            entity.HasIndex(f => f.Genre);
        });
    }
}
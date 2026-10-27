using CatalogExperiment.Api.Controllers;
using CatalogExperiment.Core.Interfaces;
using CatalogExperiment.Persistence.Caching;
using CatalogExperiment.Persistence.Data;
using CatalogExperiment.Persistence.Repositories;
using CatalogExperiment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(CatalogController).Assembly);

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));

builder.Services.AddScoped<SqlCatalogRepository>();

var cachingEnabled = builder.Configuration.GetValue<bool>("Caching:Enabled");

if (cachingEnabled)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["Caching:RedisConnectionString"];
    });

    var ttl = TimeSpan.FromSeconds(
        builder.Configuration.GetValue("Caching:TtlSeconds", 60));

    builder.Services.AddScoped<ICatalogRepository>(sp =>
        new CachedCatalogRepository(
            sp.GetRequiredService<SqlCatalogRepository>(),
            sp.GetRequiredService<IDistributedCache>(),
            ttl));
}
else
{
    builder.Services.AddScoped<ICatalogRepository>(sp =>
        sp.GetRequiredService<SqlCatalogRepository>());
}

builder.Services.AddScoped<ICatalogService, CatalogService>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("CatalogDb")!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await DatabaseSeeder.InitializeAsync(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
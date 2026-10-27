\# ADR 0001: Technology stack



Status: accepted



\## Context

The CI/CD plan left the programming language and test framework open. The experiment needs a catalogue API, a simple frontend, a database, Redis, and reproducible load tests.



\## Decision

\- C# on .NET 10 with ASP.NET Core; Razor Pages for the frontend.

\- PostgreSQL 17 with Entity Framework Core (Npgsql) as database.

\- Redis 7 via IDistributedCache, implemented as a cache-aside decorator (CachedCatalogRepository) around the SQL repository.

\- xUnit for unit tests; Testcontainers for integration and end-to-end tests with real Postgres and Redis.

\- Docker Compose with fixed CPU and memory limits; GitHub Actions for CI/CD; k6 for load, stress and outage tests.



\## Consequences

\- Layered solution (Domain, Core, Services, Persistence, Api, Web) keeps the cache swappable and testable.

\- The cache can be switched on or off by configuration (Caching:Enabled), so baseline and cached runs use the same build.

\- Only the build and test commands in the pipeline depend on the language; the rest is container-based.






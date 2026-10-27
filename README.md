\# Redis Caching Performance Experiment



Catalogue API of a mini streaming application, built in two variants (without and with a Redis cache) to measure whether the cache is worth the extra complexity.



\## Requirements

Docker Desktop, .NET 10 SDK, k6.



\## Run everything (API, PostgreSQL, Redis, with CPU/memory limits)

&#x20;   docker compose up --build



\- Frontend: http://localhost:8080/

\- API: http://localhost:8080/api/catalogue?page=1\&pageSize=20\&genre=Drama

\- Health: http://localhost:8080/health



Run without cache (baseline) on Windows cmd:



&#x20;   set CACHING\_ENABLED=false

&#x20;   docker compose up --build



\## Run the tests

Docker must be running (Testcontainers starts real Postgres and Redis).



&#x20;   dotnet test CatalogExperiment.slnx



\## Run the performance tests

&#x20;   cd k6

&#x20;   set BASE\_URL=http://localhost:8080

&#x20;   set VUS=100

&#x20;   k6 run load.js

&#x20;   k6 run stress.js

&#x20;   k6 run outage.js



\## Pipeline

| Lane | Stages | Trigger |

|---|---|---|

| A | build, unit, integration (coverage >= 70%), end-to-end | every pull request, required checks |

| B | deploy + health check | merge to main |

| C | k6 load, stress, Redis outage | manual or weekly |



The DORA metrics workflow summarises deployment frequency, lead time and change failure rate.



\## Results so far (local laptop, 100 virtual users, 3 runs each)

| Variant | P95 | Throughput |

|---|---|---|

| No cache | about 141 ms | about 1170 req/s |

| Redis cache | about 30 ms | about 5250 req/s |



App, database, Redis and k6 shared one machine, so absolute numbers are not transferable.


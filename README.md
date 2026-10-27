# Redis Caching Performance Experiment

A performance engineering experiment evaluating the impact of Redis caching on a catalogue API for a mini streaming application. The project compares two configurations—without caching and with Redis caching—to determine whether the performance improvements justify the additional architectural complexity.

## Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running Without Redis Caching](#running-without-redis-caching)
- [Application Endpoints](#application-endpoints)
- [Running the Tests](#running-the-tests)
- [Performance Testing](#performance-testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [Performance Results](#performance-results)
- [Limitations](#limitations)

## Overview

The purpose of this experiment is to measure how Redis caching affects the performance and scalability of a catalogue API.

The application supports two configurations:

- **Baseline:** Catalogue requests are served without Redis caching.
- **Cached:** Redis is used to cache catalogue data and reduce repeated database access.

Both configurations use the same application code and infrastructure wherever possible, allowing for a more consistent comparison.

The experiment evaluates response times, request throughput, system behaviour under load, and resilience when Redis becomes unavailable.

## Technology Stack

| Component | Technology |
|---|---|
| Backend | C# and .NET 10 |
| API | ASP.NET Core |
| Frontend | ASP.NET Core Razor Pages |
| Database | PostgreSQL 17 |
| Caching | Redis 7 |
| Data access | Entity Framework Core with Npgsql |
| Testing | xUnit and Testcontainers |
| Containerization | Docker and Docker Compose |
| Performance testing | k6 |
| CI/CD | GitHub Actions |

## Prerequisites

Ensure the following tools are installed:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)

Docker Desktop must be running before starting the application or executing integration tests that use Testcontainers.

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Jordib2/CatalogExperiment.git
cd CatalogExperiment
```

### 2. Start the application

Build and start the application, PostgreSQL, and Redis using Docker Compose:

```bash
docker compose up --build
```

Docker Compose starts the configured services using the resource limits defined in the Compose configuration.

To stop the application, run:

```bash
docker compose down
```

## Running Without Redis Caching

To run the baseline configuration without caching, use Windows Command Prompt:

```cmd
set CACHING_ENABLED=false
docker compose up --build
```

The environment variable applies to commands executed in the current Command Prompt session. Ensure the application configuration reads this variable correctly and that Docker Compose passes it to the relevant service.

To compare the two configurations, run the same performance scenarios under comparable conditions.

## Application Endpoints

Once the application is running, the following endpoints are available:

| Purpose | Endpoint |
|---|---|
| Frontend | http://localhost:8080/ |
| Catalogue API | http://localhost:8080/api/catalogue?page=1&pageSize=20&genre=Drama |
| Health check | http://localhost:8080/health |

The catalogue endpoint supports pagination and genre filtering through query parameters.

## Running the Tests

Run the complete test solution from the repository root:

```bash
dotnet test CatalogExperiment.slnx
```

The test suite includes unit tests and API integration and end-to-end tests.

Integration tests use Testcontainers to run real PostgreSQL and Redis instances. Docker must therefore be available when running these tests locally.

The CI pipeline also collects code coverage and enforces a minimum line coverage threshold of 70%.

## Performance Testing

The project uses k6 to evaluate application performance under different workload conditions.

The available scenarios are:

| Scenario | Purpose |
|---|---|
| `load.js` | Measure performance under the configured normal load |
| `stress.js` | Evaluate behaviour under increasing or elevated load |
| `outage.js` | Evaluate behaviour when Redis becomes unavailable |

### Configure the test environment

In Windows Command Prompt, navigate to the performance test directory:

```cmd
cd k6
set BASE_URL=http://localhost:8080
set VUS=100
```

### Execute the scenarios

Run the load test:

```cmd
k6 run load.js
```

Run the stress test:

```cmd
k6 run stress.js
```

Run the Redis outage test:

```cmd
k6 run outage.js
```

For a meaningful comparison, execute the same scenarios against both caching configurations. Keep the virtual-user count, test duration, application configuration, and infrastructure conditions as consistent as possible.

Record the results and retain the output as evidence for the performance evaluation.

## CI/CD Pipeline

The project uses GitHub Actions to automate building, testing, deployment verification, and performance measurement.

| Lane | Stages | Trigger |
|---|---|---|
| A — Continuous Integration | Docker build, unit tests, integration tests, code coverage, and end-to-end tests | Pushes to branches and pull requests targeting `main` |
| B — Deployment Verification | Start the Docker Compose stack and verify application health and API availability | Push to `main` after successful prerequisite jobs |
| C — Performance Testing | k6 load, stress, and Redis outage scenarios | Manual or scheduled, if configured |

### Continuous Integration

Lane A validates changes before they are merged into the main branch. The integration-test stage enforces a minimum line coverage threshold of 70%.

### Deployment Verification

Lane B starts the configured application stack on a GitHub Actions runner and performs health and API checks.

This is a temporary deployment verification environment. It does not provide a permanently hosted public application.

### Performance Testing and DORA Metrics

Lane C evaluates performance and resilience using k6. The DORA metrics workflow summarizes deployment frequency, lead time for changes, and change failure rate, subject to the data collected and the workflow's implementation.

## Performance Results

The following preliminary results were recorded on a local machine using 100 virtual users and three runs per configuration.

| Configuration | P95 Response Time | Throughput |
|---|---:|---:|
| Without Redis caching | Approximately 141 ms | Approximately 1,170 requests/s |
| With Redis caching | Approximately 30 ms | Approximately 5,250 requests/s |

### Initial Observations

In these measurements, the Redis-cached configuration achieved a lower P95 response time and higher throughput than the baseline configuration.

These results suggest that caching may reduce repeated database access and improve API responsiveness under the tested workload.

The results should be interpreted in the context of the test environment and validated through repeatable measurements.

## Limitations

The API, PostgreSQL, Redis, and k6 shared the same physical machine during the local performance tests. They therefore competed for CPU, memory, and other system resources.

Consequently:

- The absolute performance figures are environment-dependent.
- The results should not be treated as representative of production capacity.
- Repeated runs and consistent test conditions are necessary for reliable comparisons.
- Additional testing on dedicated infrastructure would help establish how the results generalize to other environments.

## Conclusion

This experiment provides a reproducible foundation for evaluating Redis caching in a catalogue API. By combining automated testing, CI/CD validation, and performance testing, the project aims to assess both the benefits and the operational trade-offs of introducing a caching layer.

The final decision should consider response time, throughput, reliability, and the additional complexity of maintaining Redis alongside the database.

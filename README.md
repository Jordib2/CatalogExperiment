# Redis Caching Performance Experiment

## About the Project

This project investigates the impact of Redis caching on the performance of a catalogue API for a mini streaming application.

The application has two configurations. The first runs without caching, while the second uses Redis to cache catalogue data. The goal is to compare both configurations and determine whether caching improves performance enough to justify the additional complexity.

The experiment focuses on response times, throughput, behaviour under load, and what happens when Redis becomes unavailable.

## Technologies

- C# and .NET 10
- ASP.NET Core
- Razor Pages
- PostgreSQL 17
- Redis 7
- Entity Framework Core
- xUnit
- Testcontainers
- Docker and Docker Compose
- GitHub Actions
- k6

## Requirements

Before running the application, make sure you have installed:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)

Docker Desktop must be running before starting the application or running integration tests that use Testcontainers.

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Jordib2/CatalogExperiment.git
cd CatalogExperiment
```

### 2. Start the Application

Run the following command from the project root:

```bash
docker compose up --build
```

This builds the application and starts the services configured in Docker Compose, including the API, PostgreSQL, and Redis.

To stop the application, run:

```bash
docker compose down
```

## Running Without Caching

To run the application without Redis caching, open Windows Command Prompt and execute:

```cmd
set CACHING_ENABLED=false
docker compose up --build
```

This sets the caching environment variable for the current Command Prompt session. The Docker Compose configuration must pass this variable to the application.

Use the same test scenarios for both configurations to make the results easier to compare.

## Application Endpoints

Once the application is running, you can access the following endpoints:

| Description | URL |
|---|---|
| Frontend | http://localhost:8080/ |
| Catalogue API | http://localhost:8080/api/catalogue?page=1&pageSize=20&genre=Drama |
| Health check | http://localhost:8080/health |

The catalogue API supports pagination and filtering by genre.

## Running the Tests

Run the complete test suite from the project root:

```bash
dotnet test CatalogExperiment.slnx
```

The project includes unit tests, integration tests, and end-to-end tests.

Integration tests use Testcontainers to start real PostgreSQL and Redis instances. Docker must therefore be running when these tests are executed locally.

The GitHub Actions pipeline also collects code coverage and checks that line coverage reaches at least 70%.

## Performance Testing

The project uses k6 to measure performance under different conditions.

Three test scenarios are available:

| Test | Purpose |
|---|---|
| `load.js` | Measures performance under normal load |
| `stress.js` | Tests how the application behaves under heavier load |
| `outage.js` | Tests application behaviour when Redis becomes unavailable |

### Configure the Tests

Open Windows Command Prompt and navigate to the k6 directory:

```cmd
cd k6
set BASE_URL=http://localhost:8080
set VUS=100
```

### Run the Tests

Load test:

```cmd
k6 run load.js
```

Stress test:

```cmd
k6 run stress.js
```

Redis outage test:

```cmd
k6 run outage.js
```

Run the same scenarios with caching enabled and disabled. Keep the workload and test conditions as consistent as possible.

Save the results so they can be used to compare response times, throughput, and application behaviour.

## CI/CD Pipeline

GitHub Actions is used to build the application, run automated tests, and verify the deployment configuration.

The pipeline is divided into three parts.

| Stage | Description | Trigger |
|---|---|---|
| CI | Docker build, unit tests, integration tests, code coverage, and end-to-end tests | Pushes to branches and pull requests targeting `main` |
| Deployment check | Starts the Docker Compose stack and checks application health and API availability | Pushes to `main` after the required tests pass |
| Performance tests | Runs k6 load, stress, and Redis outage tests | Manual or scheduled, if configured |

### Continuous Integration

The CI workflow checks changes before they are merged into `main`. It builds the Docker image, runs the automated tests, and verifies that code coverage meets the configured threshold.

### Deployment Check

The deployment workflow starts the application stack on a temporary GitHub Actions runner. It then checks whether the application becomes healthy and whether the catalogue API responds correctly.

This step verifies that the application can start in the configured environment. It does not deploy the application to a permanent public server.

### Performance Tests and DORA Metrics

The performance workflow is used to evaluate response times, throughput, and resilience.

A separate DORA workflow summarises deployment frequency, lead time for changes, and change failure rate based on the data available to the workflow.

## Performance Results

The following results were recorded on a local machine using 100 virtual users and three runs per configuration.

| Configuration | P95 Response Time | Throughput |
|---|---:|---:|
| Without Redis caching | Approximately 141 ms | Approximately 1,170 requests/s |
| With Redis caching | Approximately 30 ms | Approximately 5,250 requests/s |

### Results

In these tests, the configuration with Redis caching achieved a lower P95 response time and higher throughput than the configuration without caching.

The results indicate that caching can improve performance when catalogue data is requested repeatedly.

However, performance measurements depend on the test environment and workload. Further testing is needed to determine how these results translate to other environments.

## Limitations

The application, PostgreSQL, Redis, and k6 were running on the same machine during the local performance tests. All components shared the available CPU and memory resources.

This affects the results and means the measured performance should not be treated as a guarantee of production performance.

For a more reliable comparison, both configurations should be tested under the same conditions, with repeated runs and consistent workloads. Testing on dedicated infrastructure would also help show how the application performs when resources are less constrained.

## Conclusion

This project compares the performance of a catalogue API with and without Redis caching.

The initial results show improvements in response time and throughput when caching is enabled. The automated tests and performance scenarios provide a way to check these results and evaluate the behaviour of the application under different conditions.

The final assessment should consider not only performance improvements, but also the extra complexity and maintenance required to run Redis.

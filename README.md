[![CI](https://github.com/shreyapatil9480/dotnet-bug-tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/shreyapatil9480/dotnet-bug-tracker/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/shreyapatil9480/dotnet-bug-tracker/branch/main/graph/badge.svg)](https://codecov.io/gh/shreyapatil9480/dotnet-bug-tracker)

# dotnet-bug-tracker

A fully tested **Bug Tracker REST API** built with **ASP.NET Core (.NET 8)**, **Entity Framework Core**, and **SQLite**. This is a portfolio project demonstrating C# application development, REST API design, software testing, and CI/CD automation.

---

## What This Project Demonstrates

| Skill Area | Implementation |
|---|---|
| **C# / .NET 8** | ASP.NET Core Web API, async/await, generics, LINQ |
| **REST API Design** | 10 CRUD endpoints across 3 domain entities |
| **Entity Framework Core** | Code-first migrations, relationships, SQLite |
| **Unit Testing** | xUnit + Moq — business logic tested in isolation |
| **Integration Testing** | `WebApplicationFactory` — full HTTP stack, in-memory SQLite |
| **CI/CD** | GitHub Actions — build, test, and coverage on every push |
| **Dependency Injection** | All services registered via .NET DI container |
| **Swagger / OpenAPI** | Auto-generated interactive API docs |
| **Code Coverage** | Coverlet — targeting 80%+ coverage |

---

## Domain Overview

The API models a basic issue tracking workflow. Three core entities:

- **Project** — a container for bugs (e.g. "Payment Service", "User Auth Module")
- **Bug** — an issue logged against a project with severity and status lifecycle
- **Comment** — a threaded note attached to a bug

### Bug Status Lifecycle
```
Open → InProgress → Resolved → Closed
```

### Bug Severity Levels
```
Low | Medium | High | Critical
```

---

## API Endpoints

### Projects
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/projects` | Create a new project |
| `GET` | `/api/projects` | List all projects |
| `GET` | `/api/projects/{id}` | Get project by ID |

### Bugs
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/projects/{id}/bugs` | Log a bug against a project |
| `GET` | `/api/projects/{id}/bugs` | List all bugs for a project |
| `GET` | `/api/bugs/{id}` | Get bug details |
| `PUT` | `/api/bugs/{id}` | Update bug (status, severity, assignee) |
| `DELETE` | `/api/bugs/{id}` | Delete a bug |

### Comments
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/bugs/{id}/comments` | Add a comment to a bug |
| `GET` | `/api/bugs/{id}/comments` | Get all comments for a bug |

---

## Project Structure

```
dotnet-bug-tracker/
├── src/
│   ├── BugTracker.API/              # ASP.NET Core project (controllers, DTOs, middleware)
│   ├── BugTracker.Core/             # Domain models, interfaces (no framework dependencies)
│   └── BugTracker.Infrastructure/   # EF Core DbContext, repositories, migrations
├── tests/
│   ├── BugTracker.UnitTests/        # xUnit unit tests with Moq
│   └── BugTracker.IntegrationTests/ # xUnit integration tests via WebApplicationFactory
├── .github/
│   └── workflows/
│       └── ci.yml                   # GitHub Actions CI/CD pipeline
└── README.md
```

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 8.0 | Runtime |
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.x | ORM |
| SQLite | — | Database (file-based, zero infra cost) |
| xUnit | 2.x | Unit and integration test framework |
| Moq | 4.x | Mocking framework |
| Swashbuckle | 6.x | Swagger/OpenAPI docs |
| Coverlet | 6.x | Code coverage |
| GitHub Actions | — | CI/CD (free for public repos) |

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (free)
- [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit extension (free)

### Run Locally
```bash
git clone https://github.com/shreyapatil9480/dotnet-bug-tracker.git
cd dotnet-bug-tracker/src/BugTracker.API
dotnet restore
dotnet run
```

API is available at `https://localhost:5001`  
Swagger UI at `https://localhost:5001/swagger`

### Run Tests
```bash
# Unit tests only
dotnet test tests/BugTracker.UnitTests

# Integration tests only
dotnet test tests/BugTracker.IntegrationTests

# All tests with coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

## CI/CD Pipeline

Every push to `main` triggers the GitHub Actions workflow:

1. **Checkout** — pull latest code
2. **Setup .NET 8** — install SDK
3. **Restore** — `dotnet restore`
4. **Build** — `dotnet build --no-restore`
5. **Unit Tests** — run with coverage collection
6. **Integration Tests** — run against in-memory SQLite
7. **Coverage Report** — upload artifact

---

## Test Strategy

The test suite targets **80%+ code coverage** and is structured in two layers:

### Unit Tests (`BugTracker.UnitTests`)
- Test service and business logic classes **in isolation**
- Repository layer is **mocked** using Moq — no database involved
- Examples:
  - Creating a bug with invalid severity throws `ValidationException`
  - Status transitions are enforced (cannot go from `Closed` back to `Open`)
  - Deleting a project cascades to its bugs and comments

### Integration Tests (`BugTracker.IntegrationTests`)
- Use `WebApplicationFactory<T>` to spin up the **real API in memory**
- Run against a fresh **SQLite in-memory database** per test
- Test the full HTTP request/response cycle
- Examples:
  - `POST /api/projects` returns `201 Created` with `Location` header
  - `GET /api/bugs/{id}` returns `404 Not Found` for non-existent IDs
  - `PUT /api/bugs/{id}` with invalid status transition returns `400 Bad Request`

---

## Background

This project was built as part of a portfolio to demonstrate C#/.NET application development and software testing skills, targeting a **Software Engineer / SDE II** role with a focus on quality engineering and test automation. See [PROJECT_PLAN.md](PROJECT_PLAN.md) for the full build plan.

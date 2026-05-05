# Project Plan — dotnet-bug-tracker

This document outlines the full build plan, scope, and learning objectives for this project.

---

## Why This Project

This is the **anchor portfolio project** for demonstrating C#/.NET Software Engineering skills.  
It closes four critical resume gaps simultaneously:

- **C# and .NET** — writing production-style C# with ASP.NET Core
- **Test Automation Frameworks** — xUnit, Moq, integration testing with `WebApplicationFactory`
- **CI/CD** — GitHub Actions pipeline running on every push
- **REST API design** — building and documenting CRUD APIs with proper HTTP semantics

---

## Learning Objectives (what this project teaches)

| Objective | How It's Covered |
|---|---|
| C# syntax, OOP, LINQ | Domain models, service classes, LINQ queries in repositories |
| ASP.NET Core Web API | Controllers, routing, model binding, middleware |
| Entity Framework Core | DbContext, code-first migrations, relationships |
| Dependency Injection | Services registered in `Program.cs`, injected via constructor |
| Unit Testing with Moq | Mock `IProjectRepository` / `IBugRepository` in unit tests |
| Integration Testing | `WebApplicationFactory` + in-memory SQLite for HTTP-level tests |
| Code Coverage | Coverlet integration, 80%+ target |
| GitHub Actions CI/CD | `.github/workflows/ci.yml` runs build + tests on every push |

---

## Implementation Phases

### Phase 1 — Core API (Week 1–2)

**Goal:** A working REST API with Swagger documentation.

**Steps:**
1. Create .NET 8 solution with three projects: `API`, `Core`, `Infrastructure`
2. Define domain models in `Core`: `Project`, `Bug`, `Comment`
3. Define repository interfaces in `Core`: `IProjectRepository`, `IBugRepository`, `ICommentRepository`
4. Implement `BugTrackerDbContext` in `Infrastructure` with EF Core relationships
5. Implement SQLite repositories in `Infrastructure`
6. Build ASP.NET Core controllers in `API` for all 10 endpoints
7. Add Swagger/OpenAPI with Swashbuckle
8. Register all services in `Program.cs`
9. Run first EF migration: `dotnet ef migrations add InitialCreate`
10. Test manually via Swagger UI

**Key decisions:**
- Use SQLite (file-based, no hosting cost, EF Core supports it natively)
- Use constructor injection throughout — no `new` keyword in business logic
- Keep controllers thin — delegate all logic to service layer

---

### Phase 2 — Unit Test Suite (Week 2–3)

**Goal:** xUnit + Moq test suite covering all business logic.

**Test project:** `BugTracker.UnitTests`

**What to test:**

```
BugServiceTests
  ├── CreateBug_ValidInput_ReturnsBugId
  ├── CreateBug_InvalidSeverity_ThrowsValidationException
  ├── CreateBug_EmptyTitle_ThrowsArgumentException
  ├── UpdateStatus_OpenToInProgress_Succeeds
  ├── UpdateStatus_ClosedToOpen_ThrowsInvalidOperationException
  ├── DeleteBug_ExistingId_CallsRepositoryDelete
  └── DeleteBug_NonExistentId_ThrowsNotFoundException

ProjectServiceTests
  ├── CreateProject_ValidName_ReturnsProjectId
  ├── CreateProject_DuplicateName_ThrowsConflictException
  ├── DeleteProject_CascadesDeleteToBugsAndComments
  └── GetAllProjects_EmptyDatabase_ReturnsEmptyList

CommentServiceTests
  ├── AddComment_OpenBug_Succeeds
  ├── AddComment_ClosedBug_ThrowsInvalidOperationException
  └── GetComments_OrderedByCreatedAtAscending
```

**Moq pattern:**
```csharp
// Arrange
var mockRepo = new Mock<IBugRepository>();
mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingBug);
var service = new BugService(mockRepo.Object);

// Act
var result = await service.GetBugAsync(1);

// Assert
Assert.Equal(existingBug.Title, result.Title);
```

---

### Phase 3 — Integration Test Suite (Week 3)

**Goal:** Full HTTP-level tests using `WebApplicationFactory`.

**Test project:** `BugTracker.IntegrationTests`

**Setup pattern:**
```csharp
public class BugControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BugControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real SQLite with in-memory SQLite for tests
                services.RemoveAll<DbContextOptions<BugTrackerDbContext>>();
                services.AddDbContext<BugTrackerDbContext>(opts =>
                    opts.UseSqlite("DataSource=:memory:"));
            });
        }).CreateClient();
    }
}
```

**Test scenarios:**
```
POST /api/projects
  ├── Valid payload → 201 Created + Location header
  ├── Missing name → 400 Bad Request
  └── Empty body → 400 Bad Request

POST /api/projects/{id}/bugs
  ├── Valid bug → 201 Created
  ├── Invalid severity → 400 Bad Request
  └── Non-existent project → 404 Not Found

PUT /api/bugs/{id}
  ├── Valid status update → 200 OK
  ├── Invalid transition (Closed → Open) → 400 Bad Request
  └── Non-existent bug → 404 Not Found

DELETE /api/projects/{id}
  └── Deleting project removes all its bugs (cascade verified by follow-up GET)
```

---

### Phase 4 — GitHub Actions CI Pipeline (Week 3)

**Goal:** Automated build + test on every push to `main` and on all pull requests.

**File:** `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET 8
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Run unit tests
      run: dotnet test tests/BugTracker.UnitTests --no-build --collect:"XPlat Code Coverage"

    - name: Run integration tests
      run: dotnet test tests/BugTracker.IntegrationTests --no-build

    - name: Upload coverage report
      uses: actions/upload-artifact@v4
      with:
        name: coverage-report
        path: '**/TestResults/**/*.xml'
```

**What this achieves:**
- Every push shows a green ✅ or red ❌ on GitHub
- Pull requests cannot be merged if tests fail
- Coverage report downloadable from Actions tab
- Demonstrates real-world CI/CD practices on your resume

---

## Code Coverage Target

| Layer | Target |
|---|---|
| `BugTracker.Core` (domain models, interfaces) | 90%+ |
| `BugTracker.Infrastructure` (repositories) | 70%+ (covered by integration tests) |
| `BugTracker.API` (controllers) | 75%+ (covered by integration tests) |
| **Overall** | **80%+** |

---

## Estimated Timeline

| Phase | Time (part-time, ~2 hrs/day) |
|---|---|
| Phase 1 — Core API | 1 week |
| Phase 2 — Unit Tests | 1 week |
| Phase 3 — Integration Tests | 3–4 days |
| Phase 4 — GitHub Actions | 1 day |
| README polish | 1 day |
| **Total** | **~3 weeks** |

---

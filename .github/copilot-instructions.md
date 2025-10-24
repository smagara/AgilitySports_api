# Copilot Instructions for AgilitySports_api

## Project Overview
- **Purpose:** Backend API for an Angular sports website, built with .NET Minimal API (C-Sharp), serving data for MLB, NBA, NFL, NHL, and PGA.
- **Related Repos:**
  - Frontend: [AgilitySports_web](https://github.com/smagara/AgilitySports_web)
  - Database: [AgilitySports_data](https://github.com/smagara/AgilitySports_data)

## Architecture & Key Components
- **Endpoints:**
  - Located in `Endpoints/` (e.g., `endpoints.mlb.cs`, `endpoints.nba.cs`).
  - Each file handles routes for a specific sport or static/system data.
- **Data Layer:**
  - `Data/` contains repositories (e.g., `MLBRepo.cs`, `NBARepo.cs`) implementing data access logic.
  - Interfaces (e.g., `IMLBRepo.cs`) define contracts for each repo.
- **DTOs:**
  - `Dtos/` holds Data Transfer Objects for API responses (e.g., `MLBAttendanceDto.cs`).
- **Models:**
  - `Models/` contains core data models (e.g., `MLBAttendance.cs`).
- **Services:**
  - `Services/` includes input sanitization and validation logic.
- **Middleware:**
  - `Middleware/XssLoggingMiddleware.cs` for XSS protection and logging.

## Developer Workflows
- **Build:** Use the VS Code task `build` or run:
  ```powershell
  dotnet build agility-sports-api.csproj
  ```
- **Run (with hot reload):**
  ```powershell
  dotnet watch run --project agility-sports-api.csproj
  ```
- **Publish:**
  ```powershell
  dotnet publish agility-sports-api.csproj
  ```
- **Test HTTP Endpoints:** Use `.http` files in `Test/` with the REST Client extension.

## Project Conventions & Patterns
- **Minimal API:** Endpoints are registered directly in each `endpoints.*.cs` file using extension methods.
- **Repository Pattern:** All data access is abstracted via repositories in `Data/`.
- **DTO Usage:** API responses are shaped using DTOs from `Dtos/` to decouple from internal models.
- **Input Validation:** All user input is sanitized/validated via services in `Services/` and middleware.
- **Logging:** XSS and input issues are logged via custom middleware.
- **Configuration:** App settings in `appsettings.json` and `appsettings.Development.json`.

## Integration Points
- **Frontend:** Consumed by Angular app (see linked repo).
- **Database:** SQL Server, accessed via repositories.
- **CI/CD:** Deployed to Azure via pipelines (see project board for details).

## Examples
- To add a new sport, create new endpoint, repo, interface, model, and DTO files following the existing naming conventions.
- For input validation, extend `InputSanitizationService.cs` or `XssValidationService.cs` as needed.

---
## Additional Coding Guidelines

- **Hardcoded Values:**
  - Prefer using configuration files (`appsettings.json`, `appsettings.Development.json`) for environment-specific or sensitive values. If you find hardcoded values, refactor to use config settings unless there is a clear reason.

- **DRY Principle:**
  - Avoid duplicating logic across endpoint files or repositories. If you see repeated code, refactor into shared services, extension methods, or base classes in `Services/` or `Data/`.

- **REST API Conventions:**
  - Use standard HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`) and plural resource names for endpoints.
  - Return appropriate status codes (e.g., 200 OK, 201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error).
  - Shape responses using DTOs and avoid leaking internal model details.

- **Error Handling:**
  - Use try/catch blocks in endpoints and repositories to handle exceptions gracefully.
  - Log errors using the middleware and provide meaningful error messages in API responses.

- **Testing Endpoints:**
  - Use `.http` files in `Test/` with the REST Client extension for manual endpoint testing.
  - Include example requests and expected responses in these files for new endpoints.


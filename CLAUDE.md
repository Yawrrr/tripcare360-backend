# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Overview

TripCare360 backend is a **.NET 9 Clean Architecture** solution (`tripcare360.slnx`) for a travel insurance claim system. It handles flight-delay claim submission with STP (Straight-Through Processing) auto-approval logic.

## Build & Run

```powershell
# Build entire solution (run from the tripcare360/ directory)
dotnet build

# Build a single project
dotnet build Tripcare360.Application/Tripcare360.Application.csproj

# Run the API
dotnet run --project Tripcare360.WebApi/Tripcare360.WebApi.csproj

# Run tests (when a test project exists)
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

## Architecture

Dependencies flow **inward only** — outer layers reference inner layers, never the reverse.

```
Domain  ←  Application  ←  Infrastructure
                ↑                ↑
             WebApi  ────────────┘
```

| Project | Role |
|---|---|
| `Tripcare360.Domain` | Entities, enums, `ApiException`, `ErrorCode`. Zero dependencies. |
| `Tripcare360.Application` | Use cases (MediatR + FluentValidation), DTOs, mappers, interface contracts. |
| `Tripcare360.Infrastructure` | EF Core DbContext, `RojakkkService`, `ClaimRepository`. |
| `Tripcare360.WebApi` | Controllers, middleware pipeline, DI wiring, response envelope. |

## Project Structure

### Tripcare360.Domain
```
Entities/
  Common/
    BaseEntity.cs         — abstract base: CreatedAt, UpdatedAt (UpdatedAt auto-set by DbContext on save)
  Claim/
    ClaimEntity.cs        — main aggregate (Id, PolicyNumber, IdentityNumber, FlightNumber, Type, EstimatedPayout, Status) + BaseEntity
  Errors/
    ErrorCode.cs          — static registry of typed error definitions (Code, ErrorMsg, Details)
    ApiException.cs       — custom exception: ApiException(ErrorCode, msg?, details?)
Enums/
  ClaimStatus.cs          — Submitted | StpApproved | ManualReview | Rejected
  ClaimType.cs            — FlightDelay | BaggageDelay | MedicalExpenses | TripCancellation | LostDocuments | PersonalAccident
```

### Tripcare360.Application
```
DependencyInjection.cs            — AddApplicationServices() registers MediatR + FluentValidation pipeline
Common/
  Behaviours/
    ValidationBehaviour.cs        — MediatR pipeline: runs nested Validators before every handler
Dtos/
  Claim/
    SubmitClaimRequest.cs         — inbound request DTO
    SubmitClaimResponse.cs        — outbound response DTO
Features/
  Claim/
    Commands/
      SubmitClaimCommand.cs       — self-contained class: primary ctor + nested Validator + nested Handler
    Queries/                      — empty, ready for future read operations
Interfaces/
  Repositories/
    IGenericRepository.cs     — base CRUD interface: GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync
    IClaimRepository.cs       — extends IGenericRepository<ClaimEntity>; add claim-specific methods here
  Services/
    IRojakkkService.cs
Mappers/
  ClaimMapper.cs                  — ToCommand() (request → command) and ToResponse() (entity → response DTO)
```

### Tripcare360.Infrastructure
```
DependencyInjection.cs            — AddInfrastructureServices() registers DbContext, services, repositories
Persistence/
  Tripcare360DbContext.cs         — EF Core; enums stored as strings; SaveChangesAsync auto-sets UpdatedAt on modified BaseEntity instances
Repositories/
  GenericRepository.cs            — base EF Core implementation of IGenericRepository<T>; exposes protected Db field
  ClaimRepository.cs              — extends GenericRepository<ClaimEntity>, implements IClaimRepository
Services/
  RojakkkService.cs               — implements IRojakkkService (mock: MH123 → delayed)
```

### Tripcare360.WebApi
```
DependencyInjection.cs            — AddWebApiServices() registers controllers + CORS
Controllers/
  ClaimsController.cs             — POST /api/claims → dispatches SubmitClaimCommand via ISender
Middleware/
  ExceptionHandlerMiddleware.cs   — outermost; catches ApiException → ErrorResponse, unhandled → SYS_001
  ResponseWrapperMiddleware.cs    — wraps 2xx responses in SuccessResponse<T>
Responses/
  ApiResponse.cs                  — SuccessResponse<T> and ErrorResponse records
Program.cs                        — wires DI extensions + middleware order
appsettings.json                  — non-sensitive defaults (LocalDB connection string)
appsettings.Local.json            — git-ignored local overrides (secrets, env-specific values)
```

## Key Conventions

**CQRS structure** — one `.cs` file per command/query. The command is a **single self-contained class**: it is the `IRequest<T>`, holds the request DTO as a public property via primary constructor, and owns both `Validator` and `Handler` as nested classes — all within one set of curly braces. Queries go in `Features/<Feature>/Queries/`.

```csharp
public class SubmitClaimCommand(SubmitClaimRequest request) : IRequest<SubmitClaimResponse>
{
    public SubmitClaimRequest Request { get; } = request;

    public class Validator : AbstractValidator<SubmitClaimCommand>
    {
        public Validator() { /* add rules here */ }
    }

    public class Handler(IRojakkkService ...) : IRequestHandler<SubmitClaimCommand, SubmitClaimResponse>
    {
        public async Task<SubmitClaimResponse> Handle(SubmitClaimCommand command, CancellationToken ct)
        {
            var req = command.Request;
            // ... business logic ...
            return entity.ToResponse();   // always use the mapper, never new Dto() inline
        }
    }
}
```

- The command takes the request DTO via primary constructor and exposes it as `Request` — never duplicate the DTO's fields
- `Validator` and `Handler` are **nested classes inside the command class**
- Every command/query must have a `Validator`, even if the body is empty — populate rules later
- The handler always returns via a **mapper extension** (`entity.ToResponse()`) — never construct the response DTO inline
- `ValidationBehaviour` runs all validators automatically before the handler; no manual wiring needed per command

**DTO naming and location** — DTOs are grouped by feature under `Dtos/<Feature>/`. Inbound DTOs use the `<Name>Request` suffix; outbound DTOs use the `<Name>Response` suffix (never `Result`).

**Mapper** — `ClaimMapper` owns all mapping for the Claim feature. Add a new static extension method per mapping direction. Never construct DTOs or commands inline in controllers or handlers.

**Entity naming** — all domain entities use the `<Name>Entity` suffix (e.g. `ClaimEntity`). Each entity lives in its own subfolder under `Domain/Entities/<Name>/`. Every entity must inherit `BaseEntity` (from `Domain/Entities/Common/`) to get `CreatedAt` and `UpdatedAt`; `UpdatedAt` is auto-stamped by `Tripcare360DbContext.SaveChangesAsync` on every EF Core `Modified` state — no manual setting needed.

**Repositories** — every entity-specific repository interface extends `IGenericRepository<TEntity>` (defined in `Application/Interfaces/Repositories/`). Every concrete repository inherits `GenericRepository<TEntity>` and implements its specific interface. The protected `Db` field on `GenericRepository` is available for custom EF Core queries in subclasses.

**Error codes** — add a `static readonly ErrorCode` entry to `Domain/Entities/Errors/ErrorCode.cs`. Codes prefixed `SYS` return HTTP 500; all others return HTTP 400.

**Response envelope** — controllers return plain DTOs. `ResponseWrapperMiddleware` wraps them:
```json
{ "status": "Success", "data": { ... }, "timestamp": "...", "traceId": "..." }
```

**Error handling** — throw `ApiException(ErrorCode.X)` anywhere in the Application layer. `ExceptionHandlerMiddleware` catches it and returns:
```json
{ "status": "Failed", "errorCode": "CLM_001", "errMsg": "...", "details": "...", "timestamp": "...", "traceId": "..." }
```

**Middleware order** — `ExceptionHandlerMiddleware` must be registered before `ResponseWrapperMiddleware` in `Program.cs`.

**Environment config** — `appsettings.Local.json` is loaded optionally at startup and is git-ignored. Put secrets and local connection strings there, never in `appsettings.json`.

## STP Flow

```
POST /api/claims
  → ClaimsController.Submit([FromBody] SubmitClaimRequest)
  → request.ToCommand() → SubmitClaimCommand
  → MediatR → SubmitClaimCommand.Handler
  → IRojakkkService.VerifyFlightDelayAsync
      true  → ClaimStatus.StpApproved
      false → ClaimStatus.ManualReview
  → claim.ToResponse() → SubmitClaimResponse
  → ResponseWrapperMiddleware → SuccessResponse<SubmitClaimResponse>
```

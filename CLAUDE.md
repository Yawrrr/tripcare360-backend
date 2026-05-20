# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Overview

TripCare360 backend is a **.NET 9 Clean Architecture** solution for a travel insurance claim system. It implements a multi-step claim submission flow — policy verification, claim type selection, pre-validation with file uploads, finalization, and real-time status broadcasting via SSE — against the Etiqa insurance domain.

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

# Add an EF Core migration (always specify both projects)
dotnet-ef migrations add <MigrationName> --project Tripcare360.Infrastructure --startup-project Tripcare360.WebApi
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
| `Tripcare360.Domain` | Entities, enums, custom attributes, `ApiException`, `ErrorCode`. Zero dependencies. |
| `Tripcare360.Application` | Use cases (MediatR + FluentValidation), DTOs, mappers, service and repository interface contracts. |
| `Tripcare360.Infrastructure` | EF Core DbContext, HTTP client adapters, MinIO storage service, SSE broadcaster, repositories. |
| `Tripcare360.WebApi` | Controllers, middleware pipeline, DI wiring, multipart form binding, response envelope. |

## API Surface

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/policy/verify` | Step 1 — verify policy number + identity against the external policy registry |
| `POST` | `/api/claim/verify` | Step 3 — pre-validate incident data, upload files to MinIO, reserve claim as `Pending` |
| `POST` | `/api/claim` | Step 4 — finalize submission; enforces 10-minute reservation window |
| `GET` | `/api/claim/sse/{claimCode}` | Step 5 — SSE stream for real-time status updates |

## Claim Flow

```
Step 1  POST /api/policy/verify
          → calls external policy registry → returns eligible claim types

Step 2  (frontend-only)
          → filters claim type cards based on eligible types returned from Step 1

Step 3  POST /api/claim/verify  (multipart/form-data)
          → for flight claims: calls external flight status registry
              delay threshold not met → 400 AutomatedCheckFailed
              service outage         → bypass, IsPreValidationFailedDueToOutage = true
          → uploads supporting files to MinIO
          → saves claim as Pending, generates ClaimCode (CLM-YYYYMMDD-XXXX)
          → returns ClaimCode + CalculatedPayout + ExpiresAt (CreatedAt + 10 min)

Step 4  POST /api/claim
          → checks claim is Pending and within 10-minute window
          → transitions Pending → Submitted
          → returns final status

Step 5  GET /api/claim/sse/{claimCode}
          → SSE stream stays open; admin actions or STP logic push events
          → PaymentSuccess closes the connection; ManualReviewNeeded keeps it open
```

## Key Conventions

### CQRS

One file per command or query. Every command is a **single self-contained class**: it is the `IRequest<TResponse>`, takes the inbound DTO via primary constructor, and owns both `Validator` and `Handler` as nested classes.

```csharp
public class DoSomethingCommand(DoSomethingRequest request) : IRequest<DoSomethingResponse>
{
    public DoSomethingRequest Request { get; } = request;

    public class Validator : AbstractValidator<DoSomethingCommand>
    {
        public Validator() { /* add rules here */ }
    }

    public class Handler(IDependency dep) : IRequestHandler<DoSomethingCommand, DoSomethingResponse>
    {
        public async Task<DoSomethingResponse> Handle(DoSomethingCommand command, CancellationToken ct)
        {
            var req = command.Request;
            // ... business logic ...
            return entity.ToResponse();   // always use the mapper
        }
    }
}
```

- Never duplicate DTO fields on the command — expose the whole DTO as `Request`
- Every command/query must have a `Validator` (even if empty)
- `ValidationBehaviour` runs validators automatically — no manual wiring needed
- The handler must return via a **mapper extension method**, never construct the response DTO inline

### DTOs

Grouped by feature under `Dtos/<Feature>/`. Inbound DTOs use the `<Name>Request` suffix; outbound DTOs use `<Name>Response` (never `Result`). A feature mapper owns all static extension methods for that feature — never construct DTOs or commands inline in controllers or handlers.

### Entities & BaseEntity

All domain entities use the `<Name>Entity` suffix and inherit `BaseEntity`, which provides `CreatedAt` and `UpdatedAt` (`DateTimeOffset`). `UpdatedAt` is auto-stamped by `SaveChangesAsync` on every EF Core `Modified` entry — never set it manually. Each entity lives in its own subfolder under `Domain/Entities/<Name>/`.

Incident detail types (flight, medical, baggage, etc.) are plain POCOs under `Domain/Entities/Claim/Incidents/` — they are serialized to JSON and stored in the `IncidentDetailsJson` column, not mapped as separate tables.

### Repositories

Every entity-specific repository interface extends `IGenericRepository<TEntity>`. Every concrete repository inherits `GenericRepository<TEntity>` and implements its interface. The protected `Db` field on `GenericRepository` is available for custom EF Core queries in subclasses.

### Enum Annotations

`ClaimType` values are annotated with `[ClaimCategory(ClaimCategory.X)]`. Use reflection on the enum field to read the category — this drives dynamic UI filtering without hardcoding groupings in application logic.

### External Service Clients

HTTP adapters live in `Infrastructure/Clients/`. Their interfaces are defined in `Application/Interfaces/Services/` — the Application layer never references concrete HTTP types. Registered via `AddHttpClient<IInterface, Implementation>` with a base URL read from `IConfiguration["ExternalServices:BaseUrl"]`.

Convention for all HTTP adapters:
- `404` response → return `null`
- Network / timeout exception → rethrow as a plain `Exception` (the caller decides whether to treat this as an outage bypass or a hard failure)

### File Upload Abstraction

The Application layer uses `ClaimFileUpload` (FileName, ContentType, Content, Length) instead of `IFormFile`, keeping the layer independent of ASP.NET Core. The WebApi layer maps `IFormFile → ClaimFileUpload` in the controller action before dispatching the command.

### Outage Bypass

If an external service throws a network exception during pre-validation, the handler catches it, sets `IsPreValidationFailedDueToOutage = true` on the claim, and proceeds to save it as `Pending`. This allows the user to continue while flagging the claim for manual review.

### SSE Broadcasting

`ISseEventBroadcaster` is registered as a **Singleton**. Handlers inject it and call `BroadcastStateAsync(claimCode, eventType, data)` after status transitions. The SSE controller registers a write delegate per `claimCode`, streams `text/event-stream`, and unregisters on disconnect or cancellation.

### Error Codes

Add a `static readonly ErrorCode` entry to `Domain/Entities/Errors/ErrorCode.cs`. Code prefix rules:
- `SYS_*` → HTTP 500
- All others → HTTP 400

### Response Envelope

Controllers return plain DTOs. `ResponseWrapperMiddleware` wraps all 2xx responses:
```json
{ "status": "Success", "data": { ... }, "timestamp": "...", "traceId": "..." }
```

Throw `ApiException(ErrorCode.X)` anywhere in the Application layer. `ExceptionHandlerMiddleware` catches it:
```json
{ "status": "Failed", "errorCode": "CLM_001", "errMsg": "...", "details": "...", "timestamp": "...", "traceId": "..." }
```

**Middleware order** — `ExceptionHandlerMiddleware` must be registered before `ResponseWrapperMiddleware`.

### Config & Secrets

`appsettings.json` holds non-sensitive defaults only. `appsettings.Development.json` holds environment-specific values (external service URLs, MinIO credentials) and must be kept out of source control. Never put secrets in `appsettings.json`.

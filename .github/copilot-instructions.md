# BolsaFE UCN Backend - AI Agent Instructions

## CRITICAL: Maintaining These Instructions

**MANDATORY**: When making structural changes to the project (new models, services, controllers, migrations, authentication patterns, external integrations, architectural changes, etc.), you MUST update this document to reflect those changes. This ensures future AI agents have accurate context.

**Update triggers include**:
- Adding/removing/modifying domain models or enums
- Creating new services, repositories, or controllers
- Changing authentication/authorization patterns
- Adding external service integrations (email, storage, etc.)
- Modifying database schema or migration patterns
- Adding new endpoints or changing API conventions
- Updating dependency injection configuration
- Changing file/folder structure conventions
- Adding new testing credentials or seed data

**How to update**: After making structural changes, review this document and update the relevant sections with the new information. Be specific and concise. Follow the existing documentation style.

## Architecture Overview

**Clean Architecture + ASP.NET Core 9.0** with strict layer separation:
- **Domain** (`src/Domain/Models/`): Entity models, enums. Zero external dependencies.
- **Infrastructure** (`src/Infrastructure/`): `AppDbContext`, Repository implementations, `DataSeeder`. Only layer that talks to PostgreSQL.
- **Application** (`src/Application/`): Services (business logic), DTOs, Mappers (Mapster), Validators, Templates.
- **API** (`src/API/`): Controllers, Middlewares. Thin routing layer - NO business logic.

**Key Pattern**: Repository → Service → Controller. Services contain ALL business logic. Controllers only handle HTTP concerns (auth extraction, status codes).

## Identity & Authentication

Uses **ASP.NET Core Identity** with custom `GeneralUser : IdentityUser<int>`:
- `UserType` enum: `Estudiante`, `Empresa`, `Particular`, `Administrador`
- One-to-one discriminated inheritance: `GeneralUser` links to `Student`, `Company`, `Individual`, or `Admin`
- JWT Bearer authentication (configured in `Program.cs`)
- Role-based authorization: `Applicant` (estudiantes), `Offerent` (empresas/particulares), `Admin`, `SuperAdmin`

**Authorization Pattern**: Use `[Authorize(Roles = "Applicant,Offerent")]` on controller actions. Extract `userId` from JWT:
```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
int.TryParse(userIdClaim, out int currentUserId)
```

## Database & Migrations

**PostgreSQL 16** via EF Core. Connection string in `appsettings.json`.

**Critical Commands** (run from `backend/`):
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef database drop --force  # Drop DB completely
```

**Makefile shortcuts** (run from project root):
```bash
make db-restart    # Drop DB, migrate, run with watch (most common during dev)
make watch         # dotnet watch --no-hot-reload
make run           # dotnet run (without watch)
make docker-create # Create and start PostgreSQL container
make docker-start  # Start existing container
make docker-stop   # Stop container
make docker-rm     # Stop and remove container
```

**Data Seeding**: `DataSeeder.Initialize()` runs on startup (see `Program.cs`). Creates test users with password `Test123!`:
- `estudiante@alumnos.ucn.cl` (Applicant role)
- `empresa@techcorp.cl` (Offerent role)
- `particular@ucn.cl` (Offerent role)
- `admin@ucn.cl` (Admin role)

Seed includes fake offers, job applications, and reviews using **Bogus** library.

## Mapster Configuration

**NOT AutoMapper**. Uses **Mapster** with explicit configuration classes in `src/Application/Mappers/`:
- Each entity type has a dedicated Mapper class (e.g., `StudentMapper`, `OfferMapper`)
- Call `ConfigureAllMappings()` in mapper classes
- Registered via `MapperExtensions.ConfigureMapster()` in startup

**Pattern**: Services inject mappers as dependencies or use `TypeAdapter.Adapt<TDestination>(source)` directly.

## Service Layer Patterns

**Dependency Injection**: All services registered in `Program.cs` as Scoped:
```csharp
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
// ... plus repositories and mappers
```

**Common Service Responsibilities**:
- **Validation**: Throw `InvalidOperationException`, `KeyNotFoundException`, `UnauthorizedAccessException` with descriptive messages
- **Repository calls**: Always async (`await`)
- **Rating updates**: `GeneralUser.Rating` is auto-calculated. Access directly, don't recalculate in services.
- **Transactions**: EF Core tracks changes automatically. Call `context.SaveChanges()` in repositories.

## External Service Integrations

**Resend (Email)**: For email verification and notifications
- Configured in `Program.cs` with API key from `appsettings.json`
- Service: `IEmailService` / `EmailService` in `src/Application/Services/`
- Templates in `src/Application/Templates/`

**Cloudinary (Images)**: For image upload/storage
- Credentials in `appsettings.json`
- Service: `IFileService` / `FileService`

**Hangfire (Background Jobs)**: For scheduled tasks
- Uses MemoryStorage (not persisted to DB)
- Recurring job: `CloseExpiredReviewsAsync()` runs hourly
- Dashboard: `http://localhost:5185/hangfire` (dev only)

## Review System (Bidirectional)

`Review` model is **bidirectional**: Offeror rates Student AND Student rates Offeror.
- `RatingForStudent` / `CommentForStudent` (from Offeror)
- `RatingForOfferor` / `CommentForOfferor` (from Student)
- `ReviewChecklistValues` (embedded object for student evaluation):
  - `AtTime`: boolean flag indicating if student arrived on time
  - `GoodPresentation`: boolean flag for student's presentation quality
  - `StudentHasRespectOfferor`: boolean flag indicating if student showed respect to offeror
- `PublicationId` links to the offer
- Window closes 14 days after creation (`ReviewWindowEndDate`)

**Hangfire Job**: `CloseExpiredReviewsAsync()` runs hourly (configured in `Program.cs`).

**Pending Reviews Limit**: Users cannot create new publications or apply to offers if they have more than 3 pending reviews. A review is considered pending when:
- The review is not completed by both parties (`!IsCompleted`)
- The review is not closed (`!IsClosed`)
- The specific user has not completed their part:
  - For students: `!IsReviewForOfferorCompleted`
  - For offerors: `!IsReviewForStudentCompleted`

This validation is enforced in:
- `JobApplicationController.ApplyToOffer` - Students cannot apply to new offers
- `PublicationController.CreateOffer` - Users cannot create new job offers
- `PublicationController.CreateBuySell` - Users cannot create new buy/sell publications

Service method: `IReviewService.GetPendingReviewsCountAsync(int userId)` returns the count of pending reviews for a user.

## PDF Generation

Uses **QuestPDF** (Community license for educational use).
- Service: `PdfGeneratorService` implements `IPdfGeneratorService`
- Endpoint: `GET /api/review/my-reviews/pdf` (requires `[Authorize]`)
- Pattern: Fetch data → Build DTO → Generate with QuestPDF's fluent API
- Color scheme: Dynamic based on rating (5.5+ = green, <3.0 = red)

## Testing Credentials

Four pre-seeded users (all use `Test123!`):
| Email | Role | UserType |
|-------|------|----------|
| `estudiante@alumnos.ucn.cl` | Applicant | Estudiante |
| `empresa@techcorp.cl` | Offerent | Empresa |
| `particular@ucn.cl` | Offerent | Particular |
| `admin@ucn.cl` | Admin | Administrador |

**Quick Test Flow**:
1. `POST /api/auth/login` with email/password
2. Copy JWT token from response
3. Use `Authorization: Bearer {token}` in subsequent requests

## Controllers Convention

All controllers inherit from `BaseController`:
```csharp
[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase { }
```

**Standard Pattern**:
- Inject services via constructor
- Extract `userId` from JWT claims for auth checks
- Delegate ALL logic to services
- Return `Ok()`, `NotFound()`, `Unauthorized()`, `BadRequest()` with meaningful messages

## Publication System

`Publication` is abstract base class. Derived types:
- **Offer**: Job offers (has `Remuneration`, `Requirements`, etc.)
- **BuySell**: Marketplace items (has `Price`, `Condition`, etc.)

Both support:
- `StatusValidation` enum: `Published`, `InProcess`, `Rejected` (admin workflow)
- `IsActive` boolean
- Image attachments via `ICollection<Image>`

## Error Handling

Global middleware: `ErrorHandlingMiddleware` (in `src/API/Middlewares/ErrorHandlingMiddlewares.cs`) catches exceptions and returns structured JSON:
```json
{
  "title": "Error title",
  "message": "Error description"
}
```

**Exception Mapping**:
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found  
- `InvalidOperationException` → 400 Bad Request
- `SecurityException` → 403 Forbidden
- All others → 500 Internal Server Error

**Service Layer**: Throw specific exceptions with descriptive messages. Middleware automatically handles HTTP status codes and logging (with trace IDs).

## Logging

**Serilog** configured in `appsettings.json`. Logs to:
- Console (colored output)
- File: `logs/log-{Date}.txt` (structured text with timestamps)

**Usage**: Inject `ILogger<T>` or use static `Log.Information()`, `Log.Error()`, `Log.Warning()`, `Log.Fatal()`.

**Pattern**: Log important operations (user creation, data seeding, migrations) and all exceptions. Error middleware adds trace IDs automatically.

## Startup Sequence

On application start (in `Program.cs`):
1. Serilog configuration (before building the app)
2. Service registration (Identity, JWT, CORS, Resend, PostgreSQL, Hangfire, DI)
3. Middleware pipeline setup (Error handling → CORS → Auth → Authorization → Controllers)
4. **Database migration**: `context.Database.MigrateAsync()` (automatic)
5. **Data seeding**: `DataSeeder.Initialize()` creates roles, test users, sample data
6. **Mapster configuration**: `MapperExtensions.ConfigureMapster()` registers all mappings
7. Hangfire recurring jobs registration (dev only)

**Critical**: Seed runs on every startup but checks `if (!await context.Users.AnyAsync())` to avoid duplicates.

## Development Workflow

**Start Development**:
```bash
cd backend
dotnet watch --no-hot-reload  # Auto-recompile on file changes
```

**Database Reset** (when models change):
```bash
make db-restart  # Drops DB, applies migrations, starts watch
```

**Swagger**: Available at `https://localhost:5001/swagger` in Development mode (only).

**Hangfire Dashboard**: `http://localhost:5185/hangfire` (background jobs monitoring, Development only).

**CORS**: Configured to allow `http://localhost:3000` (Next.js frontend). Add more origins in `Program.cs` if needed.

## Common Patterns to Follow

1. **DTOs everywhere**: Never expose domain models directly in API responses
2. **Async/await**: All data access must be async
3. **Repository pattern**: Services call repositories, never DbContext directly
4. **JWT claims**: Always validate user identity from token, never trust route parameters for userId
5. **Rating field**: Read `GeneralUser.Rating` directly - it's maintained automatically by review system
6. **Mapster config**: Create dedicated mapper class for new entities, register in `MapperExtensions`
7. **NO emojis**: Never use emojis in code comments, documentation, commit messages, or any technical documentation

## File Naming Conventions

- DTOs: `{Entity}DTO.cs` in `src/Application/DTOs/{EntityName}DTO/`
- Services: `I{Entity}Service.cs` (interface) and `{Entity}Service.cs` (impl)
- Repositories: `I{Entity}Repository.cs` and `{Entity}Repository.cs`
- Controllers: `{Entity}Controller.cs` (no "Api" prefix)
- Models: `{Entity}.cs` in `src/Domain/Models/`

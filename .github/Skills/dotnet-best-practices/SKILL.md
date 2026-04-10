---
name: dotnet-best-practices
description: 'Enforce SportMania .NET 10 backend best practices when adding or refactoring controllers, services, repositories, EF Core data access, and API contracts. Use for code reviews, implementation tasks, and quality hardening.'
argument-hint: 'Scope and intent, for example: PaymentController update with DTO validation and service refactor'
user-invocable: true
---

# SportMania .NET Best Practices

## Outcome

Produce repository-consistent .NET changes for SportMania Backend by applying architecture, async, data-access, and API contract rules before code is considered complete.

## When To Use

Use this skill when the task involves any of these:

- Backend API changes in controllers, services, repositories, or DbContext
- Refactoring for cleaner layering and dependency injection
- Bug fixes that may affect payment flow, ToyyibPay integration, or Discord-related logic
- Quality checks to ensure async safety, DTO boundaries, and EF Core query correctness

Do not use this skill for:

- Frontend-only work
- Java, Python, or non-.NET repositories
- One-off text edits unrelated to backend behavior

## Inputs

- Requested change scope and acceptance criteria
- Files likely impacted under Backend/Controllers, Backend/Services, Backend/Repository, Backend/Data, Backend/Models
- Existing repository instructions under .github/instructions

## Procedure

1. Load governing rules first.
Read repository guidance from:
- .github/copilot-instructions.md
- .github/instructions/Controllers.instructions.md when controllers are touched
- .github/instructions/Services.instructions.md when services are touched
- .github/instructions/Repository.instructions.md when repositories or DbContext are touched

2. Map the change to architecture layers.
- HTTP concerns stay in controllers.
- Business rules stay in services.
- Persistence stays in repositories and DbContext.

3. Apply implementation rules by layer.
Controllers:
- Keep actions thin and return ActionResult or ActionResult<T>.
- Use proper model binding and response types.
- Translate domain/service exceptions to HTTP responses.

Services:
- Use async Task or Task<T> and include CancellationToken cancellationToken = default.
- Keep services stateless and dependency-injected through interfaces.
- Place validation and domain rules before I/O.
- Use ConfigureAwait(false) on awaits.

Repositories and Data:
- Use async EF Core APIs and pass cancellation tokens.
- Use AsNoTracking() for read-only queries.
- Persist via repository methods, not from controllers/services through DbContext.
- Keep entity mapping and relationship config in ApplicationDbContext.

4. Enforce contract and naming consistency.
- Use DTOs for API input/output; do not expose EF entities directly from API endpoints.
- Keep naming aligned with existing conventions.
- Preserve existing DI registration style in Backend/Program.cs.

5. Run validation checks.
- dotnet restore SportMania.sln
- dotnet build SportMania.sln
- dotnet test SportMania.sln when tests exist

6. Prepare a completion summary.
Include:
- What changed and why
- Risk or behavior impact
- Validation commands run and results
- Remaining follow-ups, if any

## Decision Points

- If a change requires both controller and service logic:
Implement service behavior first, then wire controller response mapping.

- If a task suggests direct DbContext use in controller/service:
Refactor to repository abstraction unless explicitly blocked by existing architecture constraints.

- If cancellation token is missing in new async I/O path:
Add token propagation through controller -> service -> repository.

- If an endpoint response currently exposes entity shape:
Introduce or extend DTOs and map explicitly.

## Quality Gates

All must pass before completion:

- Layering is respected with no business logic in controllers.
- Async calls are non-blocking and cancellation-aware.
- Service and repository awaits use ConfigureAwait(false).
- Read-only queries use AsNoTracking() where applicable.
- DTO boundaries are preserved for API contracts.
- Build passes; tests pass if present.

## Output Format

Return results in this structure:

- Summary: one paragraph
- Changes: file-by-file bullet list
- Validation: command list with pass/fail
- Risks: any regressions or unknowns
- Next actions: optional, only if needed

## Example Prompts

- Apply dotnet-best-practices to refactor PaymentController create flow and ensure proper service exception mapping.
- Apply dotnet-best-practices to optimize TransactionRepository read queries and enforce cancellation token propagation.
- Apply dotnet-best-practices to review Backend/Services for missing ConfigureAwait(false) and DTO boundary issues.

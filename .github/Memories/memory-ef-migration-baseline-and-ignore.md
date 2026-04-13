# Memory: EF migration baseline and ignore policy

## Metadata

- PatternId: MEMORY-DOTNET-001
- PatternVersion: 1
- Status: active
- Supersedes:
- CreatedAt: 2026-04-11
- LastValidatedAt: 2026-04-11
- ValidationEvidence: `dotnet ef database update` needed additive SQL migration due existing schema and no prior tracked migrations.

## Source Context

- Triggering task: Add one-time key view flag to transactions
- Scope/system: SportMania backend EF Core/PostgreSQL
- Date/time: 2026-04-11

## Memory

- Key fact or decision: The repository currently ignores `Migrations/` via `.gitignore`, while runtime database already contains existing tables.
- Why it matters: Scaffolded first migration may generate full create-table scripts that fail against existing databases.

## Applicability

- When to reuse: Any future EF schema updates in this repo.
- Preconditions/limitations: Applies unless `.gitignore` and migration strategy are intentionally changed.

## Actionable Guidance

- Recommended future action: Use additive/idempotent migration steps for existing environments, and decide explicitly whether migration files should be tracked in git.
- Related files/services/components: `Backend/Migrations/`, `.gitignore`, `Backend/Data/ApplicationDbContext.cs`.
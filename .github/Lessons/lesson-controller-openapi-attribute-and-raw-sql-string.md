# Lesson: Use correct ASP.NET response attributes and raw SQL quoting

## Metadata

- PatternId: LESSON-DOTNET-001
- PatternVersion: 1
- Status: active
- Supersedes:
- CreatedAt: 2026-04-11
- LastValidatedAt: 2026-04-11
- ValidationEvidence: Fixed compile errors in controller attributes and successfully applied EF migration SQL after quote correction.

## Task Context

- Triggering task: One-time key view flow for transactions
- Date/time: 2026-04-11
- Impacted area: Controllers, EF migrations

## Mistake

- What went wrong: Used `ProduceResponseType` instead of `ProducesResponseType`, and escaped double quotes inside C# raw string SQL.
- Expected behavior: Controllers compile with OpenAPI attributes; migration SQL executes in PostgreSQL.
- Actual behavior: Compile error for unknown attribute and SQL syntax error near backslash.

## Root Cause Analysis

- Primary cause: Mixing naming from prompt examples and normal string escaping habits inside raw string literals.
- Contributing factors: Fast multi-file edits and migration script adjustments under time pressure.
- Detection gap: Did not run compile and migration application immediately after initial edits.

## Resolution

- Fix implemented: Replaced with `ProducesResponseType` and removed escaping in raw string SQL.
- Why this fix works: ASP.NET recognizes the proper attribute type; PostgreSQL receives valid quoted identifiers.
- Verification performed: `dotnet build SportMania.sln` and `dotnet ef database update` both succeeded.

## Preventive Actions

- Guardrails added: Validate new controller attributes with `get_errors` before broader runs.
- Tests/checks added: Execute migration command once after any manual SQL edit.
- Process updates: Prefer framework-native migration operations unless provider-specific SQL is required.

## Reuse Guidance

- How to apply this lesson in future tasks: Use `ProducesResponseType` for controller response metadata and do not escape quotes inside `"""` raw SQL blocks.
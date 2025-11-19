# Copilot Instructions for Meme-It

These instructions guide GitHub Copilot (and AI assistants) when proposing or making changes in this repository. They enforce our architecture decisions, folder structure, and delivery workflow.

## Scope & Context
- Product: Meme-It — a multiplayer party game where players create and vote on memes.
- Backend: C#/.NET with Microsoft Aspire orchestration; modular structure by module/domain.
- Frontend: Angular (separate web app; not part of these rules unless explicitly referenced).
- Repo layout: `docs/` for documentation, `src/` for code. Modules live under `src/<Module>/...`. Aspire orchestration lives in `src/Aspire/`.

## Non‑Negotiables (Compliance)
All work MUST comply with our ADRs, designs, and recommendations available via the HexMaster Design Guidelines MCP server. At minimum, follow:
- ADR 0002: Modular Monolith Project Structure
- ADR 0003: Recommend .NET Aspire for ASP.NET Projects
- ADR 0004: CQRS Recommendation for ASP.NET API
- ADR 0005: Minimal APIs over Controllers
- Design: Modular Solution Structure
- Design: Pragmatic Domain-Driven Design
- Recommendation: Unit Testing with xUnit, Moq, Bogus

Notes:
- Target framework baseline follows ADR 0001 (Adopt .NET 10) where applicable. If a project is pinned lower, propose upgrade in a separate PR/ADR rather than silently changing TFMs.
- Do not introduce cross-module coupling. Respect abstractions and dependency rules.

## Required Workflow Before Any Code Change
Before you change code or files, you MUST present, in your reply/PR description:

> Important: Make sure to always show a list of assumptions and a task list before actually changing code. Ask for permission if the projected change is correct before continuing.

1) Assumptions
- Bullet the key assumptions you’re making based on code, docs, and ADRs.

2) Task List
- Bullet the concrete steps you plan to execute (3–7 concise steps).

3) Ask for Permission
- Explicitly ask: "Shall I proceed with these changes?" and wait for approval.

Only after approval should you implement the plan.

## Folder & Dependency Rules
- Keep current structure intact; do not relocate modules without explicit approval.
- Aspire projects reside under `src/Aspire/` (e.g., `HexMaster.MemeIt.Aspire.AppHost`, `...ServiceDefaults`).
- Module layout (example `Games`):
  - `HexMaster.MemeIt.Games` (core domain/application)
  - `HexMaster.MemeIt.Games.Abstractions` (contracts/ports/DTOs)
  - `HexMaster.MemeIt.Games.Data.<Store>` (infrastructure adapters, e.g., `MongoDb`)
  - `HexMaster.MemeIt.Games.Api` (Minimal API host)
  - `HexMaster.MemeIt.Games.Tests` (xUnit tests)
- Dependency direction: Data → Abstractions; Api → Core/Abstractions; Core never depends on Data; cross-module use only via Abstractions.

## API & Application Patterns
- Minimal APIs (ADR 0005): Group endpoints per feature using extension methods (see “Minimal API Endpoint Organization”). Keep lambdas thin; delegate to handlers.
- CQRS (ADR 0004):
  - Map request payloads to immutable records (Commands/Queries).
  - Implement `ICommandHandler<>`/`IQueryHandler<>` in application layer.
  - Handlers orchestrate; no HTTP/persistence logic inside handlers.
- Pragmatic DDD:
  - Rich domain models guard invariants with private setters and behavior methods.
  - Use value objects when they add domain clarity (multi-field, interdependent, or behavior-rich).
  - Repositories operate over aggregate roots only.
- Persistence:
  - Keep EF/driver specifics in Data projects; no persistence attributes in domain.
  - Map value objects via owned types/configurations (when using EF Core).

## Testing Policy
- Unit tests: xUnit with optional Moq and Bogus.
- Aim for ≥80% coverage in core and API modules.
- Use factories/builders for test data; seed Bogus when asserting specific values.
- Every code change must be backed by unit tests wherever practicable; keep overall solution coverage ≥80% and do not merge work that causes coverage regressions without explicit maintainer approval.

## MCP Server Usage (Mandatory)
- Use the HexMaster Design Guidelines MCP server to:
  - Search relevant ADRs/designs before proposing changes.
  - Cite the documents you applied (IDs/titles) in your Assumptions.
  - If a guideline conflicts with current code, propose an ADR or call it out explicitly.

## Pull Request Checklist
- Assumptions listed and justified with references to ADRs/guidelines.
- Task list provided and approved before implementation.
- Changes adhere to Minimal APIs, CQRS, and DDD rules above.
- Folder structure preserved; dependencies flow inward only.
- Tests added/updated with xUnit; coverage target considered.
- Aspire integration respected for new services/resources.

## Example Template (Use As-Is)

Assumptions
- The change touches `src/Games/HexMaster.MemeIt.Games.Api` only.
- ADRs applied: 0002 (Modular Monolith), 0004 (CQRS), 0005 (Minimal APIs).
- No cross-module contracts need updates.

Task List
- Add endpoint group `MemeEndpoints` in `Endpoints/` with Minimal API style.
- Introduce `CreateMemeCommand` + handler in Core project.
- Wire DI registrations and map endpoints.
- Add xUnit tests for handler and endpoint routing.

Shall I proceed with these changes?

## Notes for Maintainers
- If Copilot proposes changes without an Assumptions + Task List + Permission step, request revision.
- If guidelines evolve, update the references above and link the new ADR.

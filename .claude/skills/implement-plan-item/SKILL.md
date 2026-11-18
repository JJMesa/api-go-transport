---
name: implement-plan-item
description: Implement an item of the improvement plan (F-xx in docs/audit/INFORME_AUDITORIA.md §6.2) and record it in the plan and the commit. Use when asked to implement, fix, work on or close a plan item such as F-07.
---

# Implement plan item

1. **Scope.** Find the item's row in §6.2 of `docs/audit/INFORME_AUDITORIA.md` and its evidence (the matching finding in §5, or §6.4 for F-26 onward — search the document for the ID and the code symbols it names). Restate the scope in one sentence. When the row bundles several actions or the evidence no longer matches the code, confirm the scope with the user before writing code.
2. **Implement**, following the `CLAUDE.md` of every folder you touch. For a bug, write the failing test first so it goes red on the defect.
3. **Verify.** `dotnet build GoTransport.sln` adds no new warnings, and both test projects pass (`dotnet test tests/GoTransport.Api.Unit.Tests`, `dotnet test tests/GoTransport.Api.Component.Tests`).
4. **Record** in the same change:
   - The item's row: `[x]`, Estado `✅ Hecho`, Responsable as the existing rows write it (e.g. `Juan Mesa`), Verificado `✅`; leave PR / Commit as `—`.
   - §6.3: recount every column from the §6.2 table and recompute the completion percentage.
   - Any `CLAUDE.md` whose statement the change made false (e.g. a gotcha that cites this item).
5. **Commit** on the current branch: subject `[Fix|Feature|Refactor|Test] <summary>` — `Fix` for defects, `Feature` for new capability, `Refactor` for structure, `Test` for test-only changes — and a body that says `Implements F-xx.` Stage the item's files by path; developers keep unrelated local edits (notably `src/GoTransport.Api/appsettings.json`) out of the commit. Pushing is left to the user.

**Done when** the commit exists, the row and §6.3 match the table, and both test projects are green.

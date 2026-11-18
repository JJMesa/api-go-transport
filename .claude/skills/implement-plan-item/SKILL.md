---
name: implement-plan-item
description: Implement an item of the improvement plan (F-xx in docs/audit/TECHNICAL_AUDIT.md §7.2) and record it in the plan and the commit. Use when asked to implement, fix, work on or close a plan item such as F-07.
---

# Implement plan item

1. **Scope.** Find the item's row in §7.2 of `docs/audit/TECHNICAL_AUDIT.md`; the row names its findings (`A-xx`), whose evidence and recommendation are in §5. Restate the scope in one sentence. When the row bundles several actions or the evidence no longer matches the code, confirm the scope with the user before writing code.
2. **Implement**, following the `CLAUDE.md` of every folder you touch. For a bug, write the failing test first so it goes red on the defect.
3. **Verify.** `dotnet build GoTransport.sln` adds no new warnings, and both test projects pass (`dotnet test tests/GoTransport.Api.Unit.Tests`, `dotnet test tests/GoTransport.Api.Component.Tests`).
4. **Record** in the same change:
   - The item's row: `[x]`, Status `✅ Done`, Owner as the existing rows write it (e.g. `Juan Mesa`), Verified `✅`; leave PR / Commit as `—`. Tick the matching quick win in §7.1 too, if there is one.
   - §5: move each finding the item closes from §5.1 to the §5.2 table, naming the item and commit.
   - §7.3: recount every column from the §7.2 table and recompute the completion percentage.
   - Any `CLAUDE.md` whose statement the change made false (e.g. a gotcha that cites this item).
5. **Commit** on the current branch: subject `[Fix|Feature|Refactor|Test] <summary>` — `Fix` for defects, `Feature` for new capability, `Refactor` for structure, `Test` for test-only changes — and a body that says `Implements F-xx.` Stage the item's files by path; developers keep unrelated local edits (notably `src/GoTransport.Api/appsettings.json`) out of the commit. Pushing is left to the user.

**Done when** the commit exists, the row, §5 and §7.3 match the table, and both test projects are green.

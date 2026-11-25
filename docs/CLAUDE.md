# docs/ — project documentation

**Responsibility:** documentation that isn't code and isn't domain vocabulary: the technical audit with its improvement plan, and architecture decision records.

## Contents

- `audit/TECHNICAL_AUDIT.md` — technical audit (baseline June 2026, re-assessed November 2026). §3 compares the dimension scores; §5 holds the findings (`A-xx`: open, resolved, no longer applicable) with their evidence; §7.2 is the improvement plan (`F-xx`), the source of truth for what to work on and what each commit references; §7.3 summarizes progress.
- `adr/` — architecture decision records, numbered `NNNN-title.md` (format from the `domain-modeling` skill). `0001` keeps the `JsonResponse` envelope instead of ProblemDetails.

## Conventions

- Update a plan item's row (✔, Status, Owner, Verified), move its findings to §5.2, and recount §7.3 in the same commit that implements it — the `implement-plan-item` skill does this.
- The audit is written in English. Finding (`A-xx`) and plan (`F-xx`) IDs are stable and never renumbered: new ones continue the sequence, and an obsolete plan item is marked ❌ Won't do with its reason.
- Record an ADR only for a decision that is hard to reverse, surprising without context, and the result of a real trade-off.
- Domain terms belong in `/GLOSSARY.md`.

# docs/ — project documentation

**Responsibility:** documentation that isn't code and isn't domain vocabulary: the technical audit with its improvement plan, and architecture decision records.

## Contents

- `audit/INFORME_AUDITORIA.md` — technical audit (June 2026, Spanish). §6.2 is the improvement plan (`F-01`…): the source of truth for what to work on and what each commit references. §6.3 summarizes progress; §6.4 holds the evidence for findings added after the audit.
- `adr/` — architecture decision records, created with the first one (format from the `domain-modeling` skill).

## Conventions

- Update a plan item's row (✔, Estado, Responsable, Verificado) and the §6.3 counts in the same commit that implements it — the `implement-plan-item` skill does this.
- The audit is written in Spanish; new rows and findings follow the document's language and table format.
- Record an ADR only for a decision that is hard to reverse, surprising without context, and the result of a real trade-off.
- Domain terms belong in `/GLOSSARY.md`.

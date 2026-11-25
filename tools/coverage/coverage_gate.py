"""Coverage gate for CI: fails when the merged coverage drops under the thresholds or a method's CRAP
score is too high.

Reads the Cobertura report that ReportGenerator merges from both test suites (coverlet writes each
method's cyclomatic complexity) and computes, per method, the same score as ReportGenerator's Risk Hotspots:

    CRAP = complexity^2 * (1 - coverage)^3 + complexity

where coverage is the method's line rate. A method with CRAP above the limit is complex code that the
tests barely run. Writes a summary to $GITHUB_STEP_SUMMARY when it is set.

Usage: python3 coverage_gate.py <Cobertura.xml> [--min-line 80] [--min-branch 60] [--max-crap 30]
"""
import argparse
import os
import sys
import xml.etree.ElementTree as ET


def crap(complexity: float, coverage: float) -> float:
    return complexity ** 2 * (1 - coverage) ** 3 + complexity


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("report")
    parser.add_argument("--min-line", type=float, default=80.0)
    parser.add_argument("--min-branch", type=float, default=60.0)
    parser.add_argument("--max-crap", type=float, default=30.0)
    args = parser.parse_args()

    root = ET.parse(args.report).getroot()
    line = float(root.get("line-rate", 0)) * 100
    branch = float(root.get("branch-rate", 0)) * 100

    methods = []
    for cls in root.iter("class"):
        for method in cls.iter("method"):
            complexity = float(method.get("complexity", 1) or 1)
            coverage = float(method.get("line-rate", 0))
            methods.append((crap(complexity, coverage), complexity, coverage * 100, f"{cls.get('name')}.{method.get('name')}"))
    methods.sort(reverse=True)
    offenders = [m for m in methods if m[0] > args.max_crap]

    failures = []
    if line < args.min_line:
        failures.append(f"line coverage {line:.1f}% is under {args.min_line:.0f}%")
    if branch < args.min_branch:
        failures.append(f"branch coverage {branch:.1f}% is under {args.min_branch:.0f}%")
    for score, complexity, coverage, name in offenders:
        failures.append(f"CRAP {score:.1f} > {args.max_crap:.0f}: {name} (complexity {complexity:.0f}, {coverage:.0f}% covered)")

    lines = [
        "## Coverage gate",
        "",
        "| Metric | Value | Limit |",
        "|---|---:|---:|",
        f"| Line coverage | {line:.1f}% | >= {args.min_line:.0f}% |",
        f"| Branch coverage | {branch:.1f}% | >= {args.min_branch:.0f}% |",
        f"| Methods above CRAP {args.max_crap:.0f} | {len(offenders)} | 0 |",
        "",
        "Highest CRAP scores:",
        "",
        "| CRAP | Complexity | Line coverage | Method |",
        "|---:|---:|---:|---|",
    ]
    lines += [f"| {s:.1f} | {c:.0f} | {cov:.0f}% | `{n}` |" for s, c, cov, n in methods[:10]]
    lines += ["", "**Result:** " + ("passed" if not failures else "failed")]
    lines += [f"- {f}" for f in failures]
    summary = "\n".join(lines) + "\n"

    print(summary)
    step_summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if step_summary:
        with open(step_summary, "a", encoding="utf-8") as handle:
            handle.write(summary)

    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())

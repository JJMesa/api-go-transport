"""Coverage gate for CI: fails when the merged coverage drops under the thresholds or a method's CRAP
score is too high.

Reads the Cobertura report that ReportGenerator merges from both test suites (coverlet writes each
method's cyclomatic complexity) and computes, per method, the same score as ReportGenerator's Risk Hotspots:

    CRAP = complexity^2 * (1 - coverage)^3 + complexity

where coverage is the method's line rate. A method with CRAP above the limit is complex code that the
tests barely run. Writes a summary to $GITHUB_STEP_SUMMARY when it is set.

Usage: python3 coverage_gate.py <Cobertura.xml> [--min-line 80] [--min-branch 60] [--max-crap 30]
Tests: python3 -m unittest discover -s tools/coverage
"""
import argparse
import math
import os
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field


@dataclass
class MethodScore:
    crap: float
    complexity: float
    coverage: float  # line coverage, 0-100
    name: str


@dataclass
class GateResult:
    line: float  # 0-100
    branch: float  # 0-100
    methods: list[MethodScore] = field(default_factory=list)  # highest CRAP first
    offenders: list[MethodScore] = field(default_factory=list)
    failures: list[str] = field(default_factory=list)

    @property
    def passed(self) -> bool:
        return not self.failures


def crap(complexity: float, coverage: float) -> float:
    return complexity ** 2 * (1 - coverage) ** 3 + complexity


def number(value: str | None, default: float) -> float:
    """Parses a report attribute. A missing, malformed or NaN value gives the default: coverlet writes
    complexity="NaN" for some property accessors, and a NaN score would neither sort nor exceed a limit."""
    try:
        parsed = float(value)  # type: ignore[arg-type]
    except (TypeError, ValueError):
        return default
    return default if math.isnan(parsed) else parsed


def analyse(report: str, min_line: float = 80.0, min_branch: float = 60.0, max_crap: float = 30.0) -> GateResult:
    root = ET.parse(report).getroot()
    result = GateResult(line=number(root.get("line-rate"), 0.0) * 100, branch=number(root.get("branch-rate"), 0.0) * 100)

    for cls in root.iter("class"):
        for method in cls.iter("method"):
            # An unknown complexity counts as 1 (a single path); an unknown coverage as 0, so it can still fail.
            complexity = number(method.get("complexity"), 1.0)
            coverage = number(method.get("line-rate"), 0.0)
            name = f"{cls.get('name')}.{method.get('name')}"
            result.methods.append(MethodScore(crap(complexity, coverage), complexity, coverage * 100, name))

    result.methods.sort(key=lambda m: (-m.crap, m.name))
    result.offenders = [m for m in result.methods if m.crap > max_crap]

    if result.line < min_line:
        result.failures.append(f"line coverage {result.line:.1f}% is under {min_line:.0f}%")
    if result.branch < min_branch:
        result.failures.append(f"branch coverage {result.branch:.1f}% is under {min_branch:.0f}%")
    for m in result.offenders:
        result.failures.append(f"CRAP {m.crap:.1f} > {max_crap:.0f}: {m.name} (complexity {m.complexity:.0f}, {m.coverage:.0f}% covered)")
    return result


def summary(result: GateResult, min_line: float, min_branch: float, max_crap: float, top: int = 10) -> str:
    lines = [
        "## Coverage gate",
        "",
        "| Metric | Value | Limit |",
        "|---|---:|---:|",
        f"| Line coverage | {result.line:.1f}% | >= {min_line:.0f}% |",
        f"| Branch coverage | {result.branch:.1f}% | >= {min_branch:.0f}% |",
        f"| Methods above CRAP {max_crap:.0f} | {len(result.offenders)} | 0 |",
        "",
        "Highest CRAP scores:",
        "",
        "| CRAP | Complexity | Line coverage | Method |",
        "|---:|---:|---:|---|",
    ]
    lines += [f"| {m.crap:.1f} | {m.complexity:.0f} | {m.coverage:.0f}% | `{m.name}` |" for m in result.methods[:top]]
    lines += ["", "**Result:** " + ("passed" if result.passed else "failed")]
    lines += [f"- {f}" for f in result.failures]
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("report")
    parser.add_argument("--min-line", type=float, default=80.0)
    parser.add_argument("--min-branch", type=float, default=60.0)
    parser.add_argument("--max-crap", type=float, default=30.0)
    args = parser.parse_args()

    result = analyse(args.report, args.min_line, args.min_branch, args.max_crap)
    text = summary(result, args.min_line, args.min_branch, args.max_crap)

    print(text)
    step_summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if step_summary:
        with open(step_summary, "a", encoding="utf-8") as handle:
            handle.write(text)

    return 0 if result.passed else 1


if __name__ == "__main__":
    sys.exit(main())

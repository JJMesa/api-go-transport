"""Tests of coverage_gate.py against small Cobertura reports. Run: python3 -m unittest discover -s tools/coverage"""
import os
import tempfile
import unittest

from coverage_gate import analyse, number, summary

TEMPLATE = """<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="{line}" branch-rate="{branch}">
  <packages><package name="GoTransport.Sample"><classes>
    <class name="GoTransport.Sample.Service" filename="Service.cs">
      <methods>{methods}</methods>
    </class>
  </classes></package></packages>
</coverage>
"""


def method(name: str, complexity: str, line_rate: str) -> str:
    return f'<method name="{name}" signature="()" line-rate="{line_rate}" branch-rate="1" complexity="{complexity}" />'


class CoverageGateTests(unittest.TestCase):
    def analyse(self, methods: list[str], line: str = "0.9", branch: str = "0.7", **limits):
        handle, path = tempfile.mkstemp(suffix=".xml")
        with os.fdopen(handle, "w", encoding="utf-8") as report:
            report.write(TEMPLATE.format(line=line, branch=branch, methods="".join(methods)))
        self.addCleanup(os.remove, path)
        return analyse(path, **limits)

    def test_passes_a_well_covered_report(self):
        result = self.analyse([method("Simple", "2", "1")])

        self.assertTrue(result.passed)
        self.assertEqual([], result.offenders)

    def test_ranks_the_highest_crap_first_when_a_complexity_is_nan(self):
        # 4 ^ 2 * 0.8 ^ 3 + 4 = 12.19 must lead, whatever the NaN accessor does to the order
        result = self.analyse([
            method("Accessor", "NaN", "1"),
            method("FullyCovered", "10", "1"),
            method("Transaction", "4", "0.2"),
            method("OtherAccessor", "NaN", "1"),
        ])

        self.assertEqual(
            ["GoTransport.Sample.Service.Transaction", "GoTransport.Sample.Service.FullyCovered"],
            [m.name for m in result.methods[:2]])
        self.assertAlmostEqual(12.19, result.methods[0].crap, places=2)
        self.assertEqual(1, next(m for m in result.methods if m.name.endswith(".Accessor")).complexity)

    def test_fails_on_a_method_above_the_crap_limit(self):
        # 8 ^ 2 * 1 + 8 = 72
        result = self.analyse([method("Untested", "8", "0"), method("Simple", "1", "1")])

        self.assertFalse(result.passed)
        self.assertEqual(["GoTransport.Sample.Service.Untested"], [m.name for m in result.offenders])
        self.assertIn("CRAP 72.0 > 30", result.failures[0])

    def test_an_unknown_complexity_on_an_untested_method_can_still_count(self):
        # complexity 1, coverage 0: CRAP 2, an offender once the limit is below it
        result = self.analyse([method("Accessor", "NaN", "0")], max_crap=1.5)

        self.assertEqual(["GoTransport.Sample.Service.Accessor"], [m.name for m in result.offenders])

    def test_fails_under_the_line_and_branch_minimums(self):
        result = self.analyse([method("Simple", "1", "1")], line="0.79", branch="0.59")

        self.assertEqual(
            ["line coverage 79.0% is under 80%", "branch coverage 59.0% is under 60%"],
            result.failures)

    def test_summary_reports_the_result_and_the_failures(self):
        result = self.analyse([method("Untested", "8", "0")])

        text = summary(result, 80, 60, 30)

        self.assertIn("| Methods above CRAP 30 | 1 | 0 |", text)
        self.assertIn("**Result:** failed", text)
        self.assertIn("`GoTransport.Sample.Service.Untested`", text)

    def test_number_falls_back_on_missing_malformed_and_nan_values(self):
        self.assertEqual(1.0, number(None, 1.0))
        self.assertEqual(1.0, number("", 1.0))
        self.assertEqual(1.0, number("NaN", 1.0))
        self.assertEqual(1.0, number("nan", 1.0))
        self.assertEqual(3.0, number("3", 1.0))


if __name__ == "__main__":
    unittest.main()

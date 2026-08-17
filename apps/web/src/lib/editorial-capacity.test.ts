import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const component = readFileSync(new URL("../components/admin/editorial-command-center.tsx", import.meta.url), "utf8");
const styles = readFileSync(new URL("../app/globals.css", import.meta.url), "utf8");

test("editorial capacity desk is localized and exposes real workload evidence", () => {
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"])
    assert.match(component, new RegExp(`"${locale}"`));
  assert.match(component, /data\.workloads\.map/);
  assert.match(component, /summary\.unassigned/);
  assert.match(component, /summary\.teamMembers/);
});

test("task reassignment is csrf protected and responsive", () => {
  assert.match(component, /x-csrf-token/);
  assert.match(component, /canReassign/);
  assert.match(component, /\/api\/admin\/editorial\/tasks\/\$\{item\.taskId\}\/assignee/);
  assert.match(styles, /@media\(max-width:600px\)[\s\S]*\.capacity-grid\{grid-template-columns:1fr\}/);
});

test("quality debt is actionable, localized, and uses the real command queue", () => {
  assert.match(component, /filter === "quality"/);
  assert.match(component, /item\.missingGates\.map/);
  assert.match(component, /QualityGate/);
  assert.match(component, /CoverAccessibility/);
  assert.match(styles, /\.quality-debt-gates/);
});

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const overview = readFileSync(new URL("../components/admin/admin-overview.tsx", import.meta.url), "utf8");
const styles = readFileSync(new URL("../app/globals.css", import.meta.url), "utf8");

test("release reliability desk exposes localized SLO, latency and streak evidence", () => {
  assert.match(overview, /deploymentReliability/);
  assert.match(overview, /successRate/);
  assert.match(overview, /medianDurationSeconds/);
  assert.match(overview, /p95DurationSeconds/);
  assert.match(overview, /healthyStreak/);
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"]) assert.match(overview, new RegExp(`"${locale}"`));
  assert.match(styles, /\.release-slo/);
  assert.match(styles, /reliability-atrisk/);
});

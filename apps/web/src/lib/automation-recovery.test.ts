import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const component=readFileSync(new URL("../components/admin/development-live.tsx",import.meta.url),"utf8");
const styles=readFileSync(new URL("../app/globals.css",import.meta.url),"utf8");

test("development desk exposes localized autonomous recovery evidence",()=>{
  for(const locale of ["tr-TR","en-US","de-DE","fr-FR"])assert.match(component,new RegExp(`"${locale}"`));
  for(const field of ["heartbeatHealthy","consecutiveFailures","automaticRecoveries","nextRetryAt"])assert.match(component,new RegExp(field));
  assert.match(component,/development-recovery/);
  assert.match(styles,/\.development-recovery\.is-risk/);
  assert.match(styles,/\.recovery-metrics/);
});

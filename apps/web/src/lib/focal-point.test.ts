import assert from "node:assert/strict";
import test from "node:test";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { focalPointStyle } from "./focal-point.ts";

test("focal point defaults to center and clamps unsafe API values", () => {
  assert.equal(focalPointStyle(null).objectPosition, "50% 50%");
  assert.equal(focalPointStyle({ focalX: -.2, focalY: 1.5 }).objectPosition, "0% 100%");
  assert.equal(focalPointStyle({ focalX: .25, focalY: .75 }).objectPosition, "25% 75%");
});

test("public discovery and member card surfaces preserve the editorial focal point", () => {
  const surfaces = [
    "src/app/[locale]/[collection]/[slug]/page.tsx",
    "src/app/[locale]/articles/[slug]/page.tsx",
    "src/app/[locale]/search/page.tsx",
    "src/app/[locale]/sources/[domain]/page.tsx",
    "src/app/[locale]/topics/page.tsx",
    "src/components/account-dashboard.tsx",
    "src/components/continue-reading.tsx",
    "src/components/member-onboarding.tsx",
  ];

  for (const surface of surfaces) {
    const source = readFileSync(resolve(process.cwd(), surface), "utf8");
    assert.match(source, /style=\{focalPointStyle\([^)]+\)\}/, surface);
  }
});

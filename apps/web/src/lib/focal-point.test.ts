import assert from "node:assert/strict";
import test from "node:test";
import { focalPointStyle } from "./focal-point.ts";

test("focal point defaults to center and clamps unsafe API values", () => {
  assert.equal(focalPointStyle(null).objectPosition, "50% 50%");
  assert.equal(focalPointStyle({ focalX: -.2, focalY: 1.5 }).objectPosition, "0% 100%");
  assert.equal(focalPointStyle({ focalX: .25, focalY: .75 }).objectPosition, "25% 75%");
});

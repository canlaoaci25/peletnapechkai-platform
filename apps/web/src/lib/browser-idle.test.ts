import assert from "node:assert/strict";
import test from "node:test";
import { scheduleWhenIdle } from "./browser-idle.ts";

test("desteklenen tarayicida isi bos zamana planlar ve iptal eder", () => {
  let scheduled: (() => void) | undefined;
  let timeout: number | undefined;
  let cancelled: number | undefined;
  const originalWindow = globalThis.window;

  Object.defineProperty(globalThis, "window", {
    configurable: true,
    value: {
      requestIdleCallback(callback: () => void, options: { timeout: number }) {
        scheduled = callback;
        timeout = options.timeout;
        return 42;
      },
      cancelIdleCallback(handle: number) { cancelled = handle; },
    },
  });

  let called = false;
  const cancel = scheduleWhenIdle(() => { called = true; }, 900);
  assert.equal(timeout, 900);
  assert.equal(called, false);
  scheduled?.();
  assert.equal(called, true);
  cancel();
  assert.equal(cancelled, 42);

  Object.defineProperty(globalThis, "window", { configurable: true, value: originalWindow });
});

test("idle API yoksa kisa ve iptal edilebilir zaman asimi kullanir", () => {
  let delay: number | undefined;
  let cleared: number | undefined;
  const originalWindow = globalThis.window;

  Object.defineProperty(globalThis, "window", {
    configurable: true,
    value: {
      setTimeout(_callback: () => void, value: number) { delay = value; return 7; },
      clearTimeout(handle: number) { cleared = handle; },
    },
  });

  const cancel = scheduleWhenIdle(() => undefined, 1500);
  assert.equal(delay, 250);
  cancel();
  assert.equal(cleared, 7);

  Object.defineProperty(globalThis, "window", { configurable: true, value: originalWindow });
});

import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { pushCopy } from "../i18n/push-copy.ts";
const locales=["tr-TR","en-US","de-DE","fr-FR"] as const;

test("push consent is locale complete and only requested by an explicit action",()=>{
  for(const locale of locales){assert.ok(pushCopy[locale].title);assert.ok(pushCopy[locale].enable);assert.ok(pushCopy[locale].quietHelp);}
  const component=readFileSync(fileURLToPath(new URL("../components/push-preferences.tsx",import.meta.url)),"utf8");
  assert.match(component,/onClick=\{\(\)=>void enable\(\)\}/);
  assert.match(component,/Notification\.requestPermission\(\)/);
  assert.doesNotMatch(component.slice(component.indexOf("useEffect"),component.indexOf("async function csrf")),/requestPermission/);
  assert.match(component,/role=\{message===copy\.failed\?"alert":"status"\}/);
});

test("push worker opens only the payload destination",()=>{
  const worker=readFileSync(fileURLToPath(new URL("../../public/boecl-push-worker.js",import.meta.url)),"utf8");
  assert.match(worker,/showNotification/);assert.match(worker,/notificationclick/);assert.match(worker,/clients\.openWindow/);
});

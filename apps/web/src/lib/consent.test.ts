import assert from "node:assert/strict";
import test from "node:test";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { consentStorageKey, hasOptionalConsent } from "./consent.ts";

test("yalnız açık izin isteğe bağlı entegrasyonları etkinleştirir", () => {
  assert.equal(hasOptionalConsent({ getItem: key => key === consentStorageKey ? "granted" : null }), true);
  assert.equal(hasOptionalConsent({ getItem: () => "denied" }), false);
  assert.equal(hasOptionalConsent({ getItem: () => null }), false);
});

test("onay değişikliği polling olmadan aynı ve farklı sekmelere yayılır", () => {
  const banner = readFileSync(fileURLToPath(new URL("../components/consent-banner.tsx", import.meta.url)), "utf8");
  const integrations = readFileSync(fileURLToPath(new URL("../components/third-party-integrations.tsx", import.meta.url)), "utf8");
  const adSlot = readFileSync(fileURLToPath(new URL("../components/ad-slot.tsx", import.meta.url)), "utf8");

  assert.match(banner, /dispatchEvent\(new Event\(consentChangeEvent\)\)/);
  for (const source of [integrations, adSlot]) {
    assert.match(source, /addEventListener\("storage",onStorage\)/);
    assert.match(source, /addEventListener\(consentChangeEvent,update\)/);
  }
  assert.doesNotMatch(integrations, /setInterval/);
  assert.match(integrations, /\{allowed&&adsense&&/);
});

test("onay bölgesi yerelleştirilmiş erişilebilir ad ve mobil dokunma hedefleri sunar", () => {
  const banner = readFileSync(fileURLToPath(new URL("../components/consent-banner.tsx", import.meta.url)), "utf8");
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");

  for (const title of ["Gizlilik tercihleri", "Privacy choices", "Datenschutzeinstellungen", "Choix de confidentialité"]) {
    assert.match(banner, new RegExp(title));
  }
  assert.match(banner, /aria-labelledby="consent-title"/);
  assert.match(banner, /<h2 className="sr-only" id="consent-title">/);
  assert.match(styles, /\.consent-banner button\s*\{[\s\S]*?min-height:\s*44px/);
  assert.match(styles, /\.consent-banner\s*\{[\s\S]*?z-index:\s*100/);
});

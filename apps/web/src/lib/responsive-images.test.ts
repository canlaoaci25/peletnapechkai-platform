import assert from "node:assert/strict";
import test from "node:test";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { homeImageSizes } from "./responsive-images.ts";

test("ana sayfa görselleri gerçek mobil kart genişliklerini bildirir", () => {
  assert.match(homeImageSizes.lead, /calc\(100vw - 28px\)/);
  assert.match(homeImageSizes.secondary, /120px/);
  assert.match(homeImageSizes.pick, /calc\(50vw - 35px\)/);
  assert.match(homeImageSizes.latest, /110px, 240px/);
});

test("yayın görselleri Next.js optimizasyon hattını kullanır", () => {
  const homePage = readFileSync(fileURLToPath(new URL("../app/[locale]/page.tsx", import.meta.url)), "utf8");
  const articlePage = readFileSync(fileURLToPath(new URL("../app/[locale]/articles/[slug]/page.tsx", import.meta.url)), "utf8");

  assert.doesNotMatch(homePage, /\bunoptimized\b/);
  assert.doesNotMatch(articlePage, /\bunoptimized\b/);
  assert.doesNotMatch(homePage, /\bpriority\b/);
  assert.doesNotMatch(articlePage, /\bpriority\b/);
  assert.match(homePage, /preload=\{preload\}/);
  assert.match(articlePage, /\bpreload\b/);
});

test("genel yayın kabuğu yerelleştirilmiş navigasyon ve ekran altı render sözleşmesini korur", () => {
  const header = readFileSync(fileURLToPath(new URL("../components/site-header.tsx", import.meta.url)), "utf8");
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");

  for (const label of ["Hesap", "Account", "Konto", "Compte"]) assert.match(header, new RegExp(`account:\"${label}\"`));
  assert.match(header, /aria-label=\{c\.account\}/);
  assert.doesNotMatch(header, /aria-label=\"Account\"/);
  assert.match(styles, /\.picks-section,[\s\S]*content-visibility:\s*auto/);
  assert.match(styles, /contain-intrinsic-block-size:\s*auto 700px/);
});

test("dar ekran basligi eylemleri ayri satira alir ve pahali bulanikligi kapatir", () => {
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");
  const marker = "@media (max-width: 480px) {\n  .site-header";
  const start = styles.indexOf(marker);
  const narrowViewport = start >= 0 ? styles.slice(start, start + 800) : "";

  assert.match(narrowViewport, /grid-template-columns:\s*minmax\(0, 1fr\) auto/);
  assert.match(narrowViewport, /\.header-actions\s*\{[\s\S]*?grid-column:\s*1 \/ -1/);
  assert.match(narrowViewport, /\.site-menu nav\s*\{[\s\S]*?max-width:\s*calc\(100vw - 28px\)/);
  assert.match(narrowViewport, /\.theme-lamp\s*\{[\s\S]*?backdrop-filter:\s*none/);
});

test("kritik olmayan kamu istekleri mobil yukleme yolunu bloke etmez", () => {
  const accountActions = readFileSync(fileURLToPath(new URL("../components/account-actions.tsx", import.meta.url)), "utf8");
  const engagement = readFileSync(fileURLToPath(new URL("../components/article-engagement.tsx", import.meta.url)), "utf8");
  const idleHelper = readFileSync(fileURLToPath(new URL("browser-idle.ts", import.meta.url)), "utf8");

  assert.match(accountActions, /scheduleWhenIdle\(\(\)\s*=>\s*\{void fetch\("\/api\/admin\/auth\/session"/);
  assert.match(engagement, /scheduleWhenIdle\(\(\)\s*=>\s*\{void send\("view"\)/);
  assert.match(idleHelper, /requestIdleCallback\(callback, \{ timeout \}\)/);
  assert.match(idleHelper, /Math\.min\(timeout, 250\)/);
  assert.match(accountActions, /active=false;cancelIdle\(\)/);
  assert.match(engagement, /cancelIdle\(\)/);
});

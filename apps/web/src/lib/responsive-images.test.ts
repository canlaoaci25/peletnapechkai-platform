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

test("ana sayfa gorsel baglantilari yinelenen klavye duraklari olusturmaz", () => {
  const homePage = readFileSync(fileURLToPath(new URL("../app/[locale]/page.tsx", import.meta.url)), "utf8");

  assert.match(homePage, /function ArticleImageLink/);
  assert.match(homePage, /tabIndex=\{-1\}/);
  assert.match(homePage, /aria-hidden="true"/);
  assert.match(homePage, /<Image src=\{article\.cover\.url\} alt=""/);
  assert.doesNotMatch(homePage, /<Link className="(?:lead|secondary|pick|category|latest)-image"/);
});

test("mobil konu seridi kaydirilabilirligini gorsel olarak belli eder", () => {
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");

  assert.doesNotMatch(styles, /\.topic-strip\s*\{[^}]*scrollbar-width:\s*none/);
  assert.match(styles, /@media\(max-width:700px\)\{\.topic-strip\{[^}]*scrollbar-width:thin/);
  assert.match(styles, /\.topic-strip::-webkit-scrollbar\{height:5px\}/);
});

test("arama sayfasi mobil klavyeyi kendiliginden acmaz ve dar ekranda tasmaz", () => {
  const searchPage = readFileSync(fileURLToPath(new URL("../app/[locale]/search/page.tsx", import.meta.url)), "utf8");
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");

  assert.doesNotMatch(searchPage, /\bautoFocus\b/);
  assert.match(searchPage, /type="search"/);
  assert.match(searchPage, /enterKeyHint="search"/);
  assert.match(searchPage, /<button type="submit">/);
  assert.match(styles, /@media \(max-width: 480px\) \{[\s\S]*?\.search-page form div \{[\s\S]*?flex-direction: column/);
  assert.match(styles, /\.search-page input,[\s\S]*?\.search-page button \{[\s\S]*?min-height: 44px/);
});

test("mobil menu dokunma, kisa ekran ve yuksek karsitlik sozlesmesini korur", () => {
  const styles = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");

  assert.match(styles, /\.site-menu summary\s*\{[^}]*min-width:44px[^}]*min-height:44px/);
  assert.match(styles, /\.site-menu nav\s*\{[^}]*max-height:calc\(100dvh - 96px\)[^}]*overflow-y:auto[^}]*overscroll-behavior:contain/);
  assert.match(styles, /\.site-menu nav a\s*\{[^}]*min-height:44px/);
  assert.match(styles, /@media \(forced-colors: active\)\s*\{[\s\S]*?outline: 3px solid CanvasText/);
});

test("izinli ucuncu taraf betikleri ana yukleme yolundan sonra calisir", () => {
  const integrations = readFileSync(fileURLToPath(new URL("../components/third-party-integrations.tsx", import.meta.url)), "utf8");

  assert.equal((integrations.match(/strategy="lazyOnload"/g) ?? []).length, 4);
  assert.doesNotMatch(integrations, /strategy="afterInteractive"/);
  assert.match(integrations, /allowed&&clarity/);
  assert.match(integrations, /allowed&&adsense/);
  assert.match(integrations, /allowed&&ga/);
});

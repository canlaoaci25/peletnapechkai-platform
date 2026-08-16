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
});

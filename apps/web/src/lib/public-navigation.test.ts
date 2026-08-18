import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const read = (path: string) => readFileSync(new URL(path, import.meta.url), "utf8");

test("public navigation keeps dynamic taxonomy and an accessible persistent desktop rail", () => {
  const navigation = read("../components/public-navigation.tsx");
  const header = read("../components/site-header.tsx");
  const css = read("../app/globals.css");
  assert.match(header, /getPublicArchiveIndex\(locale\)/);
  assert.match(navigation, /categories\.map/);
  assert.match(navigation, /aria-pressed=\{collapsed\}/);
  assert.match(navigation, /boecl-public-nav-collapsed/);
  assert.match(css, /data-public-nav=collapsed/);
  assert.match(css, /@media\(max-width:1023px\)[\s\S]*\.sidebar-collapse\{display:none\}/);
});

test("mobile drawer retains focus, escape, overlay and scroll-lock safeguards", () => {
  const navigation = read("../components/public-navigation.tsx");
  for (const evidence of [/event\.key === "Escape"/, /event\.key !== "Tab"/, /document\.body\.style\.overflow = "hidden"/, /trigger\?\.focus\(\)/, /drawer-backdrop/]) {
    assert.match(navigation, evidence);
  }
  assert.match(navigation, /role=\{open \? "dialog"/);
  assert.match(navigation, /aria-modal=\{open \|\| undefined\}/);
  assert.match(navigation, /element\.inert = true/);
  assert.match(navigation, /min-width: 1024px/);
});

test("theme is selected before body rendering to avoid a light-theme flash", () => {
  const layout = read("../app/[locale]/layout.tsx");
  assert.match(layout, /<head><script/);
  assert.match(layout, /boecl-theme/);
  assert.match(layout, /prefers-color-scheme: dark/);
  assert.match(layout, /document\.documentElement\.dataset\.theme/);
  assert.match(layout, /boecl-public-nav-collapsed/);
  assert.match(layout, /document\.documentElement\.dataset\.publicNav/);
});

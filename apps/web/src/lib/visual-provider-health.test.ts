import test from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";

const component = readFileSync(new URL("../components/admin/visual-quality-desk.tsx", import.meta.url), "utf8");
const styles = readFileSync(new URL("../app/globals.css", import.meta.url), "utf8");

test("visual studio exposes fail-closed provider health in every locale", () => {
  for (const locale of ["tr-TR", "en-US", "de-DE", "fr-FR"])
    assert.match(component, new RegExp(`"${locale}"`));
  for (const provider of ["editorial-library", "official-source", "licensed-stock", "generative-ai"])
    assert.match(component, new RegExp(`"${provider}"`));
  assert.match(component, /provider\.canSupplyCandidates/);
  assert.match(component, /provider\.requiresEditorialReview/);
  assert.match(component, /provider\.rightsMetadataRequired/);
  assert.match(component, /owner-activation-required/);
});

test("provider health and public content retain narrow-screen width contracts", () => {
  assert.match(styles, /\.visual-provider-grid\{grid-template-columns:1fr\}/);
  assert.match(styles, /\.rich-article-body :where\(p,li,a,blockquote,figcaption\)/);
  assert.match(styles, /overflow-wrap:anywhere;word-break:break-word/);
});

test("visual studio exposes bounded full-article section art direction", () => {
  assert.match(component, /item\.sectionPlan\.map/);
  assert.match(component, /H\{section\.headingLevel\}/);
  assert.match(component, /section\.visualType/);
  assert.match(component, /section\.typeReason/);
  assert.match(component, /section\.prompt/);
  assert.match(styles, /\.visual-section-plan li\{display:grid;grid-template-columns:38px minmax\(0,1fr\)/);
});

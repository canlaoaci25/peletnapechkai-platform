import assert from "node:assert/strict";
import test from "node:test";
import { buildDiscoveryStructuredData } from "./discovery-structured-data.ts";

test("discovery schema mirrors visible breadcrumbs and collection items", () => {
  const schema = buildDiscoveryStructuredData({ type: "CollectionPage", title: "Sources", url: "https://example.test/en-US/sources/example.org", locale: "en-US", breadcrumbs: [{ name: "Sources", url: "https://example.test/en-US/sources" }, { name: "example.org", url: "https://example.test/en-US/sources/example.org" }], items: [{ name: "Story", url: "https://example.test/en-US/articles/story" }] });
  const graph = schema["@graph"] as Record<string, unknown>[];
  assert.equal(graph[0]["@type"], "CollectionPage");
  assert.equal((graph[0].mainEntity as { numberOfItems: number }).numberOfItems, 1);
  assert.equal(graph[1]["@type"], "BreadcrumbList");
});

test("legal schema remains a WebPage without an invented item list", () => {
  const schema = buildDiscoveryStructuredData({ type: "WebPage", title: "Privacy", url: "https://example.test/en-US/legal/privacy", locale: "en-US", breadcrumbs: [{ name: "Privacy", url: "https://example.test/en-US/legal/privacy" }] });
  const page = (schema["@graph"] as Record<string, unknown>[])[0];
  assert.equal(page["@type"], "WebPage");
  assert.equal(page.mainEntity, undefined);
});

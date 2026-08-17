import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { memberCopy, memberHubCopy } from "../i18n/member-copy.ts";

test("member reading-list copy covers every supported locale", () => {
  assert.deepEqual(Object.keys(memberCopy).sort(), ["de-DE", "en-US", "fr-FR", "tr-TR"]);
  for (const copy of Object.values(memberCopy)) {
    assert.ok(copy.savedTitle.length > 0);
    assert.ok(copy.signInToSave.length > 0);
    assert.ok(copy.savedEmpty.length > 0);
  }
});

test("member hub provides locale-complete navigation and library discovery", () => {
  assert.deepEqual(Object.keys(memberHubCopy).sort(), ["de-DE", "en-US", "fr-FR", "tr-TR"]);
  for (const copy of Object.values(memberHubCopy)) {
    assert.ok(copy.title.length > 3);
    assert.ok(copy.searchPlaceholder.length > 8);
    assert.ok(copy.noResults.length > 8);
  }
  const account = readFileSync(fileURLToPath(new URL("../components/account-dashboard.tsx", import.meta.url)), "utf8");
  assert.match(account, /className="member-hub-nav"/);
  assert.match(account, /type="search"/);
  assert.match(account, /toLocaleLowerCase\(locale\)/);
  assert.match(account, /aria-live="polite"/);
});

test("article save control is a csrf-protected accessible toggle", () => {
  const source = readFileSync(fileURLToPath(new URL("../components/save-article-button.tsx", import.meta.url)), "utf8");
  assert.match(source, /<button[^>]+aria-pressed=\{saved\}/);
  assert.match(source, /"x-csrf-token":token/);
  assert.match(source, /method:saved\?"DELETE":"PUT"/);
  assert.match(source, /role="status"/);
});

test("topic following and personal feed cover every locale", () => {
  for (const copy of Object.values(memberCopy)) {
    assert.ok(copy.follow.length > 2);
    assert.ok(copy.following.length > 2);
    assert.ok(copy.topicsTitle.length > 2);
    assert.ok(copy.feedTitle.length > 2);
    assert.ok(copy.feedEmpty.length > 8);
  }
  const follow = readFileSync(fileURLToPath(new URL("../components/follow-category-button.tsx", import.meta.url)), "utf8");
  const account = readFileSync(fileURLToPath(new URL("../components/account-dashboard.tsx", import.meta.url)), "utf8");
  assert.match(follow, /aria-pressed=\{following\}/);
  assert.match(follow, /x-csrf-token/);
  assert.match(follow, /method:following\?"DELETE":"PUT"/);
  assert.match(account, /personal-discovery/);
  assert.match(account, /followed-topic-grid/);
});

test("reading progress offers a locale-complete return journey", () => {
  for (const copy of Object.values(memberCopy)) {
    assert.ok(copy.continueTitle.length > 3);
    assert.ok(copy.continueAction.length > 3);
    assert.ok(copy.progress.length > 1);
  }
  const tracker = readFileSync(fileURLToPath(new URL("../components/article-engagement.tsx", import.meta.url)), "utf8");
  const surface = readFileSync(fileURLToPath(new URL("../components/continue-reading.tsx", import.meta.url)), "utf8");
  assert.match(tracker, /reading-progress/);
  assert.match(tracker, /x-csrf-token/);
  assert.match(surface, /continue-meter/);
});

test("registration starts an accessible locale-complete interest onboarding", () => {
  const form = readFileSync(fileURLToPath(new URL("../components/account-form.tsx", import.meta.url)), "utf8");
  const onboarding = readFileSync(fileURLToPath(new URL("../components/member-onboarding.tsx", import.meta.url)), "utf8");
  assert.match(form, /account\/onboarding/);
  assert.match(onboarding, /aria-pressed=\{active\}/);
  assert.match(onboarding, /x-csrf-token/);
  assert.match(onboarding, /method:"PUT"/);
  assert.match(onboarding, /current\.length<5/);
  const css = readFileSync(fileURLToPath(new URL("../app/globals.css", import.meta.url)), "utf8");
  assert.match(css, /body:has\(\.consent-banner\) \.member-onboarding>footer/);
});

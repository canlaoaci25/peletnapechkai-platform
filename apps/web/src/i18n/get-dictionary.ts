import "server-only";

import type { Locale } from "./config";

const dictionaries = {
  "tr-TR": () => import("./dictionaries/tr-TR.json").then((module) => module.default),
  "en-US": () => import("./dictionaries/en-US.json").then((module) => module.default),
  "de-DE": () => import("./dictionaries/de-DE.json").then((module) => module.default),
} satisfies Record<Locale, () => Promise<Dictionary>>;

export type Dictionary = typeof import("./dictionaries/tr-TR.json");

export function getDictionary(locale: Locale) {
  return dictionaries[locale]();
}

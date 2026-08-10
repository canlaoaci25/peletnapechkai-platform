import { readFile, access } from "node:fs/promises";
import { constants } from "node:fs";
import path from "node:path";

const root = process.cwd();
const config = JSON.parse(await readFile(path.join(root, "config/supported-locales.json"), "utf8"));
const generatedConfig = JSON.parse(await readFile(path.join(root, "apps/web/src/i18n/supported-locales.generated.json"), "utf8"));
if (JSON.stringify(generatedConfig) !== JSON.stringify(config)) throw new Error("Web locale configuration is stale. Run npm run sync:locales.");
const locales = Object.keys(config.locales ?? {});
if (locales.length === 0 || !locales.includes(config.defaultLocale)) throw new Error("Locale configuration or default locale is invalid.");

for (const locale of locales) {
  if (!/^[a-z]{2}-[A-Z]{2}$/.test(locale)) throw new Error(`Invalid locale code: ${locale}`);
  await access(path.join(root, `apps/web/src/i18n/dictionaries/${locale}.json`), constants.R_OK);
}

const dictionaryEntries = await Promise.all(locales.map(async locale => [locale, JSON.parse(await readFile(path.join(root, `apps/web/src/i18n/dictionaries/${locale}.json`), "utf8"))]));
const flatten = (value, prefix = "") => Object.entries(value).flatMap(([key, child]) => child && typeof child === "object" && !Array.isArray(child) ? flatten(child, `${prefix}${key}.`) : [`${prefix}${key}`]).sort();
const sourceKeys = flatten(dictionaryEntries.find(([locale]) => locale === config.defaultLocale)[1]);
for (const [locale, dictionary] of dictionaryEntries) {
  const keys = flatten(dictionary);
  const missing = sourceKeys.filter(key => !keys.includes(key));
  const extra = keys.filter(key => !sourceKeys.includes(key));
  if (missing.length || extra.length) throw new Error(`${locale} dictionary mismatch. Missing: ${missing.join(", ") || "none"}; extra: ${extra.join(", ") || "none"}`);
}
console.log(`Locale consistency passed for ${locales.length} locales: ${locales.join(", ")}`);

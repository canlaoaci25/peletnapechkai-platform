import { access, readFile } from "node:fs/promises";
import { constants } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const localePattern = /^[a-z]{2}-[A-Z]{2}$/;
const placeholderPattern = /\{[A-Za-z][A-Za-z0-9_.-]*\}/g;

function flatten(value, prefix = "") {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    throw new Error(`${prefix || "Dictionary"} must be an object.`);
  }

  return Object.entries(value).flatMap(([key, child]) => {
    const entryPath = prefix ? `${prefix}.${key}` : key;
    return child && typeof child === "object" && !Array.isArray(child)
      ? flatten(child, entryPath)
      : [[entryPath, child]];
  });
}

function placeholders(value) {
  return [...value.matchAll(placeholderPattern)].map((match) => match[0]).sort();
}

function assertString(locale, key, value) {
  if (typeof value !== "string") {
    throw new Error(`${locale} dictionary value ${key} must be a string.`);
  }
  if (value.trim().length === 0) {
    throw new Error(`${locale} dictionary value ${key} must not be blank.`);
  }
}

export async function validateLocaleConsistency(root) {
  const configPath = path.join(root, "config/supported-locales.json");
  const generatedPath = path.join(root, "apps/web/src/i18n/supported-locales.generated.json");
  const config = JSON.parse(await readFile(configPath, "utf8"));
  const generatedConfig = JSON.parse(await readFile(generatedPath, "utf8"));

  if (JSON.stringify(generatedConfig) !== JSON.stringify(config)) {
    throw new Error("Web locale configuration is stale. Run npm run sync:locales.");
  }

  const locales = Object.keys(config.locales ?? {});
  if (locales.length === 0 || !locales.includes(config.defaultLocale)) {
    throw new Error("Locale configuration or default locale is invalid.");
  }

  for (const locale of locales) {
    if (!localePattern.test(locale)) throw new Error(`Invalid locale code: ${locale}`);
    if (typeof config.locales[locale] !== "string" || config.locales[locale].trim().length === 0) {
      throw new Error(`Locale display name must not be blank: ${locale}`);
    }
    await access(path.join(root, `apps/web/src/i18n/dictionaries/${locale}.json`), constants.R_OK);
  }

  const dictionaries = new Map(await Promise.all(locales.map(async (locale) => [
    locale,
    JSON.parse(await readFile(path.join(root, `apps/web/src/i18n/dictionaries/${locale}.json`), "utf8")),
  ])));
  const sourceEntries = flatten(dictionaries.get(config.defaultLocale));
  const source = new Map(sourceEntries);

  for (const [key, value] of sourceEntries) assertString(config.defaultLocale, key, value);

  for (const locale of locales) {
    const entries = flatten(dictionaries.get(locale));
    const dictionary = new Map(entries);
    const missing = [...source.keys()].filter((key) => !dictionary.has(key)).sort();
    const extra = [...dictionary.keys()].filter((key) => !source.has(key)).sort();
    if (missing.length || extra.length) {
      throw new Error(`${locale} dictionary mismatch. Missing: ${missing.join(", ") || "none"}; extra: ${extra.join(", ") || "none"}`);
    }

    for (const [key, value] of entries) {
      assertString(locale, key, value);
      const expected = placeholders(source.get(key));
      const actual = placeholders(value);
      if (JSON.stringify(actual) !== JSON.stringify(expected)) {
        throw new Error(`${locale} dictionary placeholder mismatch at ${key}. Expected: ${expected.join(", ") || "none"}; actual: ${actual.join(", ") || "none"}`);
      }
    }
  }

  return locales;
}

const isMainModule = process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url);
if (isMainModule) {
  const locales = await validateLocaleConsistency(process.cwd());
  console.log(`Locale consistency passed for ${locales.length} locales: ${locales.join(", ")}`);
}

import { defaultLocale, locales, type Locale } from "../i18n/config.ts";
import { buildLocaleAlternates } from "./locale-alternates-core.ts";

type LocalizedPaths = Partial<Record<Locale, string>>;

export function buildStaticLocaleAlternates(path: string) {
  return buildAvailableLocaleAlternates(
    Object.fromEntries(locales.map((locale) => [locale, `/${locale}${path}`])) as LocalizedPaths,
  );
}

export function buildAvailableLocaleAlternates(paths: LocalizedPaths) {
  return buildLocaleAlternates(locales, defaultLocale, paths);
}

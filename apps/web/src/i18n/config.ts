import localeConfig from "./supported-locales.generated.json";

export type Locale = keyof typeof localeConfig.locales;

export const locales = Object.keys(localeConfig.locales) as Locale[];

export const defaultLocale = localeConfig.defaultLocale as Locale;

export const localeLabels: Record<Locale, string> = localeConfig.locales;

export function hasLocale(value: string): value is Locale {
  return locales.some((locale) => locale === value);
}

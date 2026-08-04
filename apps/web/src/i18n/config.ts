export const locales = ["tr-TR", "en-US", "de-DE"] as const;

export type Locale = (typeof locales)[number];

export const defaultLocale: Locale = "tr-TR";

export const localeLabels: Record<Locale, string> = {
  "tr-TR": "Türkiye — Türkçe",
  "en-US": "United States — English",
  "de-DE": "Deutschland — Deutsch",
};

export function hasLocale(value: string): value is Locale {
  return locales.some((locale) => locale === value);
}

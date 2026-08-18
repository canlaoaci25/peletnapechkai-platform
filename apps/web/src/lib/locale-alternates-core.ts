export function buildLocaleAlternates(
  locales: readonly string[],
  defaultLocale: string,
  paths: Readonly<Record<string, string | undefined>>,
) {
  const languages: Record<string, string> = {};
  for (const locale of locales) {
    const path = paths[locale];
    if (path) languages[locale] = path;
  }
  const fallback = paths[defaultLocale] ?? Object.values(paths).find(Boolean);
  if (fallback) languages["x-default"] = fallback;
  return languages;
}

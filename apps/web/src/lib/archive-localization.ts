export type ArchiveTranslation = { locale: string; slug: string };

export function archiveLanguages(
  collection: string,
  translations: ArchiveTranslation[] | undefined,
) {
  if (!["categories", "tags"].includes(collection) || !translations?.length) return undefined;
  const languages = Object.fromEntries(
    translations.map(({ locale, slug }) => [locale, `/${locale}/${collection}/${slug}`]),
  );
  languages["x-default"] = languages["tr-TR"] ?? Object.values(languages)[0];
  return languages;
}

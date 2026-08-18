import { PublicNavigation } from "@/components/public-navigation";
import { type Locale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicArchiveIndex, type PublicArchiveIndex } from "@/lib/public-api";

type SiteHeaderProps = {
  locale: Locale;
  localeHrefs?: Partial<Record<Locale, string>>;
  homeActive?: boolean;
  archives?: PublicArchiveIndex;
};

export async function SiteHeader({ locale, localeHrefs, homeActive = false, archives: suppliedArchives }: SiteHeaderProps) {
  const [dictionary, archives] = await Promise.all([
    getDictionary(locale),
    suppliedArchives ? Promise.resolve(suppliedArchives) : getPublicArchiveIndex(locale),
  ]);

  return <PublicNavigation
    locale={locale}
    localeHrefs={localeHrefs}
    homeActive={homeActive}
    copy={dictionary.navigation}
    categories={archives.categories.map(({ slug, title, articleCount, parent, children }) => ({ slug, title, articleCount, parent, children }))}
  />;
}

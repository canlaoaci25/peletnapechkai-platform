import { PublicNavigation } from "@/components/public-navigation";
import { type Locale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicArchiveIndex } from "@/lib/public-api";

type SiteHeaderProps = {
  locale: Locale;
  localeHrefs?: Partial<Record<Locale, string>>;
  homeActive?: boolean;
};

export async function SiteHeader({ locale, localeHrefs, homeActive = false }: SiteHeaderProps) {
  const [dictionary, archives] = await Promise.all([
    getDictionary(locale),
    getPublicArchiveIndex(locale),
  ]);

  return <PublicNavigation
    locale={locale}
    localeHrefs={localeHrefs}
    homeActive={homeActive}
    copy={dictionary.navigation}
    categories={archives.categories.map(({ slug, title, articleCount }) => ({ slug, title, articleCount }))}
  />;
}

import Link from "next/link";

import { ThemeToggle } from "@/components/theme-toggle";
import { siteConfig } from "@/config/site";
import type { Locale } from "@/i18n/config";

const searchLabel = { "tr-TR": "Ara", "en-US": "Search", "de-DE": "Suchen", "fr-FR": "Rechercher" };

export function SiteHeader({ locale }: { locale: Locale; localeHrefs?: Partial<Record<Locale, string>> }) {
  return <><header className="site-header">
    <div className="brand-group">
      <Link className="brand" href={`/${locale}`}>{siteConfig.name}</Link>
      <Link className="header-search" href={`/${locale}/search`}>{searchLabel[locale]}</Link>
    </div>
  </header><ThemeToggle locale={locale} /></>;
}

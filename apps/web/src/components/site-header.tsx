import Link from "next/link";
import { siteConfig } from "@/config/site";
import { localeLabels, locales, type Locale } from "@/i18n/config";

export function SiteHeader({ locale, localeHrefs }: { locale: Locale; localeHrefs?: Partial<Record<Locale, string>> }) {
  return <header className="site-header"><Link className="brand" href={`/${locale}`}>{siteConfig.name}</Link><nav aria-label="Language and region" className="locale-nav">{locales.map(item => <Link aria-current={item === locale ? "page" : undefined} className="locale-link" href={localeHrefs?.[item] ?? `/${item}`} key={item}>{localeLabels[item]}</Link>)}</nav></header>;
}

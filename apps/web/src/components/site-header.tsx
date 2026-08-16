import Link from "next/link";

import { AccountActions } from "@/components/account-actions";
import { ThemeToggle } from "@/components/theme-toggle";
import { siteConfig } from "@/config/site";
import { localeLabels, locales, type Locale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicArchiveIndex } from "@/lib/public-api";

type SiteHeaderProps = {
  locale: Locale;
  localeHrefs?: Partial<Record<Locale, string>>;
};

export async function SiteHeader({ locale, localeHrefs }: SiteHeaderProps) {
  const [dictionary, archives] = await Promise.all([
    getDictionary(locale),
    getPublicArchiveIndex(locale),
  ]);
  const copy = dictionary.navigation;
  const categories = archives.categories.slice(0, 6);

  return (
    <>
      <header className="site-header">
        <div className="masthead-topline">
          <details className="site-menu">
            <summary aria-label={copy.menu}>
              <span className="menu-icon" aria-hidden="true"><i /><i /></span>
              <span>{copy.menu}</span>
            </summary>
            <div className="site-menu-panel">
              <p>{copy.sections}</p>
              <nav aria-label={copy.sections}>
                <Link href={`/${locale}`}>{copy.home}</Link>
                <Link href={`/${locale}/topics`}>{copy.allTopics}</Link>
                {categories.map((category) => (
                  <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>
                    {category.title}
                  </Link>
                ))}
              </nav>
            </div>
          </details>

          <Link className="brand" href={`/${locale}`} aria-label={`${siteConfig.name} — ${copy.home}`}>
            {siteConfig.name}
          </Link>

          <nav className="header-actions" aria-label={copy.account}>
            <Link className="header-search" href={`/${locale}/search`}>
              <span aria-hidden="true">⌕</span>{copy.search}
            </Link>
            <AccountActions locale={locale} />
          </nav>
        </div>

        <div className="masthead-navline">
          <nav className="primary-navigation" aria-label={copy.sections}>
            <Link href={`/${locale}`}>{copy.latest}</Link>
            <Link href={`/${locale}/topics`}>{copy.allTopics}</Link>
            {categories.map((category) => (
              <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>
                {category.title}
              </Link>
            ))}
          </nav>

          <details className="locale-menu">
            <summary>{locale.split("-")[0].toUpperCase()}<span className="sr-only"> — {copy.language}</span></summary>
            <nav aria-label={copy.language}>
              {locales.map((supportedLocale) => (
                <Link
                  key={supportedLocale}
                  href={localeHrefs?.[supportedLocale] ?? `/${supportedLocale}`}
                  hrefLang={supportedLocale}
                  aria-current={supportedLocale === locale ? "page" : undefined}
                >
                  {localeLabels[supportedLocale]}
                </Link>
              ))}
            </nav>
          </details>
        </div>
      </header>
      <ThemeToggle locale={locale} />
    </>
  );
}

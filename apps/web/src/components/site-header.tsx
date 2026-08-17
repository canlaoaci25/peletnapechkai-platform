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
  homeActive?: boolean;
};

export async function SiteHeader({ locale, localeHrefs, homeActive = false }: SiteHeaderProps) {
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
              <div className="site-menu-heading">
                <p>{copy.sections}</p>
                <span>{copy.menuDescription}</span>
              </div>
              <nav aria-label={copy.sections}>
                <Link href={`/${locale}`} aria-current={homeActive ? "page" : undefined}>{copy.home}</Link>
                <Link href={`/${locale}/search`}>{copy.search}</Link>
                <Link href={`/${locale}/topics`}>{copy.allTopics}</Link>
                {categories.map((category) => (
                  <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>
                    <span>{category.title}</span><small>{category.articleCount}</small>
                  </Link>
                ))}
              </nav>
            </div>
          </details>

          <div className="brand-lockup">
            <Link className="brand" href={`/${locale}`} aria-label={`${siteConfig.name} — ${copy.home}`}>
              {siteConfig.name}
            </Link>
            <span>{copy.publicationPromise}</span>
          </div>

          <nav className="header-actions" aria-label={copy.account}>
            <Link className="header-search" href={`/${locale}/search`}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m15.5 15.5 5 5"/></svg>
              <span>{copy.search}</span>
            </Link>
            <AccountActions locale={locale} />
          </nav>
        </div>

        <div className="masthead-navline">
          <nav className="primary-navigation" aria-label={copy.sections}>
            <Link href={`/${locale}`} aria-current={homeActive ? "page" : undefined}>{copy.latest}</Link>
            <Link href={`/${locale}/topics`}>{copy.allTopics}</Link>
            {categories.map((category) => (
              <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>
                <span>{category.title}</span><small aria-label={`${category.articleCount}`}>{category.articleCount}</small>
              </Link>
            ))}
          </nav>

          <Link className="trust-link" href={`/${locale}/sources`}>
            <span aria-hidden="true">◆</span>{copy.sources}
          </Link>

          <details className="locale-menu">
            <summary>{locale.split("-")[0].toUpperCase()}<span className="sr-only"> — {copy.language}</span></summary>
            <nav aria-label={copy.language}>
              {locales.map((supportedLocale) => localeHrefs && !localeHrefs[supportedLocale] ? (
                <span className="locale-unavailable" key={supportedLocale} aria-disabled="true">
                  {localeLabels[supportedLocale]}<small>{copy.translationUnavailable}</small>
                </span>
              ) : (
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
          <ThemeToggle locale={locale} />
        </div>
      </header>
    </>
  );
}

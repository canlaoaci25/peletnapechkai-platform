import Link from "next/link";
import { notFound } from "next/navigation";

import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale, localeLabels } from "@/i18n/config";
import { siteConfig } from "@/config/site";
import { SiteHeader } from "@/components/site-header";
import { getPublishedArticles } from "@/lib/public-api";

export const dynamic = "force-dynamic";

export default async function LocaleHome({ params }: PageProps<"/[locale]">) {
  const { locale } = await params;

  if (!hasLocale(locale)) {
    notFound();
  }

  const dictionary = await getDictionary(locale);
  const articles = await getPublishedArticles(locale);
  const topics = [
    dictionary.navigation.technology,
    dictionary.navigation.ai,
    dictionary.navigation.science,
    dictionary.navigation.software,
    dictionary.navigation.mobile,
  ];

  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />

      <main>
        <section className="hero">
          <p className="eyebrow">{dictionary.home.eyebrow}</p>
          <h1>{dictionary.home.title}</h1>
          <p className="hero-description">{dictionary.home.description}</p>
          <div className="status-pill">
            <span aria-hidden="true" />
            {dictionary.home.status}
          </div>
        </section>

        <section className="content-grid" aria-labelledby="topics-title">
          <div>
            <p className="section-kicker">01</p>
            <h2 id="topics-title">{dictionary.home.topicsTitle}</h2>
            <ul className="topic-list">
              {topics.map((topic) => (
                <li key={topic}>{topic}</li>
              ))}
            </ul>
          </div>
          <aside className="principle-card">
            <p className="section-kicker">02</p>
            <h2>{dictionary.home.principleTitle}</h2>
            <p>{dictionary.home.principle}</p>
          </aside>
        </section>
        <section className="latest-section" aria-labelledby="latest-title">
          <p className="section-kicker">03</p><h2 id="latest-title">{dictionary.home.latestTitle}</h2>
          {articles.length === 0 ? <p className="muted">{dictionary.home.noArticles}</p> : <div className="article-cards">{articles.map(article => <article className="public-card" key={article.slug}><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p><div><time dateTime={article.publishedAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"long"}).format(new Date(article.publishedAt))}</time><Link href={`/${locale}/articles/${article.slug}`}>{dictionary.home.readArticle} →</Link></div></article>)}</div>}
        </section>
        <section className="search-callout"><h2>{dictionary.search.title}</h2><form action={`/${locale}/search`} role="search"><label htmlFor="home-search">{dictionary.search.label}</label><div><input id="home-search" name="q" minLength={2} required/><button>{dictionary.search.submit}</button></div></form></section>
      </main>

      <footer className="site-footer">
        <span>© {new Date().getUTCFullYear()} {siteConfig.legalName}</span>
        <span>{localeLabels[locale]}</span>
      </footer>
    </div>
  );
}

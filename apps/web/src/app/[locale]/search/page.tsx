import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale } from "@/i18n/config";
import { getPublicArchiveIndex, getPublishedArticles, searchPublishedArticles } from "@/lib/public-api";
import { focalPointStyle } from "@/lib/focal-point";

export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  robots: { index: false, follow: true },
};

export default async function SearchPage({
  params,
  searchParams,
}: PageProps<"/[locale]/search">) {
  const { locale } = await params;
  if (!hasLocale(locale)) notFound();
  const queryValue = (await searchParams).q;
  const query = typeof queryValue === "string" ? queryValue.trim() : "";
  const [dictionary, results, archive, latest] = await Promise.all([
    getDictionary(locale),
    searchPublishedArticles(locale, query),
    getPublicArchiveIndex(locale),
    getPublishedArticles(locale, 4),
  ]);
  const formatDate = (value: string) => new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(value));
  const searchCopy = dictionary.search;
  const discoveryCategories = archive.categories
    .filter(category => category.articleCount > 0)
    .sort((left, right) => right.articleCount - left.articleCount)
    .slice(0, 6);
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />
      <main className="search-page" id="main-content">
        <header className="search-hero">
          <p className="section-kicker">{searchCopy.eyebrow}</p>
          <h1>{searchCopy.title}</h1>
          <p>{searchCopy.description}</p>
        </header>
        <form action={`/${locale}/search`} role="search">
          <label htmlFor="search-query">{dictionary.search.label}</label>
          <div>
            <input
              id="search-query"
              name="q"
              type="search"
              enterKeyHint="search"
              defaultValue={query}
              minLength={2}
              maxLength={120}
              required
            />
            <button type="submit">{dictionary.search.submit}</button>
          </div>
        </form>
        {query.length >= 2 && (
          <section aria-live="polite" aria-labelledby="results-title">
            <h2 id="results-title">
              {searchCopy.results.replace(
                "{count}",
                String(results.length),
              )}
            </h2>
            {results.length === 0 ? (
              <div className="search-empty"><strong>{searchCopy.emptyTitle}</strong><p>{searchCopy.empty}</p><a href="#search-discovery">{searchCopy.exploreTopics} ↓</a></div>
            ) : (
              <ul className="search-results">
                {results.map((article) => (
                  <li key={article.slug}>
                    <article className={article.cover ? undefined : "search-result-no-cover"}>
                      {article.cover && <Link className="search-result-cover" href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><Image src={article.cover.url} alt="" fill sizes="(max-width: 640px) calc(100vw - 28px), 240px" style={focalPointStyle(article.cover)} /></Link>}
                      <div className="search-result-copy">
                        <div className="search-result-taxonomy">
                          {article.categories?.map(category => <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>{category.name}</Link>)}
                        </div>
                        <h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3>
                        <p>{article.summary}</p>
                        <footer><span>{article.type}</span><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time>{article.sourceCount ? <span>{searchCopy.sources.replace("{count}", String(article.sourceCount))}</span> : null}</footer>
                      </div>
                    </article>
                  </li>
                ))}
              </ul>
            )}
          </section>
        )}
        <section className="search-discovery" id="search-discovery" aria-labelledby="search-discovery-title">
          <header>
            <div><p className="section-kicker">{searchCopy.discoveryEyebrow}</p><h2 id="search-discovery-title">{searchCopy.discoveryTitle}</h2></div>
            <p>{searchCopy.discoveryDescription}</p>
          </header>
          {discoveryCategories.length > 0 && <nav aria-label={searchCopy.discoveryTitle} className="search-topic-grid">
            {discoveryCategories.map(category => <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}><span>{category.title}</span><small>{searchCopy.stories.replace("{count}", String(category.articleCount))}</small></Link>)}
          </nav>}
          {latest.length > 0 && <div className="search-latest">
            <h3>{searchCopy.latestTitle}</h3>
            <ol>{latest.map(article => <li key={article.slug}><Link href={`/${locale}/articles/${article.slug}`}><span>{article.title}</span><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></Link></li>)}</ol>
          </div>}
        </section>
      </main>
    </div>
  );
}

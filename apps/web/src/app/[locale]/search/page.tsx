import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale } from "@/i18n/config";
import { searchPublishedArticles } from "@/lib/public-api";

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
  const [dictionary, results] = await Promise.all([
    getDictionary(locale),
    searchPublishedArticles(locale, query),
  ]);
  const formatDate = (value: string) => new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(value));
  const searchCopy = dictionary.search;
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
              <div className="search-empty"><strong>{searchCopy.emptyTitle}</strong><p>{searchCopy.empty}</p></div>
            ) : (
              <ul className="search-results">
                {results.map((article) => (
                  <li key={article.slug}>
                    <article className={article.cover ? undefined : "search-result-no-cover"}>
                      {article.cover && <Link className="search-result-cover" href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><Image src={article.cover.url} alt="" fill sizes="(max-width: 640px) calc(100vw - 28px), 240px" /></Link>}
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
      </main>
    </div>
  );
}

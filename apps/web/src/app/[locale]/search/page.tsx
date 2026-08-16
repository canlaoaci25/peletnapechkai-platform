import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale } from "@/i18n/config";
import { searchPublishedArticles } from "@/lib/public-api";

export const dynamic = "force-dynamic";

export default async function SearchPage({ params, searchParams }: PageProps<"/[locale]/search">) {
  const { locale } = await params;
  if (!hasLocale(locale)) notFound();
  const queryValue = (await searchParams).q;
  const query = typeof queryValue === "string" ? queryValue.trim() : "";
  const [dictionary, results] = await Promise.all([getDictionary(locale), searchPublishedArticles(locale, query)]);
  return <div className="site-shell"><SiteHeader locale={locale}/><main className="search-page" id="main-content"><h1>{dictionary.search.title}</h1><form action={`/${locale}/search`} role="search"><label htmlFor="search-query">{dictionary.search.label}</label><div><input id="search-query" name="q" type="search" enterKeyHint="search" defaultValue={query} minLength={2} required/><button type="submit">{dictionary.search.submit}</button></div></form>{query.length >= 2 && <section aria-live="polite" aria-labelledby="results-title"><h2 id="results-title">{dictionary.search.results.replace("{count}", String(results.length))}</h2>{results.length === 0 ? <p>{dictionary.search.empty}</p> : <ul className="search-results">{results.map(article => <li key={article.slug}><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p></li>)}</ul>}</section>}</main></div>;
}

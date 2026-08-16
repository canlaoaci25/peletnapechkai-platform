import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { archiveCopy } from "@/i18n/archive-copy";
import { hasLocale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { archiveLanguages } from "@/lib/archive-localization";
import { getPublicArchive } from "@/lib/public-api";

export const dynamic = "force-dynamic";
const collections = ["categories", "tags", "authors"] as const;
type Props = {
  params: Promise<{ locale: string; collection: string; slug: string }>;
};
function isCollection(value: string): value is (typeof collections)[number] {
  return collections.includes(value as (typeof collections)[number]);
}
export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, collection, slug } = await params;
  if (!hasLocale(locale) || !isCollection(collection)) return {};
  const archive = await getPublicArchive(locale, collection, slug);
  if (!archive) return {};
  return {
    title: archive.title,
    description: archive.description ?? archiveCopy[locale].description,
    alternates: {
      canonical: `/${locale}/${collection}/${slug}`,
      languages: archiveLanguages(collection, archive.translations),
    },
  };
}
export default async function ArchivePage({ params }: Props) {
  const { locale, collection, slug } = await params;
  if (!hasLocale(locale) || !isCollection(collection)) notFound();
  const [archive, dictionary] = await Promise.all([
    getPublicArchive(locale, collection, slug),
    getDictionary(locale),
  ]);
  if (!archive) notFound();
  const copy = archiveCopy[locale];
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />
      <main className="archive-page">
        <nav className="archive-breadcrumbs" aria-label="Breadcrumb"><Link href={`/${locale}`}>{dictionary.navigation.home}</Link><span aria-hidden="true">/</span>{collection === "categories" && <><Link href={`/${locale}/topics`}>{copy.allTopics}</Link><span aria-hidden="true">/</span></>}<span aria-current="page">{archive.title}</span></nav>
        <header className="archive-authority-hero">
          <div><p className="section-kicker">{copy[collection]} · {archive.articleCount} {copy.stories}</p><h1>{archive.title}</h1>{archive.description && <p className="archive-description">{archive.description}</p>}</div>
          {archive.typeCounts.length > 0 && <ul className="archive-type-counts" aria-label={copy.description}>{archive.typeCounts.map(item => <li key={item.type}><strong>{item.count}</strong><span>{item.type}</span></li>)}</ul>}
        </header>
        {collection === "categories" && archive.relatedCategories.length > 0 && <nav className="archive-related" aria-label={copy.explore}><strong>{copy.explore}</strong>{archive.relatedCategories.map(item => <Link key={item.slug} href={`/${locale}/categories/${item.slug}`}>{item.title}<span>{item.articleCount}</span></Link>)}</nav>}
        {archive.articles.length === 0 ? (
          <p className="muted">{copy.empty}</p>
        ) : (
          <section className="archive-stream" aria-labelledby="archive-latest"><div className="archive-stream-heading"><p className="section-kicker">BOECL</p><h2 id="archive-latest">{copy.latest}</h2></div><div className="archive-lead-grid">
            {archive.articles.map((article, index) => (
              <article className={index === 0 ? "archive-lead" : "public-card"} key={article.slug}>
                {article.cover && <Link className="archive-card-cover" href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><Image src={article.cover.url} alt="" fill sizes="(max-width: 700px) 100vw, 33vw" /></Link>}
                <div className="archive-card-copy"><p className="section-kicker">{article.type}</p>
                <h2>
                  <Link href={`/${locale}/articles/${article.slug}`}>
                    {article.title}
                  </Link>
                </h2>
                <p>{article.summary}</p>
                <time dateTime={article.publishedAt}>
                  {new Intl.DateTimeFormat(locale, {
                    dateStyle: "long",
                  }).format(new Date(article.publishedAt))}
                </time></div>
              </article>
            ))}
          </div></section>
        )}
      </main>
    </div>
  );
}

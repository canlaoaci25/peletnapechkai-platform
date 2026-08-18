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
import { FollowCategoryButton } from "@/components/follow-category-button";
import { buildPlanningClusters, planningHubCopy, planningHubSlugs } from "@/lib/planning-authority-hub";
import { absoluteUrl } from "@/lib/site-url";
import { focalPointStyle } from "@/lib/focal-point";

export const dynamic = "force-dynamic";
const collections = ["categories", "tags", "authors"] as const;
type Props = {
  params: Promise<{ locale: string; collection: string; slug: string }>;
  searchParams: Promise<{ page?: string | string[] }>;
};
function pageNumber(value:string|string[]|undefined){const raw=Array.isArray(value)?value[0]:value;const parsed=Number.parseInt(raw??"1",10);return Number.isFinite(parsed)&&parsed>0?parsed:1}
function isCollection(value: string): value is (typeof collections)[number] {
  return collections.includes(value as (typeof collections)[number]);
}
export async function generateMetadata({ params,searchParams }: Props): Promise<Metadata> {
  const { locale, collection, slug } = await params;
  const page=pageNumber((await searchParams).page);
  if (!hasLocale(locale) || !isCollection(collection)) return {};
  const archive = await getPublicArchive(locale, collection, slug,page);
  if (!archive) return {};
  return {
    title: archive.title,
    description: archive.description ?? archiveCopy[locale].description,
    alternates: {
      canonical: `/${locale}/${collection}/${slug}${page>1?`?page=${page}`:""}`,
      languages: archiveLanguages(collection, archive.translations),
    },
  };
}
export default async function ArchivePage({ params,searchParams }: Props) {
  const { locale, collection, slug } = await params;
  const page=pageNumber((await searchParams).page);
  if (!hasLocale(locale) || !isCollection(collection)) notFound();
  const [archive, dictionary] = await Promise.all([
    getPublicArchive(locale, collection, slug,page),
    getDictionary(locale),
  ]);
  if (!archive) notFound();
  const copy = archiveCopy[locale];
  const isPlanningHub = collection === "categories" && planningHubSlugs[locale] === slug && page === 1;
  const hubCopy = planningHubCopy[locale];
  const planningClusters = isPlanningHub ? buildPlanningClusters(locale, archive.articles) : [];
  const breadcrumbItems = [
    { name: dictionary.navigation.home, url: `/${locale}` },
    ...(collection === "categories" ? [{ name: copy.allTopics, url: `/${locale}/topics` }] : []),
    ...(archive.parent ? [{ name: archive.parent.title, url: `/${locale}/categories/${archive.parent.slug}` }] : []),
    { name: archive.title, url: `/${locale}/${collection}/${slug}` },
  ];
  const breadcrumbSchema = { "@context": "https://schema.org", "@type": "BreadcrumbList", itemListElement: breadcrumbItems.map((item, index) => ({ "@type": "ListItem", position: index + 1, name: item.name, item: item.url })) };
  const collectionSchema = isPlanningHub ? { "@context":"https://schema.org", "@type":"CollectionPage", name:archive.title, description:archive.description, url:absoluteUrl(`/${locale}/${collection}/${slug}`), mainEntity:{ "@type":"ItemList", numberOfItems:archive.articleCount, itemListElement:archive.articles.map((article,index)=>({"@type":"ListItem",position:index+1,url:absoluteUrl(`/${locale}/articles/${article.slug}`),name:article.title})) } } : null;
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />
      <main id="main-content" className="archive-page">
        <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(breadcrumbSchema).replace(/</g, "\\u003c") }} />
        {collectionSchema && <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(collectionSchema).replace(/</g, "\\u003c") }} />}
        <nav className="archive-breadcrumbs" aria-label="Breadcrumb"><Link href={`/${locale}`}>{dictionary.navigation.home}</Link><span aria-hidden="true">/</span>{collection === "categories" && <><Link href={`/${locale}/topics`}>{copy.allTopics}</Link><span aria-hidden="true">/</span></>}{archive.parent && <><Link href={`/${locale}/categories/${archive.parent.slug}`}>{archive.parent.title}</Link><span aria-hidden="true">/</span></>}<span aria-current="page">{archive.title}</span></nav>
        <header className="archive-authority-hero">
          <div><p className="section-kicker">{copy[collection]} · {archive.articleCount} {copy.stories}</p><h1>{archive.title}</h1>{archive.description && <p className="archive-description">{archive.description}</p>}{collection === "categories" && <FollowCategoryButton locale={locale} slug={slug}/>}</div>
          {archive.typeCounts.length > 0 && <ul className="archive-type-counts" aria-label={copy.description}>{archive.typeCounts.map(item => <li key={item.type}><strong>{item.count}</strong><span>{item.type}</span></li>)}</ul>}
        </header>
        {isPlanningHub && planningClusters.length > 0 && <section className="planning-authority" aria-labelledby="planning-authority-title">
          <header><div><p className="section-kicker">{hubCopy.eyebrow}</p><h2 id="planning-authority-title">{hubCopy.title}</h2></div><p>{hubCopy.intro}</p></header>
          <aside className="planning-evidence-note"><strong>{hubCopy.evidence}</strong><p>{hubCopy.evidenceNote}</p></aside>
          <div className="planning-cluster-heading"><p className="section-kicker">BOECL</p><h3>{hubCopy.guide}</h3><p>{hubCopy.guideIntro}</p></div>
          <div className="planning-clusters">{planningClusters.map((cluster,index)=><article key={cluster.id} id={`intent-${cluster.id}`}>
            <span className="planning-cluster-number">0{index+1}</span><h3>{cluster.title}</h3><p>{cluster.description}</p>
            <ol>{cluster.articles.map(article=><li key={article.slug}><Link href={`/${locale}/articles/${article.slug}`}><span>{article.title}</span><small>{article.sourceCount??0} {hubCopy.sources}{(article.reviewedSourceCount??0)>0?` · ${article.reviewedSourceCount} ${hubCopy.reviewed}`:""}</small></Link></li>)}</ol>
            <Link className="planning-cluster-open" href={`#${cluster.articles[0].slug}`}>{hubCopy.open} <span aria-hidden="true">↓</span></Link>
          </article>)}</div>
        </section>}
        {collection === "categories" && archive.relatedCategories.length > 0 && <nav className="archive-related" aria-label={copy.explore}><strong>{copy.explore}</strong>{archive.relatedCategories.map(item => <Link key={item.slug} href={`/${locale}/categories/${item.slug}`}>{item.title}<span>{item.articleCount}</span></Link>)}</nav>}
        {archive.articles.length === 0 ? (
          <p className="muted">{copy.empty}</p>
        ) : (
          <section className="archive-stream" aria-labelledby="archive-latest"><div className="archive-stream-heading"><p className="section-kicker">BOECL</p><h2 id="archive-latest">{copy.latest}</h2></div><div className="archive-lead-grid">
            {archive.articles.map((article, index) => (
              <article id={article.slug} className={index === 0 ? "archive-lead" : "public-card"} key={article.slug}>
                {article.cover && <Link className="archive-card-cover" href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><Image src={article.cover.url} alt="" fill sizes="(max-width: 700px) 100vw, 33vw" style={focalPointStyle(article.cover)} /></Link>}
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
        {archive.totalPages>1&&<nav className="archive-pagination" aria-label={`${copy.page} ${archive.page} ${copy.of} ${archive.totalPages}`}>
          {archive.page>1?<Link rel="prev" href={`/${locale}/${collection}/${slug}${archive.page===2?"":`?page=${archive.page-1}`}`}>← {copy.previous}</Link>:<span/>}
          <strong>{copy.page} {archive.page} <span>{copy.of} {archive.totalPages}</span></strong>
          {archive.page<archive.totalPages?<Link rel="next" href={`/${locale}/${collection}/${slug}?page=${archive.page+1}`}>{copy.next} →</Link>:<span/>}
        </nav>}
      </main>
    </div>
  );
}
